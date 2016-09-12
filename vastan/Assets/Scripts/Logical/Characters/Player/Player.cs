using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using ServerSideCalculations.Characters;
using ServerSideCalculations.Networking;

[Serializable]
public class Player
{
	public string Id { get; set; } //  The client player's GUId
	public NetworkPlayer NetworkPlayerLocation { get; set; }
	
	public Character BaseCharacter { get; set; } // The logical character - name, stats, etc.
	public SceneCharacter InGameCharacter { get; set; } // The in-game model of the character
	public NetworkViewID CharViewId { get { return InGameCharacter.GetComponent<NetworkView>().viewID; } }

	public int RoundLastRespondedTo { get; set; } // To synchronize with the server
	public Dictionary<int, ObjectState> StatesAfterControls = new Dictionary<int, ObjectState> ();
	
	public int LastControlNumApplied { get; set; } // For position correction
	
	public int LastCorrectionSent { get; set; }
	
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Player"/> class.
	/// </summary>
	/// <param name='guid'>
	/// GUId.
	/// </param>
	public Player (NetworkPlayer networkPlayer)
	{
		NetworkPlayerLocation = networkPlayer;
		Id = networkPlayer.guid;
	}
	
	
	/// <summary>
	/// Initializes a new instance of the <see cref="Player"/> class.
	/// </summary>
	/// <param name='guid'>
	/// GUId.
	/// </param>
	/// <param name='parentCharacter'>
	/// Parent character.
	/// </param>
	public Player (NetworkPlayer networkPlayer, Character parentCharacter) : this( networkPlayer )
	{
		BaseCharacter = parentCharacter;
	}
}
