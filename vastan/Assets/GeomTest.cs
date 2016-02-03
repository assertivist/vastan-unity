using UnityEngine;
using System.Collections;

public class GeomTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GeomBuilder gb = new GeomBuilder();
        gb.init();
        gb.add_rect(new Color(200, 34, 34), new Vector3(1, 1, 1), new Vector3(-1, -1, -1));
        Mesh m = gb.build_mesh();
        GetComponent<MeshFilter>().mesh = m;
	}
	
	// Update is called once per frame
	void Update () {
        //transform.Rotate(0, 20, 0);
	}
}
