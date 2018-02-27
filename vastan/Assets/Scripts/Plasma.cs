using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Plasma : Projectile {
    public float energy = 100;
    public Vector3 speed = Vector3.forward;
    public AudioSource plasma_sound;

    private void Start() {
        exp_colors = new List<Color> {
            Color.red
        };
    }

    public static Plasma Fire(SceneCharacter3D c, GameObject fab, float energy, int gun) {
        var pos = c.head.transform.position;
        pos += c.head.transform.forward * 1.4f;
        pos += c.head.transform.up * .49f * gun;
        pos += c.head.transform.right * -.3f;
        var rot = c.head.transform.rotation;
        var proj = (GameObject)GameObject.Instantiate(
            fab,
            pos,
            rot);
        var p = proj.GetComponent<Plasma>();
        p.set_energy(energy);
        p.fired_by = c.BaseCharacter.Id;
        p.speed = proj.transform.forward * 2f;
        return p;
    }

	public void set_energy(float e) {
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
		//tmp += gameObject.transform.forward * speed * Time.fixedDeltaTime * Game.AVARA_FPS;
		//gameObject.transform.localPosition = tmp;
		gameObject.transform.Rotate(0, 0, 200f * Time.fixedDeltaTime);

		RaycastHit hit_info;
		Debug.DrawRay(transform.position, transform.forward * .5f);
		//var r = new Ray(transform.position, transform.forward);
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
				hit_ai.state.momentum += transform.forward * energy * 3f;
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
