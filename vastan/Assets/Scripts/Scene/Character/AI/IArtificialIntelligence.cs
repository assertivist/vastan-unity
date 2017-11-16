using UnityEngine;
using System.Collections;
using ServerSideCalculations.Characters;

public interface IArtificialIntelligence
{
	GameServer Server { get; set; }
		
	SceneCharacter Target { get; set; }
	
	Character BaseCharacter { get; set; }
    
	SceneCharacter3D GetSceneChar ();
		
	void RunAtTarget ();
		
	void AttackTarget ();
}
