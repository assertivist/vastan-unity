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
        restart_sound(.1f);
        decay(6f);
        float t = Time.time - attack_time;
        transform.position = pos_for_t(t);
    }


    void OnTriggerEnter(Collider other) {
        var sc = other.gameObject.GetComponent<SceneCharacter>();
        if (sc != null)
        {
            var other_id = sc.BaseCharacter.Id;
            if (fired_by == other_id)
            {
                Debug.Log("Not hitting self!");
                return;
            }
        }
        Debug.Log("fired by: " + fired_by);
        Debug.Log("hit: " + other.gameObject.GetInstanceID() + other.gameObject.name);
        asplode_force();
		asplode();
	}

	void asplode_force() {
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, 3.0f);
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
			i++;
		}
	}

	void FixedUpdate() {
		
	}

    Vector3 pos_for_t (float t) {
        var spd = initial_speed / 2.0f;
        var x = spd.x * t * Mathf.Cos(theta * Mathf.Deg2Rad);
        var y = ((.5f * g) * Mathf.Pow(t, 2)) + (spd.y * t * Mathf.Sin(theta * Mathf.Deg2Rad));
        return attack_pos + (transform.forward * x) + (transform.up * y);
    }
}
