using UnityEngine;
using System.Collections;
using System.Linq;
using ServerSideCalculations.Characters;
using ServerSideCalculations.Networking;

public class SceneCharacter3D : SceneCharacter
{
    public CharacterController Controller { get; set; }
    public GameObject[] body_pieces;
    public GameObject visor;
    public GameObject guns;
    public GameObject head;

    public GameObject walker;
    private SceneCharacter3D walker_char;

    public WalkerPhysics state;
    public DampenedSpring crouch_spring;

    public Leg left_leg;
    public Leg right_leg;

    public AudioClip damage_sound;

    public Vector2 targetDirection;
    public float PitchAngle { get; set; }
    public Vector2 headRot;

    //private float bounce_impulse;
    //private float bounce_factor;

    public float head_rest_y;
    private bool will_jump;

    public int walking = 0;

    public float head_height;
    //public float jump_factor = 1300f;
    //public float spring_body_conversion = 100f;

    //public float spring_min_liftoff_factor = 8.5f;
    //public float spring_max_liftoff_factor = 9f;



    public bool is_on_the_ground = false;

    public Vector3 move = Vector3.zero;
    
    private Material my_material;
    private MaterialPropertyBlock my_property_block;

    
    
    // Use this for initialization
    public void Start() {
        Controller = GetComponent<CharacterController>();

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>()) {
            GetComponent<Rigidbody>().freezeRotation = true;
        }
        //TODO: fix the nightmare that is the rigging of the player character
        targetDirection = new Vector2(270f, 270f); // uhh yeah i measured this heh heh
        head_rest_y = head.transform.localPosition.z;
        state = new WalkerPhysics(0, walker.transform, Vector3.zero, Vector3.zero, transform.localEulerAngles.y);
        //crouch_spring = new DampenedSpring(crouch_factor);
        my_material = body_pieces[0].GetComponent<Renderer>().material;
        my_property_block = new MaterialPropertyBlock();
        foreach(GameObject g in body_pieces) {
            g.GetComponent<Renderer>().material = my_material;
        }
        
    }

    public void recolor_walker(Color c) {
        my_material.color = c;
        return;
    }

    public void was_hit(float power, float max_power) {
        state.shield -= power;
        var glow = power / max_power;
        StartCoroutine(this.do_glow(glow));
        GameClient.PlayClipAt(damage_sound, transform.position);
    }

    private IEnumerator do_glow(float intensity) {
        for (float f = 1f; f >= 0; f -= .1f) {
            var c = Color.Lerp(Color.black, Color.white * intensity, f);
            my_material.SetColor(Shader.PropertyToID("_EmissionColor"), c);
            yield return new WaitForSeconds(.001f); ;
        }
    }

    private void recolor_object(GameObject go, Color c) {
        Mesh m = Instantiate(go.GetComponent<SkinnedMeshRenderer>().sharedMesh);
        var colors = from n in Enumerable.Range(0, m.vertices.Length) select c;
        m.colors = colors.ToArray();
        go.GetComponent<SkinnedMeshRenderer>().sharedMesh = m;
    }

    public override bool MissingController ()
    {
        return Controller == null;
    }


    public override void ExecuteControlCommand (ControlCommand control)
    {    
        ////Debug.Log ("Executing control command " + control.ToString ());
        
        Move (control.Forward, control.Turn, control.Duration, control.Jump);
        Look (control.LookHorz, control.LookVert);
    }

    /**
    * Move the player's position
    */
    public void Move (float forward, float turn, float duration, bool jump)
    {
        if (Controller == null) {
            return;
        }
        
        LegUpdate(forward, turn);

        float bob_factor = 0f;
        if (walking != 0) {
            bob_factor = Mathf.Abs(left_leg.walk_seq_step) / 300f;
        }
        
        this.BaseCharacter.CurrentHealth = state.shield;

        head_height = state.elevation + (headRot.y * -.01f);

        var temp = head.transform.position;
        temp.y = head_height + transform.position.y + .35f;
        head.transform.position = temp;
        
        var previous_pos = state.pos;

        state.on_ground = right_leg.is_on_ground() || left_leg.is_on_ground() || state.momentum.y > 0;
        is_on_the_ground = state.on_ground;
        InputTuple i = new InputTuple(forward, turn, jump);
        state.integrate(Time.fixedTime, duration, i);
        
        transform.localEulerAngles = new Vector3(0, state.angle, 0);

        var tp = head.transform.position;
        tp.y -= .5f;
        Debug.DrawLine(tp, tp + (state.velocity * 100), Color.red);
        Debug.DrawLine(tp, tp + (state.accel * 10), Color.cyan);
        Debug.DrawLine(tp, tp + (state.momentum * .1f), Color.black);
        move = (previous_pos - state.pos);

        if (move.magnitude > 0f) {
            /*CollisionFlags flags =*/ Controller.Move(move * -1f);
        }
    }

    void OnControllerColliderHit(ControllerColliderHit h) {
        state.react_to_contact(h, move);
    }

    public void LegUpdate(float vert, float turn) {
        if (vert > 0 && walking == 0) {
            walking = 1;
            right_leg.up_step = !left_leg.up_step;

            left_leg.walking = true;
            right_leg.walking = true;
        }
        if (vert < 0 && walking == 0) {
            walking = -1;
            left_leg.up_step = !right_leg.up_step;

            left_leg.walking = true;
            right_leg.walking = true;
        }

        if (vert == 0 && walking != 0) {
            walking = 0;
            left_leg.walking = false;
            right_leg.walking = false;
        }

        left_leg.direction = vert;
        right_leg.direction = vert;

        left_leg.ride_height = head_height;
        right_leg.ride_height = head_height;

        var xz_vel = state.velocity;
        xz_vel.y = 0;
        //xz_vel.x += turn * .2f;
        left_leg.speed = xz_vel.magnitude;
        right_leg.speed = xz_vel.magnitude;
    }

    public Vector2 clampInDegrees = new Vector2(240, 60);

    // Turn/tilt the player's head as needed
    public void Look (float yawAmount, float pitchAmount)
    {
        var _smoothMouse = new Vector2(yawAmount, pitchAmount);
        // Allow the script to clamp based on a desired target value.
        var targetOrientation = Quaternion.Euler(targetDirection);

        // Find the absolute mouse movement value from point zero.
        headRot += _smoothMouse;

        // Clamp and apply the local x value first, so as not to be affected by world transforms.
        if (clampInDegrees.x < 360)
            headRot.x = Mathf.Clamp(headRot.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

        // Then clamp and apply the global y value.
        if (clampInDegrees.y < 360)
            headRot.y = Mathf.Clamp(headRot.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

        // TODO: Rigging
        var xRotation = Quaternion.AngleAxis(headRot.x, targetOrientation * Vector3.right);
        var yRotation = Quaternion.AngleAxis(headRot.y, targetOrientation * Vector3.up);
        
        head.transform.localRotation = xRotation;
        head.transform.localRotation *= yRotation;
        head.transform.localRotation *= targetOrientation;
    }

    public override float GetCurrentSpeed ()
    {
        return state.velocity.magnitude;
        //if (Controller != null) {
        //    return Controller.velocity.magnitude;
        //} else {
        //    return 0;
        //}
    }

    /// <summary>
    /// Gets the current ObjectState to store for the server
    /// </summary>
    /// <returns>The current state.</returns>
    public ObjectState GetCurrentState() {
        return new ObjectState(
            BaseCharacter.Id,
            transform.position,
            state.angle,
            head.transform.localRotation,
            state.velocity,
            state.crouch,
            state.stance,
            walking);
    }
}
