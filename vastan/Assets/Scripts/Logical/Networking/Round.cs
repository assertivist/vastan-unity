using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ServerSideCalculations.Networking
{
	/**
	 * This class is used to rewind the game state back to when needed to compensate for lag
	 * It is also sent to players who have just joined or who had a bad lag spike
	 */
	[Serializable]
	public class Round
	{
		public int RoundNumber {get;set;}	
		
		public float TimeRoundStarted {get;set;}	
		
		public Dictionary<int, ObjectState> CurrentObjectStates {get;set;}	// All objects
		
		
		public Round()
		{
			CurrentObjectStates = new Dictionary<int, ObjectState>();
		}
		
		
		public Round( int newRoundNumber, float start ) : this()
		{
			RoundNumber = newRoundNumber;
			TimeRoundStarted = start;
		}
	}
}