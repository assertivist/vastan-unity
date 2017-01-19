using UnityEngine;
using System.Collections;
using ServerSideCalculations.Characters;

public class SceneCharacter3D : SceneCharacter
{
	public CharacterController Controller { get; set; }
    public GameObject head;

    public GameObject walker;
    private SceneCharacter3D walker_char;

    public WalkerPhysics state;
    public DampenedSpring crouch_spring;

    public Leg left_leg;
    public Leg right_leg;

    public Vector2 targetDirection;
	public float PitchAngle { get; set; }
    private Vector2 _headRot;

    private const float bob_amount = .05f;
    private const float crouch_dist = .0083f;
    private float base_crouch_factor;
    public float crouch_factor = 0f;

    private float bounce_impulse;
    private float bounce_factor;

    private float head_rest;

    int walking = 0;


    // Use this for initialization
    public void Start() {
        Controller = GetComponent<CharacterController>();

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>()) {
            GetComponent<Rigidbody>().freezeRotation = true;
        }

        targetDirection = head.transform.localRotation.eulerAngles;
        targetDirection.y += 90;
        /*
        var legs = walker.GetComponents<Leg>();
        left_leg = legs[0];
        right_leg = legs[1];
        */
        head_rest = head.transform.localPosition.z;

        state = new WalkerPhysics(155f, walker.transform.position, Vector3.zero, Vector3.zero, 0f);
        var spring_rest = head.transform.position.y;
        crouch_spring = new DampenedSpring(crouch_factor, crouch_factor);
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
		if (jump) {
			//((Character)this).AttemptJump ();
		}
		
		if (Controller == null) {
			return;
		}
        crouch_factor -= base_crouch_factor;

        var crouch_dt = 5f * duration;
        if (jump) {
            crouch_factor = Mathf.Min(1.0f - bob_amount, crouch_factor + crouch_dt);
        }
        else {
            crouch_factor = Mathf.Max(0f, crouch_factor - crouch_dt);
        }

        var vert = forward;

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

        if (walking != 0) {
            var bob_factor = Mathf.Abs(left_leg.walk_seq_step) / 300f;
            crouch_factor = Mathf.Min(1.0f, crouch_factor + (bob_amount * bob_factor));
        }

        crouch_factor += base_crouch_factor;
        crouch_factor = crouch_spring.pos;

        left_leg.direction = vert;
        right_leg.direction = vert;

        left_leg.crouch_factor = crouch_factor;
        right_leg.crouch_factor = crouch_factor;

        var temp = head.transform.localPosition;
        temp.z = head_rest - crouch_factor * crouch_dist;
        head.transform.localPosition = temp;

        var fwd = Vector3.forward;
        fwd = this.transform.TransformDirection(fwd);
        
        var previous_pos = state.pos;
        if (!state.on_ground && Controller.isGrounded) {
            // Just landed
            Debug.Log(this.name + " Landed");
            crouch_spring.vel = -state.velocity.y * 100f;

        }
        state.on_ground = Controller.isGrounded;
        state.forward_vector = fwd;
        InputTuple i = new InputTuple(forward, turn);
        state.integrate(Time.fixedTime - Time.deltaTime, Time.deltaTime, i);
        
        transform.localEulerAngles = new Vector3(0, state.angle, 0);

        var tp = head.transform.position;
        tp.y -= .5f;
        Debug.DrawLine(tp, tp + (state.velocity * 10), Color.red);

        Controller.Move((previous_pos - state.pos) * -1f);
	}

    public Vector2 clampInDegrees = new Vector2(194, 84);

    // Turn/tilt the player's head as needed
    public void Look (float yawAmount, float pitchAmount)
	{
        var _smoothMouse = new Vector2(yawAmount, pitchAmount);
        // Allow the script to clamp based on a desired target value.
        var targetOrientation = Quaternion.Euler(targetDirection);

        // Find the absolute mouse movement value from point zero.
        _headRot += _smoothMouse;

        // Clamp and apply the local x value first, so as not to be affected by world transforms.
        if (clampInDegrees.x < 360)
            _headRot.x = Mathf.Clamp(_headRot.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

        // Then clamp and apply the global y value.
        if (clampInDegrees.y < 360)
            _headRot.y = Mathf.Clamp(_headRot.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

        var xRotation = Quaternion.AngleAxis(_headRot.x, targetOrientation * Vector3.right);
        var yRotation = Quaternion.AngleAxis(_headRot.y, targetOrientation * Vector3.up);
        
        head.transform.localRotation = xRotation;
        head.transform.localRotation *= yRotation;
        head.transform.localRotation *= targetOrientation;

        base_crouch_factor = _headRot.y / clampInDegrees.y * .8f;
        //Debug.Log(base_crouch_factor);
    }

	public override float GetCurrentSpeed ()
	{
		if (Controller != null) {
			return Controller.velocity.magnitude;
		} else {
			return 0;
		}
	}
}
