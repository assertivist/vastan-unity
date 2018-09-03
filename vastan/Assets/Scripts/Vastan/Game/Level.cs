using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using Vastan.Util;

namespace Vastan.Game
{
    public enum ObjectType 
    {
        Static,
        Hologram,
        Ground,
        FreeSolid
    }
    public class Level
    {
        public string Name;
        public string Tagline;
        public string Author;
        public string Description;
        List<Mesh> Statics;
        List<Mesh> Holograms;
        List<Mesh> FreeSolids;

        static Random rng = new Random();

        GameObject StaticFab;
        GameObject GroundFab;
        GameObject CelestialFab;
        GameObject FreeSolidFab;

        int IncarnCount = 0;
        int LastIncarn = -1;

        List<Transform> incarns = new List<Transform>();
        GameObject parent;
        GeomBuilder current_gb = new GeomBuilder();

        private ObjectType CurrentType = ObjectType.Static;

        Mesh Ground;
        Color GroundColor;
        Camera MyCamera = null;
        bool IsClient = false;
        bool HasCelestials = false;

        public Level() 
        {

        }

        public Level (Camera cam)
        {
            MyCamera = cam;
            IsClient = true;
        }

        public static XmlNode GetMapnode(string path)
        {
            TextAsset ta = (TextAsset)Resources.Load(path);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(ta.text);
            return doc.DocumentElement;
        }
        /* 
        public static Dictionary<string, string> GetLevelInfo(string path)
        {
            
        }
        */
    }
}