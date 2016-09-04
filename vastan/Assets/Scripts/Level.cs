using UnityEngine;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class Level {

    public string name;
    public string tagline;
    public string author;
    public string description;

    public List<Mesh> statics;
    public List<Mesh> holograms;

    private GameObject static_fab;

    private int incarn_count = 0;

    private GameObject parent;

    private GeomBuilder current_gb = new GeomBuilder();
    private bool current_is_hologram = false;

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

        current_gb.init();
        statics = new List<Mesh>();
        holograms = new List<Mesh>();

        parse_node(mapnode);
    }

    private void cycle_mesh(bool is_hologram) {
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
        }
    }

    private void parse_node(XmlNode parent_node)  {
        foreach (XmlNode node in parent_node.ChildNodes) {
            switch (node.Name)  {

                case "static":
                    cycle_mesh(current_is_hologram);
                    parse_node(node);
                    break;

                case "hologram":
                    cycle_mesh(false);
                    parse_node(node);
                    cycle_mesh(true);
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
                    parse_ground(node);
                    break;
                case "incarnator":
                    parse_incarnator(node);
                    break;
                default:
                    break;
            }
        }
    }
    private void parse_block(XmlNode block_node) {
        Vector3 center = parse_vec3(block_node.Attributes["center"]);
        Vector3 size = parse_vec3(block_node.Attributes["size"]);
        Color c = parse_color(block_node);
        Quaternion rot = Quaternion.identity;

        Vector3 angles = rot.eulerAngles;
        angles.x += parse_float(block_node, "pitch");
        angles.y += parse_float(block_node, "yaw");
        angles.z += parse_float(block_node, "roll");
        rot.eulerAngles = angles;
        
        current_gb = current_gb.add_block(c, center, size, rot);
        
    }

    private void parse_ramp(XmlNode ramp_node) {
        Vector3 ramp_base = parse_vec3(ramp_node.Attributes["base"]);
        Vector3 ramp_top = parse_vec3(ramp_node.Attributes["top"]);
        float width = parse_float(ramp_node, "width");
        float thickness = parse_float(ramp_node, "thickness");
        Color c = parse_color(ramp_node);
        Quaternion rot = Quaternion.identity;

        current_gb.add_ramp(c, ramp_base, ramp_top, width, thickness, rot);
    }

    private void parse_wedge(XmlNode wedge_node) {
        Vector3 wedge_base = parse_vec3(wedge_node.Attributes["base"]);
        Vector3 wedge_top = parse_vec3(wedge_node.Attributes["top"]);
        float width = parse_float(wedge_node, "width");
        Color c = parse_color(wedge_node);
        Quaternion rot = Quaternion.identity;

        current_gb.add_wedge(c, wedge_base, wedge_top, width, rot);

    }

    private void parse_incarnator(XmlNode incarn_node) {
        Vector3 incarn_pos = parse_vec3(incarn_node.Attributes["location"]);
        float rot = parse_float(incarn_node, "heading");

        GameObject nsp = new GameObject("incarn_"+incarn_count);
        incarn_count++;
        nsp.transform.Translate(incarn_pos);
        nsp.transform.Rotate(new Vector3(0, rot, 0));
        nsp.transform.SetParent(parent.transform);

        nsp.AddComponent<NetworkStartPosition>();
    }

    private void parse_dome(XmlNode dome_node) {

    }

    private void parse_ground(XmlNode ground_node) {
        Color c = parse_color(ground_node);
        current_gb.add_block(c, new Vector3(0, -.01f, 0), new Vector3(1000, .01f, 1000), Quaternion.identity);
    }

    private Color parse_color(XmlNode node) {
        XmlAttribute x = node.Attributes["color"];
        if (x == null) return new Color(1, 1, 1);
        string value = x.Value;
        string[] values = value.Split(',');
        float r = float.Parse(values[0]);
        float g = float.Parse(values[1]);
        float b = float.Parse(values[2]);
        return new Color(r, g, b);
    }

    private Vector3 parse_vec3(XmlAttribute vec3_attrib) {
        string value = vec3_attrib.Value;
        string[] values = value.Split(',');
        float x = float.Parse(values[2]);
        float y = float.Parse(values[1]);
        float z = float.Parse(values[0]);
        return new Vector3(x, y, z);
    }

    private static string parse_string(XmlNode n, string key) {
        if (n.Attributes[key] == null) {
            return "";
        }
        else return n.Attributes[key].Value;
    }

    private static float parse_float(XmlNode n, string key) {
        if (n.Attributes[key] == null) {
            return 0;
        }
        else return float.Parse(n.Attributes[key].Value);
    }

    public GameObject game_object() {
        cycle_mesh(current_is_hologram);
        foreach (Mesh m in statics) {
            GameObject geom = GameObject.Instantiate(static_fab, Vector3.zero, Quaternion.identity) as GameObject;
            geom.GetComponent<MeshFilter>().mesh = m;
            geom.AddComponent<Static>();

            geom.AddComponent<MeshCollider>();
            var mc = geom.GetComponent<MeshCollider>();
            mc.sharedMesh = m;


            geom.transform.SetParent(parent.transform);

        }
        foreach (Mesh m in holograms) {
            GameObject geom = GameObject.Instantiate(static_fab, Vector3.zero, Quaternion.identity) as GameObject;
            geom.GetComponent<MeshFilter>().mesh = m;
            geom.transform.SetParent(parent.transform);
        }
        return parent;
    }
}
