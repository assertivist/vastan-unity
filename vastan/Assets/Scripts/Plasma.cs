using UnityEngine;
using System.Collections;
using System.Linq;

public class Plasma : MonoBehaviour {
    public int energy = 0;
    public float decay = 0;

    private void Start() {
        Color c = new Color(.7f, 0, 0);
        Mesh m = GetComponent<MeshFilter>().sharedMesh;
        var colors = from n in Enumerable.Range(0, m.vertices.Length) select c;
        m.colors = colors.ToArray();
    }

    private void Update() {
        var tmp = gameObject.transform.position;
        tmp += gameObject.transform.forward * 15f * Time.deltaTime;
        gameObject.transform.localPosition = tmp;

        decay += Time.deltaTime;
        if (decay > 10f) {
            peterout();
        }

        var sound = GetComponent<AudioSource>();
        if(sound && !sound.isPlaying) {
            sound.time = .144f;
            sound.Play();
        }
    }

    void peterout() {
        Destroy(gameObject);
    }

    void asplode() {
        Destroy(gameObject);
    }


    void OnCollisionEnter(Collision collision) {
        var hit = collision.gameObject;
        var hit_player = hit.GetComponent<VastanPlayer>();
        if (hit_player != null) {

            var ps = hit_player.GetComponent<PlayerState>();
            ps.take_damage(energy);
            asplode();
            
        }
        var hit_static = hit.GetComponent<Static>();
        if (hit_static != null) {
            asplode();
        }
        Debug.Log(hit);
    }
}
