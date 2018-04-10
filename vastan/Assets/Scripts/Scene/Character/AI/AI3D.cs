using UnityEngine;
using System.Collections;
using ServerSideCalculations.Networking;
using ServerSideCalculations.Characters;
using ServerSideCalculations;
using System.Collections.Generic;

public class AI3D : SceneCharacter3D, IArtificialIntelligence
{
    
	public SceneCharacter Target { get; set; }

	public GameServer Server { get; set; }

	public SceneCharacter3D GetSceneChar ()
	{
		return this; 
	}
    new void Start () {
        
        //fwd = Vector3.zero;
        base.Start();
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

	float MaxTurnSpeed = 10;
	
	public void RunAtTarget ()
	{

		if (!((Character)Target).IsAlive) {
			return;
		}

        //Debug.Log ("Turning toward " + Target.transform.position + " at " + MaxTurnSpeed + "*" + Time.deltaTime);

        // Turn toward the Target
        //this.transform.rotation = Quaternion.Slerp (this.transform.rotation, Quaternion.LookRotation (Target.transform.position - this.transform.position), MaxTurnSpeed * Time.deltaTime);
        
        var pos = Target.transform.position;
        pos.y += 1.2f;
        head.transform.LookAt(pos, Vector3.up);
        var a = head.transform.localEulerAngles;
        a.z += 90;
        head.transform.localEulerAngles = a;


        var look_quat = Quaternion.LookRotation(Target.transform.position - this.transform.position, Vector3.up);
        var angles = look_quat.eulerAngles;
        var dist = Mathf.DeltaAngle(this.state.angle, angles.y);
        float turn = 0;

        if (dist < -5) {
            turn = -1f;
        }
        else if (dist > 5) {
            turn = 1;
        }
        

        // Move toward the Target
        if (Vector3.Distance (Target.transform.position, transform.position) >= ((Character)this).ArmLength) {
            
		}

        base.Move(0f, turn, Time.fixedDeltaTime, false);
    }
    
    #endregion Movement
    
    #region Combat

	public float AttackDelay;

	public float TimeLastAttacked;


	public void AttackTarget ()
	{
		var timeSinceLastAttack = Time.time - TimeLastAttacked;
		if (timeSinceLastAttack < AttackDelay) {
			return;
		}			
		
		TimeLastAttacked = Time.time;
		
		foreach (var potentialTarget in Server.SceneCharacters.Values) {
			// Check friendly fire
			if (!Server.FriendlyFire && ((Character)potentialTarget).Team == BaseCharacter.Team) {
				continue;
			}
			
			if (potentialTarget.transform.IsWithinArc (this.transform, BaseCharacter.ArmLength + 2f, 120f)) {
				Server.CharacterHits (this, (Character)potentialTarget); // 16D-E
			}
		}
	}
    
    #endregion Combat
    
}
