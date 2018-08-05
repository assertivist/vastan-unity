using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Vastan.Networking {

    [Serializable]
    public class ObjectStateFrame {
        public Vector3 Position;
        public Vector3 Velocity;
    }

    [Serializable]
    public class CharacterStateFrame : ObjectStateFrame {
        public Quaternion HeadAngles;
        public float Heading;
        public float Crouch;
        public float Stance;
    }

    public class Serializer {
        public static byte[] BytesFromSerializable (System.Object o) {
            if (o == null) {
                return null;
            }
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream()) {
                formatter.Serialize(ms, o);
                return ms.ToArray();
            }
        }
    }
}