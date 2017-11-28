using UnityEngine;
using System.Collections;

public class Goody : MonoBehaviour {
    public int grenades = 0;
    public int missiles = 0;
    public int boosters = 0;

    public Vector3 spin = new Vector3(0, 20, 0);
    public float respawn = 8f;

    public bool active = true;
    public bool taken = false;

	// Use this for initialization
	void Start () {
	
	}

    void set_mesh(Mesh m) {
        var mymf = GetComponent<MeshFilter>();
        mymf.mesh = m;
    }
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(spin * Time.deltaTime);
	}
}
