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

    public Vector3 initial_speed;
    public float g = -30f;
    public float theta = 30;

    void Start () {
        exp_colors = new List<Color> {
            Color.red,
            Color.yellow
        };
        initial_speed = new Vector3(10, 10);
    }
	
	void Update () {
        if (!isActiveAndEnabled) { return; }
        float t = Time.time - attack_time;
        transform.position = pos_for_t(t);
	}

    Vector3 pos_for_t (float t) {
        var x = initial_speed.x * t * Mathf.Cos(theta);
        var y = ((.5f * g) * Mathf.Pow(t, 2)) + (initial_speed.y * t * Mathf.Sin(theta));
        return attack_pos + (transform.forward * x) + (transform.up * y);
    }
}
