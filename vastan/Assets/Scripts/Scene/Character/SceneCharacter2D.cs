using UnityEngine;
using System.Collections;
using ServerSideCalculations.Characters;
using ServerSideCalculations;

public class SceneCharacter2D : SceneCharacter
{
	public ThrowawayCharacterController2D controller;

	public void Start ()
	{
		controller.JumpSpeed = ((Character)this).JumpSpeed;
		controller.MoveSpeed = ((Character)this).MoveSpeed;
	}

	public override bool MissingController ()
	{
		return false;
	}


	public override void ExecuteControlCommand (ControlCommand control)
	{	
		////Debug.Log ("Executing control command " + control.ToString ());
		if (control.Jump) {
			((Character)this).AttemptJump ();
		}

		controller.Move (control.Strafe, control.Duration, TimeToJump ());
	}


	public override float GetCurrentSpeed ()
	{
		////Debug.Log ("Magnitude = " + (MyPlayer.Controller2d.velocity.magnitude * 10000));
		return Mathf.Abs (controller.MoveDirection.x);
	}	
}
