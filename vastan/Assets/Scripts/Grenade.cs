using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Grenade : Projectile {

    public AudioSource grenade_sound;
    public float power = 150f;
    
	public float radius = .15f;

    private float gravity = .120f;
    private float friction = .01f;

    public Vector3 speed = new Vector3(0, 0, 0);
    public float g = -65f;
    public float attack_time;
    
    void Start () {
        
        exp_colors = new List<Color> {
            Color.red,
            Color.yellow
        };
    }

    public static Grenade Fire(SceneCharacter3D c, GameObject fab) {
        var pos = c.head.transform.position;
        pos += c.head.transform.forward * 1.4f;
        var rot = c.head.transform.rotation.eulerAngles;
        var quat = Quaternion.Euler(rot.x, rot.y, 0);
        var go = (GameObject)GameObject.Instantiate(
            fab,
            pos,
            quat);
        Debug.Log(go);
        var grenade = go.GetComponent<Grenade>();
        var t = go.transform;
        grenade.attack_time = Time.time;
        grenade.speed = c.state.velocity;
        grenade.speed += ((t.forward * 2) + t.up);
        grenade.fired_by = c.BaseCharacter.Id;
        return grenade;
    }
    

	void Update () {
        if (!isActiveAndEnabled) { return; }
        restart_sound(.1f);

        //float t = Time.time - attack_time;
        //transform.position = pos_for_t(t);
        var dt = Time.fixedDeltaTime * Game.AVARA_FPS;

        speed.x -= speed.x * friction * dt;
        speed.y -= gravity + (speed.y * friction) * dt;
        speed.z -= speed.z * friction * dt;

        var pos = transform.position;
        pos.x += speed.x * dt;
        pos.y += speed.y * dt;
        pos.z += speed.z * dt;
        transform.position = pos;

        //explode after 100 "frames"
        if ((Time.time - attack_time) / Game.AVARA_FPS > 100) {
            asplode_force();
            asplode();
        }
    }

    void FixedUpdate() {

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

    /* Vector3 pos_for_t (float t) {
        var spd = initial_speed / 2.0f;
        var x = spd.x * t * Mathf.Cos(theta * Mathf.Deg2Rad);
        var y = ((.5f * g) * Mathf.Pow(t, 2)) + (spd.y * t * Mathf.Sin(theta * Mathf.Deg2Rad));
        return attack_pos + (transform.forward * x) + (transform.up * y);
    }
    */
}
