using UnityEngine;
using System.Collections;
using System.Xml;

public class GeomTest : MonoBehaviour {

    // Use this for initialization
    void Start () {
        test_xml();
    }

    void test_xml() {
        Level l = new Level();
        l.load("indra");
        GameObject l_root = l.game_object();
        l_root.transform.SetParent(transform);
    }
	
	// Update is called once per frame
	void Update () {
        //transform.Rotate(0, .01f * Time.deltaTime, 0);
	}
}
