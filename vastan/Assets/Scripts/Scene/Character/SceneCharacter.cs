using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ServerSideCalculations.Networking;
using ServerSideCalculations.Characters;
using ServerSideCalculations;

/// <summary>
/// Unity character.  This class contains the non-serializable Unity objects
/// </summary
public abstract class SceneCharacter : MonoBehaviour
{
	#region Inheritance
	
	public Character BaseCharacter { get; set; }

	/// <summary>
	/// To get around not having multiple inheritance or the ability to set a parent object
	/// </summary
	public static implicit operator Character (SceneCharacter sc)
	{
		if (sc == null) {
			return null;
		}
		return sc.BaseCharacter;
	}
	
	public GameClient Client { get; set; }
	
	#endregion
	
	public abstract bool MissingController ();
	
	#region Constructors

	/// <summary>
	/// Creates a default SceneCharacter
	/// </summary>
	public SceneCharacter ()
	{
		BaseCharacter = new Character ();
		//LoadSounds ();
		//Debug.Log( "Creating Scene character with new Character with MoveSpeed " + ((Character)this).MoveSpeed );
	}

	/// <summary>
	/// Creates a SceneCharacter with existing Character data
	/// </summary>
	/// <param name="character">Character.</param>
	public SceneCharacter (Character character) : this()
	{
		//Debug.Log( "Creating UnityCharacter from existing Character with MoveSpeed " + character.MoveSpeed );
		BaseCharacter = character;
	}


    #endregion Constructors


    #region Movement

    public Vector3 MoveDirection;

    //public Quaternion HeadRot;

    public string PlayerName;

    public abstract void ExecuteControlCommand (ControlCommand control);

	public abstract float GetCurrentSpeed ();
	
	/// <summary>
	/// Check if it's time to jump.
	/// </summary>
	/// <returns>Whether or not it's time to jump.</returns>
	public bool TimeToJump ()
	{
		return	Time.time >= ((Character)this).TimeAttemptedJump && 
			Time.time <= ((Character)this).TimeAttemptedJump + Game.ATTEMPT_JUMP_DURATION;
	}

	
	
	
	#endregion Movement
	
	
	
	public void GoAway ()
	{
		Debug.Log ("Rosebud...");
		Destroy (gameObject);
	}
}