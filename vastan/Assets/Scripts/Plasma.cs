using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Plasma : Projectile {
    public int energy = 100;
    public float speed = 30f;
    public AudioSource plasma_sound;

    private void Start() {
        exp_colors = new List<Color> {
            Color.red
        };
    }

    public void set_energy(int e) {
        energy = e;
        Color c = new Color(1f, 0, 0) * energy / 100;
        Mesh m = GetComponent<MeshFilter>().sharedMesh;
        var cs = from n in Enumerable.Range(0, m.vertices.Length) select c;
        m.colors = cs.ToArray();
    }
    private void Update() {
        if (!isActiveAndEnabled) { return; }
        var tmp = gameObject.transform.position;
        tmp += gameObject.transform.forward * speed * Time.deltaTime;
        gameObject.transform.localPosition = tmp;
        gameObject.transform.Rotate(0, 0, 200f * Time.deltaTime);

        decay(6f);

        restart_sound(.144f);

        RaycastHit hit_info;
        Debug.DrawRay(transform.position, transform.forward * .5f);
        var r = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(transform.position, transform.forward, out hit_info, .5f)) {

            var hit = hit_info.collider.gameObject;

            //var hit_player = hit.GetComponent<SceneCharacter3D>();
            //if (hit_player != null) {
            //    hit_player.state.velocity += transform.forward * 100f;
            //    asplode();
            //}

            var hit_ai = hit.GetComponent<AI3D>();
            if (hit_ai != null) {
                //hit_ai.state.momentum += transform.forward;
                hit_ai.state.momentum += transform.forward * 3f;
                Debug.Log("adding the thing");

                asplode();
            }

            var hit_static = hit.GetComponent<Static>();
            if (hit_static != null) {
                asplode();
            }
            else {
                asplode();
            }
            Debug.Log(hit);
        }
    }

}
