using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Vastan.Util;

namespace Vastan.Networking
{   
    public class SerializedMessage
    {
        public List<byte> theBytes = new List<byte>();

        public static void HostOrder(byte[] bytes) 
		{
			if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
		}

		public static void NetOrder(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
        }

        public void AddByte(byte theByte)
        {
            theBytes.Add(theByte);
        }

        public static byte GetByte(BinaryReader reader)
        {
            return reader.ReadByte();
        }

        public void AddMessageType(MessageType type)
        {
            AddByte((byte)type);
        }

        public static MessageType GetMessageType(BinaryReader reader)
        {
            byte theType = GetByte(reader);
            return (MessageType)theType;
        }

        public void AddFloat(float theFloat)
        {
            byte[] floatBytes = BitConverter.GetBytes(theFloat);
			NetOrder(floatBytes);
            theBytes.AddRange(floatBytes);
        }

        public static float GetFloat(BinaryReader reader)
        {
            byte[] floatBytes = reader.ReadBytes(4);
			HostOrder(floatBytes);
            return BitConverter.ToSingle(floatBytes, 0);
        }

        public void AddInt(int theInt)
        {
            byte[] intBytes = BitConverter.GetBytes(theInt);
			NetOrder(intBytes);
            theBytes.AddRange(intBytes);
        }

        public static int GetInt(BinaryReader reader)
        {
            byte[] intBytes = reader.ReadBytes(4);
			HostOrder(intBytes);
            return BitConverter.ToInt32(intBytes, 0);
        }

        public void AddLong(long theLong)
		{
			byte[] longBytes = BitConverter.GetBytes(theLong);
			NetOrder(longBytes);
			theBytes.AddRange(longBytes);
		}

		public long GetLong(BinaryReader reader)
		{
			byte[] longBytes = reader.ReadBytes(8);
			HostOrder(longBytes);
			return BitConverter.ToInt64(longBytes, 0);
		}

        public void AddVector(Vector3 theVector)
        {
            AddFloat(theVector.x);
            AddFloat(theVector.y);
            AddFloat(theVector.z);
        }

        public static Vector3 GetVector(BinaryReader reader)
        {
            float x = GetFloat(reader);
            float y = GetFloat(reader);
            float z = GetFloat(reader);
            return new Vector3(x, y, z);
        }

        public void AddQuaternion(Quaternion theQuat)
        {
            AddFloat(theQuat.x);
            AddFloat(theQuat.y);
            AddFloat(theQuat.z);
            AddFloat(theQuat.w);
        }

        public static Quaternion GetQuaternion(BinaryReader reader)
        {
            float x = GetFloat(reader);
            float y = GetFloat(reader);
            float z = GetFloat(reader);
            float w = GetFloat(reader);
            return new Quaternion(x, y, z, w);
        }

        public void AddString(string theString)
        {
            Debug.AssertFormat(theString.Length < 255,
                               "Attempted to send a string too large: {0}",
                               theString);

            List<byte> stringBytes = new List<byte>();
            byte theLength = (byte)theString.Length;
            foreach (char c in theString)
            {
                byte[] theCharacter = BitConverter.GetBytes(c);
				NetOrder(theCharacter);
                stringBytes.AddRange(theCharacter);
            }
            AddByte(theLength);
            theBytes.AddRange(stringBytes);
        }

        public static string GetString(BinaryReader reader)
        {
            byte theLength = reader.ReadByte();
            string theResult = "";
            while (theLength > 0)
            {
                byte[] theCharacter = reader.ReadBytes(2);
				HostOrder(theCharacter);
                theResult += BitConverter.ToChar(theCharacter, 0);
                theLength--;
            }
            Log.Debug("Decoded string: {0}", theResult);
            return theResult;
        }

        public void AssertReaderDone(BinaryReader reader)
        {
            Debug.AssertFormat(reader.PeekChar() == -1,
                               "Reader had bytes left: {0}",
                               reader.ToString());
        }

        public virtual void Pack()
        {
            AddMessageType(MessageType.Nothing);
        }

        public byte[] Serialize()
        {
            Pack();
			return theBytes.ToArray();
        }
    }
}
