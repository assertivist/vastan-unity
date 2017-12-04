using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Grenade : Projectile {

    public AudioSource grenade_sound;
    public float power = 150f;

    public Vector3 attack_pos;
    public Vector3 attacker_speed;
    public Vector2 attacker_angles;
    public float attack_time;
	public float radius = .15f;

    public Vector3 initial_speed = new Vector3(85, 85);
    public float g = -65f;
    public float theta = 30f;

    void Start () {
        exp_colors = new List<Color> {
            Color.red,
            Color.yellow
        };
    }

	void OnDrawGizmos() {
		Gizmos.DrawWireSphere (transform.position, radius);
	}

	void Update () {
        if (!isActiveAndEnabled) { return; }
        float t = Time.time - attack_time;
        restart_sound(.1f);
        decay(6f);
        //RaycastHit hit_info;
		//Ray r1 = new Ray(transform.position, pos_for_t(t + Time.deltaTime));
		//Ray r2 = new Ray(pos_for_t (Mathf.Max(t - Time.deltaTime, 0)), transform.position);
        //Debug.DrawRay(r.origin, r.direction * 10f);
		var results = Physics.OverlapSphere (pos_for_t (t + (Time.deltaTime / 2)), radius).Concat (
			Physics.OverlapSphere (pos_for_t (t - (Time.deltaTime / 2)), radius)).Concat(
				Physics.OverlapSphere(transform.position, radius)).ToArray();

		Debug.DrawRay (transform.position, Vector3.up);
		//Debug.DrawRay (r1.origin, r1.direction);
		//Debug.DrawRay (r2.origin, r2.direction);
		// Physics.SphereCast(r1, radius) || Physics.SphereCast(r2, radius)
		if (results.Length > 0) {
            //var hit = hit_info.collider.gameObject;

			Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1.4f);
			int i = 0;
			while (i < hitColliders.Length)
			{
				var go = hitColliders[i].gameObject;
				var dist = go.transform.position - transform.position;
				var hitpower = Projectile.explosion_scale(power, dist);

				var hit_sc = go.GetComponent<SceneCharacter3D>();
				if (hit_sc != null) {
					if (dist.y < 0)
						hit_sc.crouch_spring.vel -= dist.normalized.y * hitpower;
					hit_sc.state.momentum += dist.normalized * hitpower;
				}
				var hit_p = go.GetComponent<Projectile>();
				if (hit_p != null)
					hit_p.peterout();				
				Debug.Log(go);
				Debug.DrawLine(go.transform.position, transform.position);
				i++;
			}
            asplode();
        }

        transform.position = pos_for_t(t);
    }

    Vector3 pos_for_t (float t) {
        var spd = initial_speed / 2.0f;
        var x = spd.x * t * Mathf.Cos(theta * Mathf.Deg2Rad);
        var y = ((.5f * g) * Mathf.Pow(t, 2)) + (spd.y * t * Mathf.Sin(theta * Mathf.Deg2Rad));
        return attack_pos + (transform.forward * x) + (transform.up * y);
    }
}
