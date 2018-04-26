using UnityEngine;
using System.Collections;
using System;

namespace ServerSideCalculations.Networking
{
    [Serializable]
    public class ObjectState {

        public int NetworkId { get; set; }

        public Vector3 Position { get; set; }

        public float Angle { get; set; }

        public Quaternion HeadRot { get; set; }

        public Vector3 Velocity { get; set; }
        
        public float Crouch { get; set; }

        public float Stance { get; set; }
        
        public float Walking { get; set; }
		
		
		public ObjectState ()
		{
		}
		
		public ObjectState (
            int networkId, 
            Vector3 position, 
            float angle, 
            Quaternion headRot,
            Vector3 velocity,
            float crouch,
            float stance,
            int walking)
		{
			NetworkId = networkId;
			Position = position;
            Angle = angle;
            HeadRot = headRot;
            Velocity = velocity;
            Crouch = crouch;
            Stance = stance;
            Walking = walking;
		}
	}
}