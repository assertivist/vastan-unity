using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Plasma : Projectile {
    public float max_energy = .8f;
    public float energy = .8f;
    public Vector3 speed = Vector3.forward;
    public AudioSource plasma_sound;

    private void Start() {
        exp_colors = new List<Color> {
            Color.red
        };
    }

    public static Plasma Fire(SceneCharacter3D c, GameObject fab) {
        int gun = 1;
        float energy = 0;
        WalkerPhysics s = c.state;
        if (s.plasma1 < s.plasma2) {
            if (s.plasma2 > s.min_plasma_power) {
                energy = s.plasma2;
                s.plasma2 = 0;
                gun = -1;
            }
        }
        else { // c.plasma1 >= c.plasma2
            if (s.plasma1 > s.min_plasma_power) {
                energy = s.plasma1;
                s.plasma1 = 0;
            }
        }
        var pos = c.head.transform.position;
        pos += c.head.transform.forward * 1.3f;
        pos += c.head.transform.up * .40f * gun;
        pos += c.head.transform.right * -.3f;
        var rot = c.head.transform.rotation;
        var proj = (GameObject)GameObject.Instantiate(fab, pos, rot);
        var p = proj.GetComponent<Plasma>();
        p.set_energy(energy);
        p.fired_by = c.BaseCharacter.Id;
        p.speed = proj.transform.forward * 2f;
        return p;
    }

    public void set_energy(float e) {
        //TODO: Use a material for crying out loud
        energy = e;
        Color c = new Color(1f, 0, 0) * energy;
        Mesh m = GetComponent<MeshFilter>().sharedMesh;
        var cs = from n in Enumerable.Range(0, m.vertices.Length) select c;
        m.colors = cs.ToArray();
    }
    private void Update() {
        if (!isActiveAndEnabled) return;
        decay(6f);
        restart_sound(.144f);
    }

    private void FixedUpdate() {
        if (!isActiveAndEnabled) { return; }
        gameObject.transform.position += speed * Time.fixedDeltaTime * Game.AVARA_FPS;
        gameObject.transform.Rotate(0, 0, 200f * Time.fixedDeltaTime);

        RaycastHit hit_info;
        Debug.DrawRay(transform.position, transform.forward * .5f);
        if (Physics.Raycast(transform.position, transform.forward, out hit_info, .5f)) {

            var hit = hit_info.collider.gameObject;

            var hit_player = hit.GetComponent<SceneCharacter3D>();
            if (hit_player != null) {
                hit_player.state.momentum += transform.forward * energy * 3f;
                hit_player.was_hit(energy, max_energy);
                asplode();
            }

            var hit_ai = hit.GetComponent<AI3D>();
            if (hit_ai != null) {
                hit_ai.state.momentum += transform.forward * energy * 3f;
                hit_ai.was_hit(energy, max_energy);
                asplode();
            }
 
            var hit_static = hit.GetComponent<Static>();
            if (hit_static != null) {
                hit_wall = true;
                asplode();
            }
            else {
                asplode();
            }
            Debug.Log(hit);
        }

    }

}
