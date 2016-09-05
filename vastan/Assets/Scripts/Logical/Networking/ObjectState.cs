using UnityEngine;
using System.Collections;
using System;

namespace ServerSideCalculations.Networking
{
	[Serializable]
	public class ObjectState
	{
		
		public int NetworkId { get; set; }
		
		public Vector3 Position { get; set; }
		
		public Vector3 Forward { get; set; }

        public Vector2 HeadRot { get; set; }
		
		
		public ObjectState ()
		{
		}
		
		public ObjectState (int networkId, Vector3 position, Vector3 forward, Vector2 headRot)
		{
			NetworkId = networkId;
			Position = position;
			Forward = forward;
            HeadRot = headRot;
		}
	}
}