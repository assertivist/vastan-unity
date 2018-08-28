using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Vastan.Util;
using Vastan.Game;

namespace Vastan.Networking
{
	public enum MessageType : byte
	{
		Nothing = 0,
		ClientJoin = 1,
		ClientLeave = 2,
		ClientConfig = 3,
		ClientChat = 4,
		Chat = 5,
        PlayerConfig = 6,
        ClientInput = 7,
        ObjectStateFrame = 8,
        CharacterStateFrame = 9,
    }

	public class ClientJoin : SerializedMessage 
	{
		// Server -> Client
		public byte Id;
		public string Name;
		public string Address;
        // Client is a server only class
		public ClientJoin(byte id, Client client)
		{
			Id = id;
			Name = client.Name;
			Address = client.Info.Address;
		}

        public ClientJoin(BinaryReader reader)
		{
			Id = GetByte(reader);
			Name = GetString(reader);
			Address = GetString(reader);
			AssertReaderDone(reader);
		}

		public override void Pack()
		{
			AddMessageType(MessageType.ClientJoin);
			AddByte(Id);
			AddString(Name);
			AddString(Address);
		}
	}

	public class ClientConfig : SerializedMessage
	{
		// Client -> Server
		public string Name;

		public ClientConfig(string name)
		{
			Name = name;	
		}

		public ClientConfig(BinaryReader reader)
		{
			Name = GetString(reader);
			AssertReaderDone(reader);
		}

		public override void Pack()
		{
			AddMessageType(MessageType.ClientConfig);
			AddString(Name);
		}
	}

	public class Chat : SerializedMessage
	{
		// Client -> Server
		public string Chars;

		public Chat(string chars)
		{
			Chars = chars;
		}

		public Chat(BinaryReader reader)
		{
			Chars = GetString(reader);
			AssertReaderDone(reader);
		}

		public override void Pack()
		{
			AddMessageType(MessageType.Chat);
			AddString(Chars);
		}
	}

	public class ClientChat : SerializedMessage
    {
		// Server -> Client
		public byte Id;
        public string Chars;

		public ClientChat(byte id, string chars)
        {
			Id = id;
            Chars = chars;
        }

		public ClientChat(BinaryReader reader)
        {
			Id = GetByte(reader);
            Chars = GetString(reader);
            AssertReaderDone(reader);
        }

        public override void Pack()
        {
            AddMessageType(MessageType.ClientChat);
			AddByte(Id);
            AddString(Chars);
        }
    }

	public class ClientLeave : SerializedMessage
	{
		public byte Id;
        
        public ClientLeave(byte id)
		{
			Id = id;
		}

		public ClientLeave(BinaryReader reader)
		{
			Id = GetByte(reader);
			AssertReaderDone(reader);
		}

		public override void Pack()
		{
			AddMessageType(MessageType.ClientLeave);
			AddByte(Id);
		}
	}

	public class ClientInput : SerializedMessage
	{
		// Client -> Server
		public long Buttons;
        
        public ClientInput(long buttons)
		{
			Buttons = buttons;
		}

		public ClientInput(BinaryReader reader)
		{
			Buttons = GetLong(reader);
            AssertReaderDone(reader);
		}

		public override void Pack()
		{
			AddMessageType(MessageType.ClientInput);
			AddLong(Buttons);
		}
	}

	public class ObjectStateFrame : SerializedMessage
	{
		public Vector3 Position;
		public Vector3 Velocity;
      
        public ObjectStateFrame(Vector3 position, Vector3 velocity)
		{
			Position = position;
			Velocity = velocity;
		}

		public ObjectStateFrame(BinaryReader reader) {
			Position = GetVector(reader);
			Velocity = GetVector(reader);
            AssertReaderDone(reader);
		}

		public override void Pack()
		{
			AddMessageType(MessageType.ObjectStateFrame);
			AddVector(Position);
			AddVector(Velocity);
		}
    }
    
    public class CharacterStateFrame : ObjectStateFrame
	{
		public Quaternion HeadAngles;
		public float Heading;
		public float Elevation;

		public CharacterStateFrame(VastanCharacter theChar) : 
		    base(theChar.transform.position, theChar.state.velocity)
		{
			HeadAngles = theChar.head.transform.rotation;
			Heading = theChar.transform.eulerAngles.y;
			Elevation = theChar.state.elevation;
		}

		public CharacterStateFrame(BinaryReader reader) :
            base(GetVector(reader), GetVector(reader))
		{
			HeadAngles = GetQuaternion(reader);
			Heading = GetFloat(reader);
			Elevation = GetFloat(reader);
		}

		public override void Pack()
		{
			base.Pack();
			theBytes[0] = (byte)MessageType.CharacterStateFrame;
			AddQuaternion(HeadAngles);
			AddFloat(Heading);
			AddFloat(Elevation);
		}
	}
}