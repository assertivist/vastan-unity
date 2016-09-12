using UnityEngine;
using System.Collections;

public class Plasma : MonoBehaviour {
    public int energy = 0;
    void OnCollisionEnter(Collision collision) {
        var hit = collision.gameObject;
        var hit_player = hit.GetComponent<VastanPlayer>();
        if (hit_player != null) {

            var ps = hit_player.GetComponent<PlayerState>();
            ps.take_damage(energy);

            Destroy(gameObject);
            
        }
        var hit_static = hit.GetComponent<Static>();
        if (hit_static != null) {
            Destroy(gameObject);
        }
    }
}
