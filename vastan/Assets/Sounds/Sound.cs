using System;
using UnityEngine;

[Serializable]
public abstract class Sound
{
	public enum SoundId : int
	{
		
		None = 0,
		
		GrenadeExplode = 1,

        WallHit = 2
	}
}

