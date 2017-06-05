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

    private float head_rest_y;
    private bool will_jump;

    int walking = 0;

    public float jump_factor = 700.0f;
    public float spring_body_conversion = 100f;

    public float spring_min_liftoff_factor = 8.5f;
    public float spring_max_liftoff_factor = 9f;


    // Use this for initialization
    public void Start() {
        Controller = GetComponent<CharacterController>();

        // Make the rigid body not change rotation
        if (GetComponent<Rigidbody>()) {
            GetComponent<Rigidbody>().freezeRotation = true;
        }
        var ea = head.transform.localRotation.eulerAngles;
        targetDirection = new Vector2(ea.x, ea.y);
        targetDirection.y += 90;
        head_rest_y = head.transform.localPosition.z;

        state = new WalkerPhysics(155f, walker.transform, Vector3.zero, Vector3.zero, transform.localEulerAngles.y);
        crouch_spring = new DampenedSpring(crouch_factor);
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

        var temp = head.transform.localPosition;
        temp.z = head_rest_y - crouch_factor * crouch_dist;
        head.transform.localPosition = temp;
        
        var previous_pos = state.pos;

        JumpUpdate(jump, duration);

        crouch_spring.calculate(duration);
        state.on_ground = Controller.isGrounded;

        InputTuple i = new InputTuple(forward, turn * 4f);
        state.integrate(Time.fixedTime, duration, i);
        
        transform.localEulerAngles = new Vector3(0, state.angle, 0);

        var tp = head.transform.position;
        tp.y -= .5f;
        Debug.DrawLine(tp, tp + (state.velocity * 100), Color.red);
        Debug.DrawLine(tp, tp + (state.accel * 10), Color.cyan);
        Debug.DrawLine(tp, tp + (state.momentum * .1f), Color.black);
        var move = (previous_pos - state.pos);
        if (move.magnitude > .001) 
            Controller.Move(move * -1f);
	}

    private void LegUpdate(float vert) {
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

        float bob_factor = 0f;
        if (walking != 0) {
            bob_factor = Mathf.Abs(left_leg.walk_seq_step) / 300f;
        }
        crouch_spring.stable_pos = 0f + base_crouch_factor + (bob_factor * bob_amount);
        crouch_factor = crouch_spring.pos;

        crouch_factor = Mathf.Clamp(crouch_spring.pos, -1.0f, 1.2f);

        left_leg.direction = vert;
        right_leg.direction = vert;

        left_leg.crouch_factor = crouch_factor;
        right_leg.crouch_factor = crouch_factor;

        var xz_vel = state.velocity * .8f;
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
                    state.accel.y = jump_factor;
                    state.momentum.y = 1f;
                }
            }
            will_jump = false;
        }

        if (!state.on_ground && Controller.isGrounded) {
            // Just landed
            Debug.Log(this.name + " Landed");
            crouch_spring.vel = -state.velocity.y * spring_body_conversion;
            state.velocity.y = 0;
            // state.momentum.y = -0.1f * state.momentum.y * Time.deltaTime;
            state.accel.y = 0;
        }
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
    }

	public override float GetCurrentSpeed ()
	{
        return state.velocity.magnitude;
		if (Controller != null) {
			return Controller.velocity.magnitude;
		} else {
			return 0;
		}
	}
}
