using System;
using UnityEngine;

[Serializable]
public abstract class Sound
{
	public enum SoundId : int
	{
		
		None = 0,
		
		#region Weapon Prep
		Grunt1 = 1,
		#endregion

		#region Ability Execution
		Woosh1 = 1000,
		#endregion
		
		#region Block
		Clink1 = 2000,
		#endregion
		
		#region Hit
		MetalHitFlesh1 = 3000,
		#endregion
		
		#region Pain
		Hurt1 = 4000,
		#endregion
		
		#region Death
		Distortion = 5000
		#endregion
	}
}

