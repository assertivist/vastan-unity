using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace ServerSideCalculations
{
	public static class Util
	{
		
		#region Serialization
		
		public static byte[] Serialize (this System.Object obj)
		{
			if (obj == null)
				return null;
			BinaryFormatter bf = new BinaryFormatter ();
			MemoryStream ms = new MemoryStream ();
			bf.Serialize (ms, obj);
			return ms.ToArray ();
		}
		
		public static System.Object Deserialize (this byte[] arrBytes)
		{
			MemoryStream memStream = new MemoryStream ();
			BinaryFormatter binForm = new BinaryFormatter ();
			memStream.Write (arrBytes, 0, arrBytes.Length);
			memStream.Seek (0, SeekOrigin.Begin);
			System.Object obj = (System.Object)binForm.Deserialize (memStream);
			return obj;
		}
		
		#endregion Serialization
		
		
		#region Cloning
		
		public static T Clone<T> (this T source)
		{ 
			Debug.Log ("Trying to serialize class " + typeof(T).ToString ());
			
			/*if (!typeof(T).IsSerializable) 
			{ 
				throw new ArgumentException("The type must be serializable.", "source");
			} */
	 
			// Don't serialize a null object, simply return the default for that object 
			if (System.Object.ReferenceEquals (source, null)) { 
				return default(T);
			} 
	 
			IFormatter formatter = new BinaryFormatter ();
			Stream stream = new MemoryStream ();
			using (stream) { 
				formatter.Serialize (stream, source);
				stream.Seek (0, SeekOrigin.Begin);
				return (T)formatter.Deserialize (stream);
			} 
		} 
		
		#endregion
		
		
		#region Transform
		
		/**
		 * Checks if the character is facing within X degrees of the target, and the target is within X distance
		 */
		public static bool IsWithinArc (this Transform me, Transform origin, float arcDistance, float arcAngle)
		{	
			float distanceToTarget = Vector3.Distance (origin.position, me.position);
			////Debug.Log ("Distance to target: " + distanceToTarget + "/" + arcDistance);
			float angleToTarget = Game.DEGREES_PER_CALC_ANGLE * Mathf.Acos (Vector3.Dot ((me.position - origin.position).normalized, origin.forward));
			////Debug.Log ("Angle to target: " + angleToTarget + "/" + arcAngle);
			return distanceToTarget <= arcDistance && angleToTarget <= arcAngle;
		}
		
		
		public static float CoordinateToAngle (float xDiff, float yDiff)
		{
			if (xDiff == 0) { // Avoid divide by 0 error
				return 0;
			}
			
			try {
				float angle = -Mathf.Atan (yDiff / xDiff) * Mathf.Rad2Deg;
				if (xDiff < 0) {
					angle += 180;
				}
				return angle;
			} catch (Exception e) {
				// Meh
				Debug.Log (e.Message);
				return 0f;
			}
		}


		public static string ToCoordinates (this Transform trans)
		{
			return trans.position.x + ", " + trans.position.y + ", " + trans.position.z;
		}


		public static void ZeroZ (this Transform trans)
		{
			trans.position = new Vector3 (trans.position.x, trans.position.y, 0);
		}
		
		public static void Move (this Transform trans, float right, float up)
		{
			trans.position = new Vector3 (
							trans.position.x + right,
							trans.position.y + up,
							trans.position.z
			);
		}
		
		public static void MoveUp (this Transform trans, float up)
		{
			trans.position = new Vector3 (
						trans.position.x,
						trans.position.y + up,
						trans.position.z
			);
		}

		public static Vector2 SetX (this Vector2 v, float x)
		{
			return new Vector2 (x, v.y);
		}
		
		public static Vector2 SetY (this Vector2 v, float y)
		{
			return new Vector2 (v.x, y);
		}

		#endregion
		

		#region Game values
		
		public static Sound.SoundId ToSoundId (this int soundIdInt)
		{
			foreach (Sound.SoundId id in Sound.SoundId.GetValues(typeof(Sound.SoundId))) {
				if ((int)id == soundIdInt) {
					return id;
				}
			}
			
			return Sound.SoundId.None;
		}
		
		#endregion
	}
}
		
