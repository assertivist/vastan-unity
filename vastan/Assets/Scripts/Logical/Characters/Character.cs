using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace ServerSideCalculations.Characters
{	
	/// <summary>
	/// This is the serializable version of the character, which can be sent across 
	/// the network for saving/loading from a database, or sending the character's 
	/// data to a new player
	/// </summary>
	[Serializable]
	public class Character
	{
		#region Constants
		
		public const float ARM_LENGTH_RATIO = .45f;
		
		#endregion
		
		
		#region Attributes

		public int Id { get; set; }
		public string CharName { get; set; }
		
		public float Height { get; set; }

		public string Team { get; set; }
		
		public float ArmLength{ get { return Height * ARM_LENGTH_RATIO; } }
		
		public bool IsAlive { get; set; }
		public float MaxHealth { get; set; }
		public float CurrentHealth { get; set; }
		
		#endregion
		
		
		#region Constructors
		
		public Character () : this("NoName")
		{
		}


		public Character (string name)
		{
			CharName = name;
			FillInDefaults ();
		}
		

		public Character (Character baseCharacter) : this()
		{
			CopyFrom (baseCharacter);
		}
		
		
		private void FillInDefaults ()
		{
			//Debug.Log( "Filling in defaults for " + CharName );
			
			Team = CharName;
			Height = 2f;
			
			IsAlive = true;
			MaxHealth = 100;
			CurrentHealth = MaxHealth;

			MoveSpeed = 7f;
			JumpSpeed = 4f;
		}
		
		
		public void CopyFrom (Character baseCharacter)
		{
			CharName = baseCharacter.CharName;
			//Debug.Log( "Creating Character " + CharName + " from another character");
			
			Team = CharName;
			
			FillInDefaults ();
		}
		
		#endregion
		
		
		#region Movement
		
		public float MoveSpeed;// {get;set;}
		public float JumpSpeed { get; set; }
		
		public float TimeAttemptedJump { get; set; } // This is used to avoid missed jumps caused by minor dips in the ground
		
		
		public void AttemptJump ()
		{	
			// You can add other rules here
			TimeAttemptedJump = Time.time;
		}
		
		#endregion
				


		
	}
}