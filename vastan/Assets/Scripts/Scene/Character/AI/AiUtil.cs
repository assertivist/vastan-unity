using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ServerSideCalculations.Characters;

public static class AiUtil
{
	public static void UpdateAI (this IArtificialIntelligence ai)
	{
		if (ai.Server == null || !((Character)ai.GetSceneChar ()).IsAlive) {
			return;
		}
		
		if (ai.Target == null) {
			ai.Target = ai.GetSceneChar ().FindNearestEnemy (ai.Server.SceneCharacters.Values);
		}
		
		//Debug.Log( "AI Target is " + Target + " at " + Target.transform.position );
		
		// If there was no nearest enemy found, then don't do anything
		if (ai.Target == null) {
			return;
		}
		
		if (((Character)ai.Target).IsAlive) {
			ai.RunAtTarget ();
			ai.AttackTarget ();
		}
	}
		
		
	public static SceneCharacter FindNearestEnemy (this SceneCharacter aiChar, IEnumerable<SceneCharacter> potentialEnemies)
	{
		//Debug.Log( "Character " + aiChar + " finding nearest enemy in " + characters );
		
		SceneCharacter nearestEnemy = null;
		float nearestEnemyDistance = float.MaxValue;
		float currentEnemyDistance = 0f;
		
		foreach (SceneCharacter potentialEnemy in potentialEnemies) {
			//Debug.Log( "Checking if " + character + " is the nearest enemy" );
			
			// No friendly fire
			if (((Character)potentialEnemy).Team == ((Character)aiChar).Team) {
				//Debug.Log( "Both characters are on team " + Team );
				continue;
			}
			
			currentEnemyDistance = Vector3.Distance (potentialEnemy.transform.position, aiChar.transform.position);
			if (currentEnemyDistance < nearestEnemyDistance) {
				//Debug.Log( "Enemy " + character + " is the new nearest enemy" );
				nearestEnemy = potentialEnemy;
				nearestEnemyDistance = currentEnemyDistance;
			} else {
				//Debug.Log( "Enemy " + character + " at " + currentEnemyDistance + "is not closer than " + nearestEnemyDistance );
			}
		}
		
		//Debug.Log( "Nearest enemy is " + nearestEnemy );
		return nearestEnemy;
	}
}
