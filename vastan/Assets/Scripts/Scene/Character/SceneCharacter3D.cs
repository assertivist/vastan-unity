using UnityEngine;
using System.Collections;
using ServerSideCalculations.Characters;

public class SceneCharacter3D : SceneCharacter
{
	public CharacterController Controller { get; set; }
    public GameObject head;

    public GameObject walker;
    private SceneCharacter3D walker_char;

    public Leg left_leg;
    public Leg right_leg;

    public Vector2 targetDirection;
	public float PitchAngle { get; set; }
    private Vector2 _headRot;

    private const float bob_amount = .08f;
    private const float crouch_dist = 1.0f;
    public float crouch_factor = 0f;

    private float bounce_impulse;
    private float bounce_factor;

    private float head_rest;

    int walking = 0;


    // Use this for initialization
    public void Start ()
	{
		Controller = GetComponent<CharacterController> ();
		
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody> ()) {
			GetComponent<Rigidbody> ().freezeRotation = true;
		}

        targetDirection = head.transform.TransformDirection(head.transform.localRotation.eulerAngles);
        targetDirection.y += 90;

        var legs = walker.GetComponents<Leg>();
        left_leg = legs[0];
        right_leg = legs[1];
        head_rest = head.transform.localPosition.z;
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

        left_leg.direction = vert;
        right_leg.direction = vert;

        left_leg.crouch_factor = crouch_factor;
        right_leg.crouch_factor = crouch_factor;

        var temp = head.transform.localPosition;
        temp.z = head_rest - crouch_factor * crouch_dist;
        head.transform.localPosition = temp;

        //Debug.Log ("Moving character " + name + " " + forward + " x " + duration + (!Controller.isGrounded ? " not" : "") + " grounded");

        // Only allow movement control when touching the ground
        if (Controller.isGrounded) {
            // Rotate self based on turn input.
            this.transform.localEulerAngles = new Vector3(0, this.transform.localEulerAngles.y + turn, 0);
            // Feed moveDirection with input.
            MoveDirection = new Vector3 (0, 0, forward);
			MoveDirection = this.transform.TransformDirection (MoveDirection);
			//Debug.Log("   Grounded - Direction: " + MoveDirection + " Move Speed: " + ((Character)this).MoveSpeed);
			
			// Multiply it by speed.
			MoveDirection *= ((Character)this).MoveSpeed;
			
			// Sometimes the character isn't exactly touching the ground when jump is pressed, so give it a little time if it needs
			if (TimeToJump ()) {
				//Debug.Log("Attempting to jump!");
				MoveDirection = new Vector3 (MoveDirection.x, ((Character)this).JumpSpeed, MoveDirection.z);
			}
		} else {	
			//Debug.Log( "Not grounded - making the character fall");
			//Applying gravity to the controller
			MoveDirection = new Vector3 (MoveDirection.x, MoveDirection.y - (Game.GRAVITY * duration), MoveDirection.z);
			
			// Make sure nobody falls through the ground
			if (transform.position.y < -100) {
				Debug.Log ("Falling through the ground!");
				transform.position = new Vector3 (
					transform.position.x,
					10f,
					transform.position.z
				);
				
				MoveDirection = new Vector3 (0, 0, 0);
			}
		}
		
		//Making the character move
		Controller.Move (MoveDirection * duration);
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
