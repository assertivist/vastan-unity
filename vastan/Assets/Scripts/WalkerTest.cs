using UnityEngine;
using System.Collections;
using System.Linq;

public class WalkerTest : MonoBehaviour {
    public GameObject walker;
    public GameObject floor;
    private SceneCharacter3D walker_char;

    Vector2 _smoothMouse;
    public Vector2 sensitivity = new Vector2(3, 3);
    public Vector2 smoothing = new Vector2(3, 3);
    // Use this for initialization
    void Start () {
        walker_char = walker.GetComponent<SceneCharacter3D>();
        // Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        recolor_walker(walker, new Color(.7f, 0f, .3f));
        
    }

    void recolor_walker(GameObject w, Color c) {
        recolor_object(w, "Walker.Head.Glass", new Color(56f / 255f, 69f / 255f, 188f / 255f));
        recolor_object(w, "Walker.Head.Tubes", new Color(75f / 255f, 71f / 255f, 71f / 255f));
        recolor_object(w, "Walker.Head.Main", c);
        recolor_object(w, "Walker.Leg.High.Left", c);
        recolor_object(w, "Walker.Leg.High.Right", c);
        recolor_object(w, "Walker.Leg.Low.Left", c);
        recolor_object(w, "Walker.Leg.Low.Right", c);
    }

    void recolor_object(GameObject parent, string name, Color c) {
        GameObject go = parent.transform.Find("walker_orig/" + name).gameObject;
        Mesh m = go.GetComponent<SkinnedMeshRenderer>().sharedMesh;
        var colors = from n in Enumerable.Range(0, m.vertices.Length) select c;
        m.colors = colors.ToArray();
    }

    // Update is called once per frame
    void Update () {
        var turn = Input.GetAxis("Horizontal") * 100 * Time.deltaTime;
        walker_char.Move(Input.GetAxis("Vertical"), turn, Time.deltaTime, Input.GetButton("Jump"));

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
            walker_char.state.velocity = new Vector3(0f, 10f, 0f);
        }
    }
}
