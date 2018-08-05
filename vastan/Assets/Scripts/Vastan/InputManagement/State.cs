using UnityEngine;

namespace Vastan.InputManagement
{
	[System.Serializable]
    public class InputFrame 
    {
        public Keys keysPressed;
        public float lookX;
        public float lookY;

        private void _AddKey(Keys theKey) 
        {
            keysPressed = keysPressed | theKey;
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
                _AddKey(Keys.Forward);
            }
            if (ForwardBack < 0) 
            {
                _AddKey(Keys.Back);
            }

            if (LeftRight > 0) 
            {
                _AddKey(Keys.Right);
            }

            if (LeftRight < 0) 
            {
                _AddKey(Keys.Left);
            }

            if (Input.GetButton("Jump"))
            {
                _AddKey(Keys.Jump);
            }

            if (Input.GetButton("Fire1")) 
            {
                _AddKey(Keys.Fire);
            }

            if (Input.GetButton("LoadGrenade")) 
            {
                _AddKey(Keys.Grenade);
            }

            if (Input.GetButton("LoadMissile")) 
            {
                _AddKey(Keys.Missile);
            }

            if (Input.GetButton("FireGrenade"))
            {
                if (!HasKey(Keys.Fire)) 
                {
                    _AddKey(Keys.Fire);
                }
                if (!HasKey(Keys.Grenade)) 
                {
                    _AddKey(Keys.Grenade);
                }
            }

            if (Input.GetButton("FireMissile"))
            {
                if (!HasKey(Keys.Fire)) 
                {
                    _AddKey(Keys.Fire);
                }
                if (!HasKey(Keys.Missile)) 
                {
                    _AddKey(Keys.Grenade);
                }
            }

            if (Input.GetButton("Scout")) 
            {
                _AddKey(Keys.Scout);
            }

            if (Input.GetButton("ScoutControl")) 
            {
                _AddKey(Keys.ScoutControl);
            }
        }
    }

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