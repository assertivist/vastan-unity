using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public enum ObjectType {
	Static,
	Hologram,
	Ground
}

public class Level {

    public string name;
    public string tagline;
    public string author;
    public string description;

    public List<Mesh> statics;
    public List<Mesh> holograms;

    private static Random rng = new Random();

    private GameObject static_fab;
	private GameObject ground_fab;

    public int incarn_count = 0;
    private int last_incarn = -1;
    public List<Transform> incarns = new List<Transform>();

    private GameObject parent;

    private GeomBuilder current_gb = new GeomBuilder();
    private bool current_is_hologram = false;
	private ObjectType current_type = ObjectType.Static;

	private Mesh ground;
    private Color ground_color;

    public static XmlNode get_mapnode(string path)
    {
        TextAsset ta = (TextAsset)Resources.Load(path);
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(ta.text);
        return doc.DocumentElement;
    }

    public static Dictionary<string, string> get_levelinfo(string path)
    {
        XmlNode mapnode = get_mapnode(path);
        string name = parse_string(mapnode, "name");
        string tagline = parse_string(mapnode, "tagline");
        string author = parse_string(mapnode, "author");
        string description = parse_string(mapnode, "description");
        Dictionary<string, string> levelinfo = new Dictionary<string, string>()
        {
            { "name",  name },
            { "tagline", tagline },
            { "author", author },
            { "description", description }
        };
        return levelinfo;

    }

    public void load(string level) {
        parent = new GameObject("LevelRoot");
        XmlNode mapnode = get_mapnode(level);
        name = parse_string(mapnode, "name");
        Debug.Log("Loading Level " + name);

        static_fab = Resources.Load("LevelGeometry", typeof(GameObject)) as GameObject;
		ground_fab = Resources.Load("GroundGeometry", typeof(GameObject)) as GameObject;

        current_gb.init();
        statics = new List<Mesh>();
        holograms = new List<Mesh>();

        parse_node(mapnode);
    }

    private void cycle_mesh(ObjectType type) {
		switch (type) {
			case ObjectType.Hologram:
				holograms.Add(current_gb.get_mesh());
				break;
			case ObjectType.Static:
				statics.Add(current_gb.get_mesh());
				break;
			case ObjectType.Ground:
				ground = current_gb.get_mesh();
				break;
		}

		current_gb = new GeomBuilder();
		current_gb.init();
        /*
		if (current_gb != null) {
            if (is_hologram) {
                holograms.Add(current_gb.get_mesh());
                current_gb = new GeomBuilder();
                current_gb.init();
            }
            else {
                statics.Add(current_gb.get_mesh());
                current_gb = new GeomBuilder();
                current_gb.init();
            }
        }
        else {
            current_gb = new GeomBuilder();
            current_gb.init();
        }*/
    }

    private void parse_node(XmlNode parent_node)  {
        foreach (XmlNode node in parent_node.ChildNodes) {
            switch (node.Name) {

                case "sky":
                    parse_sky(node);
                    parse_node(node);
                    break;
                case "celestial":
                    parse_celestial(node);
                    break;
                case "static":
					cycle_mesh(current_type);
                    parse_node(node);
                    break;
                case "hologram":
                    cycle_mesh(current_type);
                    parse_node(node);
                    cycle_mesh(ObjectType.Hologram);
                    break;
                case "block":
                    parse_block(node);
                    break;
                case "ramp":
                    parse_ramp(node);
                    break;
                case "wedge":
                    parse_wedge(node);
                    break;
                case "dome":
                    parse_dome(node);
                    break;
				case "ground":
					cycle_mesh(current_type);
					parse_ground(node);
					cycle_mesh(ObjectType.Ground);
                    break;
                case "incarnator":
                    parse_incarnator(node);
                    break;
                case "goody":
                    parse_goody(node);
                    break;
                default:
                    break;
            }
        }
    }
    private void parse_block(XmlNode node) {
        Vector3 center = parse_vec3(node, "center");
        Vector3 size = parse_vec3(node, "size", new Vector3(4, 4, 4));
        Color c = parse_color(node);
        Quaternion rot = parse_euler_angles(node);
        current_gb = current_gb.add_block(c, center, size, rot);
    }

    private void parse_ramp(XmlNode node) {
        Vector3 ramp_base = parse_vec3(node, "base");
        Vector3 ramp_top = parse_vec3(node, "top", new Vector3(0, 4, 4));
        float width = parse_float(node, "width", 8.0f);
        float thickness = parse_float(node, "thickness");
        Color c = parse_color(node);
        Quaternion rot = parse_euler_angles(node);
        current_gb.add_ramp(c, ramp_base, ramp_top, width, thickness, rot);
    }

    private void parse_wedge(XmlNode node) {
        Vector3 wedge_base = parse_vec3(node, "base");
        Vector3 wedge_top = parse_vec3(node, "top", new Vector3(0, 4, 4));
        float width = parse_float(node, "width", 8.0f);
        Color c = parse_color(node);
        Quaternion rot = parse_euler_angles(node);

        current_gb.add_wedge(c, wedge_base, wedge_top, width, rot);

    }

    private void parse_incarnator(XmlNode node) {
        Vector3 incarn_pos = parse_vec3(node, "location");
        float rot = parse_float(node, "heading");

        GameObject nsp = new GameObject("incarn_"+incarn_count);
        incarn_count++;
        nsp.transform.Translate(incarn_pos);
        nsp.transform.Rotate(new Vector3(0, rot, 0));
        nsp.transform.SetParent(parent.transform);

        nsp.AddComponent<NetworkStartPosition>();
        incarns.Add(nsp.transform);
    }

    private void parse_dome(XmlNode node) {
        Vector3 center = parse_vec3(node, "center");
        float radius = parse_float(node, "radius", 2.5f);
        int samples = parse_int(node, "samples", 8);
        int planes = parse_int(node, "planes", 5);
        Color c = parse_color(node);
        Quaternion rot = parse_euler_angles(node);
        current_gb.add_dome(c, center, radius, samples, planes, rot);
    }

    private void parse_ground(XmlNode node) {
        Color c = parse_color(node);
        RenderSettings.skybox.SetColor("_GroundColor", c);
        DynamicGI.UpdateEnvironment();
		current_gb.add_block(c, 
            new Vector3(0, -10f, 0), 
            new Vector3(1000, 19.992f, 1000), 
            Quaternion.identity);
        ground_color = c;
    }

    private void parse_sky(XmlNode node) {
        Color sky = parse_color(node);
        Color horizon;
        XmlAttribute x = node.Attributes["horizon"];
        if (x == null) horizon = new Color(1, 1, 1);
        else horizon = parse_color_value(x.Value);
 
        float scale = parse_float(node, "horizonScale");

        RenderSettings.skybox.SetColor("_SkyColor", sky);
        RenderSettings.skybox.SetColor("_HorizColor", horizon);
        RenderSettings.skybox.SetFloat("_GradientHeight", scale);
        DynamicGI.UpdateEnvironment();
    }

    private void parse_celestial(XmlNode node) {
        float azimuth = parse_float(node, "azimuth");
        float elevation = parse_float(node, "elevation");
        Color color = parse_color(node);
        float intensity = parse_float(node, "intensity");
        bool visible = parse_bool(node, "visible");
        float size = parse_float(node, "size", 100f);
        //TODO: celestial prefab
    }

    private void parse_goody(XmlNode node) {
        string model = parse_string(node, "model", "AvaraA");
        Vector3 location = parse_vec3(node, "location");
        int grenades = parse_int(node, "grenades");
        int missiles = parse_int(node, "missiles");
        int boosters = parse_int(node, "boosters");
        float respawn = parse_float(node, "respawn");
        Vector3 spin = parse_vec3(node, "spin");
        //TODO: goody prefab
    }

    private Color parse_color(XmlNode node) {
        XmlAttribute x = node.Attributes["color"];
        if (x == null) return new Color(1, 1, 1);
        string value = x.Value;
        return parse_color_value(value);
    }

    private Color parse_color_value(string color) { 
        string[] values = color.Split(',');
        float r = float.Parse(values[0]);
        float g = float.Parse(values[1]);
        float b = float.Parse(values[2]);
        return new Color(r, g, b);
    }

    private Quaternion parse_euler_angles(XmlNode n) {
        var rot = Quaternion.identity;
        Vector3 angles = rot.eulerAngles;
        angles.x += parse_float(n, "pitch");
        // ``LEFT-HANDED''
        angles.y -= parse_float(n, "yaw");
        angles.z += parse_float(n, "roll");
        rot.eulerAngles = angles;
        return rot;
    }

    private Vector3 parse_vec3(XmlNode n, string key) {
        return parse_vec3(n, key, new Vector3(0, 0, 0));
    }

    private Vector3 parse_vec3(XmlNode n, string key, Vector3 def) {
        if (n.Attributes[key] == null) {
            return def;
        }

        string value = n.Attributes[key].Value;
        string[] values = value.Split(',');
        float x = float.Parse(values[2]);
        float y = float.Parse(values[1]);
        float z = float.Parse(values[0]);
        return new Vector3(x, y, z);
    }

    private static bool parse_bool(XmlNode n, string key, bool def = false) {
        if (n.Attributes[key] == null) {
            return def;
        }
        else return bool.Parse(n.Attributes[key].Value);
    }

    private static string parse_string(XmlNode n, string key, string def = "") {
        if (n.Attributes[key] == null) {
            return def;
        }
        else return n.Attributes[key].Value;
    }

    private static float parse_float(XmlNode n, string key, float def = 0.0f) {
        if (n.Attributes[key] == null) {
            return def;
        }
        else return float.Parse(n.Attributes[key].Value);
    }

    private static int parse_int(XmlNode n, string key, int def = 0) {
        if (n.Attributes[key] == null) {
            return def;
        }
        else {
            return int.Parse(n.Attributes[key].Value);
        }
    }

    public GameObject game_object() {
        cycle_mesh(current_type);
        foreach (Mesh m in statics) {
            var go = GameObject.Instantiate(static_fab, Vector3.zero, Quaternion.identity);
            GameObject geom = go as GameObject;
            geom.GetComponent<MeshFilter>().mesh = m;
            geom.AddComponent<Static>();

			var mc = geom.AddComponent<MeshCollider>();
            mc.sharedMesh = m;

            geom.transform.SetParent(parent.transform);

        }
        foreach (Mesh m in holograms) {
            var go = GameObject.Instantiate(static_fab, Vector3.zero, Quaternion.identity);
            GameObject geom = go as GameObject;
            geom.GetComponent<MeshFilter>().mesh = m;
            geom.transform.SetParent(parent.transform);
        }

		GameObject ground_go = GameObject.Instantiate(ground_fab, Vector3.zero, Quaternion.identity) as GameObject;
		ground_go.GetComponent<MeshFilter>().mesh = ground;
		BoxCollider b = ground_go.AddComponent<BoxCollider>();
		b.center = new Vector3 (0, -10, 0);
		b.size = new Vector3 (1000, 20, 1000);
		ground_go.transform.SetParent(parent.transform);
        var mat = ground_go.GetComponent<Renderer>().material;
        mat.SetColor(Shader.PropertyToID("_Color"), ground_color);
        return parent;
    }

    public Transform get_incarn() {
        if (last_incarn < 0) {
            return incarns[Random.Range(0, incarns.Count)];
        }
        else {
            int v = Random.Range(0, incarns.Count);
            while (v != last_incarn) {
                v = Random.Range(0, incarns.Count);
            }
            last_incarn = v;
            return incarns[v];
        }
    }
}
