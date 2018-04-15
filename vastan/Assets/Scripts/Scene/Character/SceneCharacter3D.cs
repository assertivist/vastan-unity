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
    private Vector2 _headRot;

    private const float bob_amount = .12f;
    public const float crouch_dist = .0083f;
    private float base_crouch_factor;
    public float crouch_factor = 0f;

    private float bounce_impulse;
    private float bounce_factor;

    public float head_rest_y;
    private bool will_jump;

    public int walking = 0;

    public float jump_factor = 700.0f;
    public float spring_body_conversion = 100f;

    public float spring_min_liftoff_factor = 8.5f;
    public float spring_max_liftoff_factor = 9f;

	float max_energy = 5f;
    public float energy = 5f;
	float energy_charge = .030f;

	float shield_charge = .030f;
	float max_shield = 3f;
    public float shield = 3f;

	float max_plasma_power = .8f;
	public float min_plasma_power = .25f;
	float plasma_charge = .035f;

    public float plasma1 = .8f;
    public float plasma2 = .8f;

	int max_grenades = 6;
	public int grenades = 6;

	int max_missiles = 4;
	public int missiles = 4;

	int max_boosters = 3;
	int boosters = 3;
	bool boosting = false;
	float boost_timer = 0;
	float boost_time = 0;

    public Vector3 move;
    
    private Material my_material;
    private MaterialPropertyBlock my_property_block;

    public bool can_fire_plasma() {
        return (plasma1 > min_plasma_power || plasma2 > min_plasma_power);
    }

    public bool can_fire_grenade() {
        return grenades > 0;
    }

    public bool can_fire_missile() {
        return missiles > 0;
    }

    private float plasma_update(float dt, float plasma) {
		float guncharge = ((energy + energy_charge) * plasma_charge) / max_energy;
		float new_energy = plasma;
		if (plasma < max_plasma_power) {
			energy -= guncharge * dt;
			if (plasma > min_plasma_power) {
				new_energy = plasma + (guncharge * .850f * dt);
			} else {
				new_energy = plasma + (guncharge * 1.050f * dt);
			}
			if (new_energy > max_plasma_power)
				new_energy = max_plasma_power;
		}
		return new_energy;
	}

	public void energy_update(float dt) {
		plasma1 = plasma_update(dt, plasma1);
		plasma2 = plasma_update(dt, plasma2);

		if (shield < max_shield) {
			float regen = (shield_charge * energy) / max_energy;

			if (boosting)
				shield += shield_charge * dt;

			shield += (regen / 8f) * dt;

			shield = Mathf.Min(shield, max_shield);
			energy -= regen * dt;
		}

		energy += energy_charge * dt;
		if (boosting)
			energy += energy_charge * 4 * dt;

		energy = Mathf.Min(max_energy, energy);
		energy = Mathf.Max(energy, 0f);

        this.BaseCharacter.CurrentHealth = shield;
	}
	
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

        state = new WalkerPhysics(155f, walker.transform, Vector3.zero, Vector3.zero, transform.localEulerAngles.y);
        crouch_spring = new DampenedSpring(crouch_factor);
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
        this.shield -= power;
        var glow = power / max_power;
        Debug.Log(glow + " " + power + " " + max_power);
        StartCoroutine(this.do_glow(glow));
        GameClient.PlayClipAt(damage_sound, transform.position);
    }

    private IEnumerator do_glow(float intensity) {
        for (float f = 1f; f >= 0; f -= .1f) {
            this.glow_walker(Color.Lerp(Color.black, Color.white * intensity, f));
            yield return new WaitForSeconds(.001f); ;
        }
    }

    private void glow_walker(Color c) {
        my_material.SetColor(Shader.PropertyToID("_EmissionColor"), c);
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
        
        LegUpdate(forward);

        float bob_factor = 0f;
        if (walking != 0) {
            bob_factor = Mathf.Abs(left_leg.walk_seq_step) / 300f;
        }

        var resting = crouch_spring.stable_pos == crouch_spring.pos;
        crouch_spring.stable_pos = 0f + base_crouch_factor + (bob_factor * bob_amount);
        if (resting) crouch_spring.pos = crouch_spring.stable_pos;
        crouch_spring.calculate(duration);
        crouch_factor = Mathf.Round(Mathf.Clamp(crouch_spring.pos, -1.2f, 1.2f) * 100) / 100f;
        //crouch_factor = crouch_spring.pos;

        var temp = head.transform.localPosition;
        temp.z = head_rest_y - crouch_factor * crouch_dist;
        head.transform.localPosition = temp;
        
        var previous_pos = state.pos;

        JumpUpdate(jump, duration);
        
        state.on_ground = Controller.isGrounded;

        InputTuple i = new InputTuple(forward, turn * 4f);
        state.integrate(Time.fixedTime, duration, i);
        
        transform.localEulerAngles = new Vector3(0, state.angle, 0);

        var tp = head.transform.position;
        tp.y -= .5f;
        Debug.DrawLine(tp, tp + (state.velocity * 100), Color.red);
        Debug.DrawLine(tp, tp + (state.accel * 10), Color.cyan);
        Debug.DrawLine(tp, tp + (state.momentum * .1f), Color.black);
        move = (previous_pos - state.pos);
        if (move.magnitude > .001) 
            Controller.Move(move * -1f);
        //Controller.SimpleMove(state.velocity);
	}

    public void LegUpdate(float vert) {
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

        left_leg.crouch_factor = crouch_factor;
        right_leg.crouch_factor = crouch_factor;

        var xz_vel = state.velocity;
        xz_vel.y = 0;
        left_leg.speed = xz_vel.magnitude;
        right_leg.speed = xz_vel.magnitude;
    }

    private void JumpUpdate(bool jump, float duration) {
        // jump key is being pressed
        if (jump) {
            //crouch_factor = Mathf.Min(1.0f - bob_amount, crouch_factor + crouch_dt);
            crouch_spring.vel += 400f * duration;
            will_jump = true;
        }
        else {
            if (state.on_ground && state.accel.y < .3) {
                if (crouch_spring.vel < -spring_min_liftoff_factor) {
                    Debug.Log(crouch_spring.vel);
					state.accel.y = crouch_spring.vel * -80f;
                    state.momentum.y = 0f;
                }
            }
            will_jump = false;
        }

        if (!state.on_ground && Controller.isGrounded) {
            // Just landed
            //Debug.Log(this.name + " Landed");
            crouch_spring.vel = -state.velocity.y * spring_body_conversion;
            state.velocity.y = 0;
            // state.momentum.y = -0.1f * state.momentum.y * Time.deltaTime;
            state.accel.y = 0;
        }
    }

    public Vector2 clampInDegrees = new Vector2(240, 60);

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

        // TODO: Rigging
        var xRotation = Quaternion.AngleAxis(_headRot.x, targetOrientation * Vector3.right);
        var yRotation = Quaternion.AngleAxis(_headRot.y, targetOrientation * Vector3.up);
        
        head.transform.localRotation = xRotation;
        head.transform.localRotation *= yRotation;
        head.transform.localRotation *= targetOrientation;

        base_crouch_factor = _headRot.y / clampInDegrees.y * .8f;
    }

	public override float GetCurrentSpeed ()
	{
        return state.velocity.magnitude;
		//if (Controller != null) {
		//	return Controller.velocity.magnitude;
		//} else {
		//	return 0;
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
            crouch_factor,
            walking);
    }
}
