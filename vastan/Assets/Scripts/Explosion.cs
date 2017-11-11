using UnityEngine;
using System.Collections;
using System.Linq;

public class Explosion : MonoBehaviour {
    public Color color = Color.black;

	// Use this for initialization
	void Start () {
        
	}

    public void set_color(Color c) {
        var ps = GetComponent<ParticleSystem>();
        ps.startColor = c;
    }
	
	// Update is called once per frame
	void Update () {
        var ps = GetComponent<ParticleSystem>();
        if (!ps.isPlaying) {
            Destroy(gameObject);
        }
	}
}
