using UnityEngine;
using System.Collections;
using ServerSideCalculations.Networking;
using ServerSideCalculations.Characters;
using ServerSideCalculations;
using System.Collections.Generic;

public class AI2D : SceneCharacter2D, IArtificialIntelligence
{
	public SceneCharacter Target { get; set; }

	public GameServer Server { get; set; }

	public SceneCharacter GetSceneChar ()
	{
		return this; 
	}


	// Update is called once per frame
	void Update ()
	{
		this.UpdateAI ();
	}

	#region Movement

	public override bool MissingController ()
	{
		return false;
	}


	public override float GetCurrentSpeed ()
	{
		return 0f;
	}

	public void RunAtTarget ()
	{	
		if (!((Character)Target).IsAlive) {
			return;
		}
		
		// Move toward the Target
		if (Vector3.Distance (Target.transform.position, transform.position) >= ((Character)this).ArmLength) {
			
			var right = this.transform.position.x - Target.transform.position.x < 0 ? 1 : -1;
			////Debug.Log ("Move " + right);
			this.ExecuteControlCommand (new ControlCommand (0, 0, Time.deltaTime, 0, right, 0, 0, false));
		} else {
			Debug.Log (Vector3.Distance (Target.transform.position, transform.position) + " > " + ((Character)this).ArmLength);
		}
	}

	#endregion Movement
	
	#region Combat

	public float AttackDelay;

	public float TimeLastAttacked;


	public void AttackTarget ()
	{

	}

	#endregion Combat
	
}
