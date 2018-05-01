using UnityEngine;
using System.Collections;
using System.Linq;

public class WalkerTest : MonoBehaviour {
    public GameObject walker;
    public GameObject floor;
    public Camera cam;
    public GameObject side_spot;
    private bool cam_is_static = true;
    private SceneCharacter3D walker_char;

    Vector2 _smoothMouse;
    public Vector2 sensitivity = new Vector2(3, 3);
    public Vector2 smoothing = new Vector2(3, 3);
    // Use this for initialization
    void Start() {
        walker_char = walker.GetComponent<SceneCharacter3D>();
        // Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        //Game.recolor_walker(walker, new Color(.7f, 0f, .3f));

        Color c = new Color(0f, 1.0f, 0f);
        Mesh m = floor.GetComponent<MeshFilter>().sharedMesh;
        var colors = from n in Enumerable.Range(0, m.vertices.Length) select c;
        m.colors = colors.ToArray();
    }

    // Update is called once per frame
    void Update () {

        // Get raw mouse input for a cleaner reading on more sensitive mice.
        var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        // Scale input against the sensitivity setting and multiply that against the smoothing value.
        mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

        // Interpolate mouse movement over time to apply smoothing delta.
        _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
        _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);
        walker_char.Look(_smoothMouse.x, _smoothMouse.y);

        if (Input.GetKey(KeyCode.I)) {
            floor.transform.eulerAngles = new Vector3(30, 0);
        }
        else if (Input.GetKey(KeyCode.K)) {
            floor.transform.eulerAngles = new Vector3(-30, 0);
        }
        else if (Input.GetKey(KeyCode.J)) {
            floor.transform.eulerAngles = new Vector3(0, 0, 30);
        }
        else if (Input.GetKey(KeyCode.L)) {
            floor.transform.eulerAngles = new Vector3(0, 0, -30);
        }
        else {
            floor.transform.eulerAngles = Vector3.zero;
        }

        if (Input.GetKeyDown(KeyCode.E)) {
            walker_char.state.accel.y += 100;
        }

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
    }

    private void FixedUpdate() {
        var turn = Input.GetAxis("Horizontal");
        walker_char.Move(Input.GetAxis("Vertical"), turn, Time.fixedDeltaTime, Input.GetButton("Jump"));
    }

    void attach_cam_to_walker(SceneCharacter3D walker) {
        cam.transform.position = walker.head.transform.position;
        cam.transform.rotation = walker.head.transform.rotation;
        var angles = cam.transform.eulerAngles;
        angles.z -= 90;
        cam.transform.eulerAngles = angles;

        var pos = cam.transform.position;
        pos += walker.state.forward_vector * .3f;
        cam.transform.position = pos;

        cam.transform.SetParent(walker.head.transform);
    }
}
