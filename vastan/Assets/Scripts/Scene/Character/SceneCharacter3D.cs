using UnityEngine;
using System.Collections;
using ServerSideCalculations.Characters;

public class SceneCharacter3D : SceneCharacter
{
	public CharacterController Controller { get; set; }
    public GameObject head;
    public Vector2 targetDirection;
	public float PitchAngle { get; set; }
    private Vector2 _headRot;


	// Use this for initialization
	public void Start ()
	{
		Controller = GetComponent<CharacterController> ();
		
		// Make the rigid body not change rotation
		if (GetComponent<Rigidbody> ()) {
			GetComponent<Rigidbody> ().freezeRotation = true;
		}

        targetDirection = head.transform.localRotation.eulerAngles;
        targetDirection.y += 90;
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
			((Character)this).AttemptJump ();
		}
		
		if (Controller == null) {
			return;
		}
		
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

    public Vector2 clampInDegrees = new Vector2(195, 85);

    /**
		* Turn/tilt the player's head as needed
		*/
    public void Look (float yawAmount, float pitchAmount)
	{
        var _smoothMouse = new Vector2(yawAmount, pitchAmount);
        // Allow the script to clamp based on a desired target value.
        var targetOrientation = Quaternion.Euler(targetDirection);

        // Find the absolute mouse movement value from point zero.
        _headRot += _smoothMouse;

        // Clamp and apply the local x value first, so as not to be affected by world transforms.
        if (clampInDegrees.x < 360)
            HeadRot.x = Mathf.Clamp(HeadRot.x, -clampInDegrees.x * 0.5f, clampInDegrees.x * 0.5f);

        // Then clamp and apply the global y value.
        if (clampInDegrees.y < 360)
            HeadRot.y = Mathf.Clamp(HeadRot.y, -clampInDegrees.y * 0.5f, clampInDegrees.y * 0.5f);

        var xRotation = Quaternion.AngleAxis(HeadRot.x, targetOrientation * Vector3.right);
        var yRotation = Quaternion.AngleAxis(HeadRot.y, targetOrientation * Vector3.up);

        head.transform.localRotation = xRotation;
        head.transform.localRotation *= yRotation;

        head.transform.localRotation *= targetOrientation;
        HeadRot = head.transform.localRotation;
        _headRot = new Vector2(HeadRot.x, HeadRot.y);
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
