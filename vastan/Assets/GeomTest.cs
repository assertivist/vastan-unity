using UnityEngine;
using System.Collections;
using System.Xml;

public class GeomTest : MonoBehaviour {

    // Use this for initialization
    void Start () {
        test_xml();
    }

    void test() {
        GeomBuilder gb = new GeomBuilder();
        gb.init();
        Quaternion rot = Quaternion.identity;
        rot.eulerAngles = new Vector3(0, 0, 0);
        var pos = new Vector3(0, 0, 0);
        var size = new Vector3(2, 2, 2);
        var black = new Color(.75f, .2f, .2f);

        gb.add_block(black, pos, size, rot);

        pos = new Vector3(0, 1, 2);
        size = new Vector3(2, 4, 2);
        gb.add_block(black, pos, size, rot);

        pos = new Vector3(2, 2, 0);
        size = new Vector3(2, 6, 2);
        gb.add_block(black, pos, size, rot);

        pos = new Vector3(0, -1, 0);
        size = new Vector3(100, 1, 100);
        gb.add_block(new Color(.1f, .1f, .3f, .2f), pos, size, rot);

        Mesh m = gb.get_mesh();
        GetComponent<MeshFilter>().mesh = m;
    }

    void test_xml() {
        Level l = new Level();
        l.load("indra");
        GameObject l_root = l.game_object();
        l_root.transform.SetParent(transform);
    }
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(0, .01f, 0);
	}
}
