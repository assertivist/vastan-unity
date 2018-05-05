using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// This is gets sent from the client to the server to represent the player's move/look controls.  It gets 
/// applied to the character on the sever using the same method that it gets applied on the client.
/// </summary>
[Serializable]
public class ControlCommand
{
    public int CharacterId { get; set; }
    
    public int Id { get; set; }
    
    public float Duration { get; set; }
    
    public float Forward { get; set; }
    
    public float Turn { get; set; }
    
    public bool Jump { get; set; }
    
    public float LookHorz { get; set; }
    
    public float LookVert { get; set; }
    
    
    public ControlCommand (int newCharacterId, int newId)
    {
        CharacterId = newCharacterId;
        Id = newId;
    }
    
    
    public ControlCommand (int newCharacterId, int newId, float newDuration, float newForward, float newTurn, float newLookHz, float newLookVt, bool jump)
    {    
        CharacterId = newCharacterId;
        Id = newId;
        Duration = newDuration;
        Forward = newForward;
        Turn = newTurn;
        LookHorz = newLookHz;
        LookVert = newLookVt;
        Jump = jump;
    }

    public override string ToString ()
    {
        return "F: " + Forward + ", Turn: " + Turn;
    }
    
}
