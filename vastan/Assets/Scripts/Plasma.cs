using UnityEngine;
using System.Collections;
using System.Linq;

public class Plasma : MonoBehaviour {
    public int energy = 100;
    public float decay = 0;
    public float speed = 25f;

    public AudioSource plasma_sound;

    private void Start() {
        Color c = new Color(.7f, 0, 0);
        Mesh m = GetComponent<MeshFilter>().sharedMesh;
        var colors = from n in Enumerable.Range(0, m.vertices.Length) select c;
        m.colors = colors.ToArray();
    }

    private void Update() {
        if (!isActiveAndEnabled) { return; }
        var tmp = gameObject.transform.position;
        tmp += gameObject.transform.forward * speed * Time.deltaTime;
        gameObject.transform.localPosition = tmp;

        decay += Time.deltaTime;
        if (decay > 6f) {
            peterout();
        }

        var sound = GetComponent<AudioSource>();
        if(sound && !sound.isPlaying && sound.isActiveAndEnabled) {
            sound.time = .144f;
            sound.Play();
        }

        RaycastHit hit_info;
        Debug.DrawRay(transform.position, transform.forward * .3f);
        var r = new Ray(transform.position, transform.forward);
        if (Physics.Raycast(transform.position, transform.forward, out hit_info, .3f)) {

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
            Debug.Log(hit);
        }
    }

    void peterout() {
        Destroy(gameObject);
    }

    void asplode() {
        Destroy(gameObject);
    }
}
