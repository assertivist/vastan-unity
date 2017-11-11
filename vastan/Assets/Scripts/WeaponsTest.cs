using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class WeaponsTest : MonoBehaviour {
    public GameObject walker;
    public GameObject floor;
    public GameObject side_spot;
    public GameObject ai;
    public Camera cam;
    private bool cam_is_static = true;
    private SceneCharacter3D walker_char;

    public GameObject TriangleExplosionPrefab;

    public List<Plasma> Plasmas { get; set; }

    public GameObject plasma_prefab;

    Vector2 _smoothMouse;
    public Vector2 sensitivity = new Vector2(3, 3);
    public Vector2 smoothing = new Vector2(3, 3);
    
    // Use this for initialization
    void Start() {
        walker_char = walker.GetComponent<SceneCharacter3D>();
        // Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        walker_char.recolor_walker(new Color(.7f, 0f, .3f));

        Color c = new Color(.2f, .2f, .2f);
        Mesh m = floor.GetComponent<MeshFilter>().sharedMesh;
        var colors = from n in Enumerable.Range(0, m.vertices.Length) select c;
        m.colors = colors.ToArray();

        var ai3d = ai.GetComponent<AI3D>();
        ai3d.Target = walker_char;
        ai3d.state.on_ground = true;

        Plasmas = new List<Plasma>();

    }

    void make_plasma(SceneCharacter3D character) {
        var pos = character.head.transform.position;
        pos += character.head.transform.forward * 1.1f;
        var plasma = (GameObject)GameObject.Instantiate(
            plasma_prefab, 
            pos, 
            character.head.transform.rotation);
        Plasmas.Add(plasma.GetComponent<Plasma>());
    }

    // Update is called once per frame
    void Update() {
        var ai3d = ai.GetComponent<AI3D>();
        ai3d.RunAtTarget();
        ai3d.state.on_ground = true;
        var turn = Input.GetAxis("Horizontal");
        walker_char.Move(Input.GetAxis("Vertical"), turn, Time.deltaTime, Input.GetButton("Jump"));

        // Get raw mouse input for a cleaner reading on more sensitive mice.
        var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        // Scale input against the sensitivity setting and multiply that against the smoothing value.
        mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

        // Interpolate mouse movement over time to apply smoothing delta.
        _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
        _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);
        walker_char.Look(_smoothMouse.x, _smoothMouse.y);

        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (cam_is_static) {
                attach_cam_to_walker(walker_char);
            }
            else {
                cam.transform.position = side_spot.transform.position;
                cam.transform.eulerAngles = new Vector3(0, 90, 0);
                cam.transform.SetParent(side_spot.transform);
            }
            cam_is_static = !cam_is_static;
        }

        if (Input.GetMouseButtonDown(0)) {
            make_plasma(walker_char);
        }
        HandlePlasmas();
    }
    void attach_cam_to_walker(SceneCharacter3D walker) {
        cam.transform.position = walker.head.transform.position;
        cam.transform.rotation = walker.head.transform.rotation;
        var angles = cam.transform.eulerAngles;
        angles.z -= 90;
        cam.transform.eulerAngles = angles;

        cam.transform.SetParent(walker.head.transform);
    }

    void HandlePlasmas() {
        foreach (var p in Plasmas) {
            if (p.hit_something) {
                var pos = p.transform.position;
                var exp = (GameObject)GameObject.Instantiate(
                    TriangleExplosionPrefab,
                    pos,
                    Quaternion.identity);
                exp.GetComponent<Explosion>().set_color(Color.red);
            }
            if (!p.alive) {
                Destroy(p.gameObject);
            }
        }
        Plasmas = (from p in Plasmas where p.alive select p).ToList();
    }
}
