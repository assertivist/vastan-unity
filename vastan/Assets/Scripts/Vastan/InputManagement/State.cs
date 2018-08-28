using UnityEngine;
using System;

namespace Vastan.InputManagement
{
	[System.Serializable]
    public class InputFrame 
    {
        public Keys keysPressed;
        public float lookX;
        public float lookY;

		private void AddKey(Keys theKey) 
        {
			Keys keys = keysPressed | theKey;
			keysPressed = keys;
        }

        public bool HasKey(Keys theKey) 
        {
            return (keysPressed & theKey) == theKey;
        }

        public InputFrame(InputFrame otherInputFrame) 
        {
            keysPressed = otherInputFrame.keysPressed;
            lookX = otherInputFrame.lookX;
            lookY = otherInputFrame.lookY;
        }

        public InputFrame() 
        {
            float ForwardBack = Input.GetAxis("Vertical");
            float LeftRight = Input.GetAxis("Horizontal");
            if (ForwardBack > 0) 
            {
                AddKey(Keys.Forward);
            }
            if (ForwardBack < 0) 
            {
                AddKey(Keys.Back);
            }

            if (LeftRight > 0) 
            {
                AddKey(Keys.Right);
            }

            if (LeftRight < 0) 
            {
                AddKey(Keys.Left);
            }

            if (Input.GetButton("Jump"))
            {
                AddKey(Keys.Jump);
            }

            if (Input.GetButton("Fire1")) 
            {
                AddKey(Keys.Fire);
            }

            if (Input.GetButton("LoadGrenade")) 
            {
                AddKey(Keys.Grenade);
            }

            if (Input.GetButton("LoadMissile")) 
            {
                AddKey(Keys.Missile);
            }

            if (Input.GetButton("FireGrenade"))
            {
                if (!HasKey(Keys.Fire)) 
                {
                    AddKey(Keys.Fire);
                }
                if (!HasKey(Keys.Grenade)) 
                {
                    AddKey(Keys.Grenade);
                }
            }

            if (Input.GetButton("FireMissile"))
            {
                if (!HasKey(Keys.Fire)) 
                {
                    AddKey(Keys.Fire);
                }
                if (!HasKey(Keys.Missile)) 
                {
                    AddKey(Keys.Grenade);
                }
            }

            if (Input.GetButton("Scout")) 
            {
                AddKey(Keys.Scout);
            }

            if (Input.GetButton("ScoutControl")) 
            {
                AddKey(Keys.ScoutControl);
            }
        }
    }
    
    [Flags]
    public enum Keys : long 
    {
        None = 0,
        Forward = 1,
        Back = 2,
        Left = 4,
        Right = 8,
        Jump = 16,
        Fire = 32,
        Grenade = 64,
        Missile = 128,
        VHeight = 256,
        Scout = 512,
        ScoutControl = 1024
    }
}