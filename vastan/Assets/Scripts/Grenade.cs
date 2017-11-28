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

    public Vector3 initial_speed = new Vector3(85, 85);
    public float g = -65f;
    public float theta = 30f;

    void Start () {
        exp_colors = new List<Color> {
            Color.red,
            Color.yellow
        };
    }
	
	void Update () {
        if (!isActiveAndEnabled) { return; }
        float t = Time.time - attack_time;
        restart_sound(.1f);
        decay(6f);
        RaycastHit hit_info;
        Ray r = new Ray(transform.position, pos_for_t(t+1));
        Debug.DrawRay(r.origin, r.direction * 10f);
        if (Physics.SphereCast(r, .2f, out hit_info, 1f)) {
            var hit = hit_info.collider.gameObject;
            
            //var hit_player = hit.GetComponent<SceneCharacter3D>();
            //if (hit_player != null) {
            //    return;
            //}
            
            var hit_ai = hit.GetComponent<AI3D>();
            if (hit_ai != null) {
                hit_ai.crouch_spring.vel += power;
                hit_ai.state.momentum.y = 3f;
                hit_ai.state.momentum += hit_info.normal * power * -1;
            }
            
            asplode();
            Debug.Log(hit_info.collider.gameObject);
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
