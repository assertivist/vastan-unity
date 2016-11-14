using UnityEngine;
using System.Collections;

public class WalkerTest : MonoBehaviour {

    public GameObject walker;
    private SceneCharacter3D walker_char;

    public Leg left_leg;
    public Leg right_leg;

    public Vector2 sensitivity = new Vector2(3, 3);
    public Vector2 smoothing = new Vector2(3, 3);

    Vector2 _smoothMouse;

    int walking = 0;

    // Use this for initialization
    void Start () {
        walker_char = walker.GetComponent<SceneCharacter3D>();
        var legs = walker.GetComponents<Leg>();
        left_leg = legs[0];
        right_leg = legs[1];
    }
	
	// Update is called once per frame
	void Update () {
        walker_char.Move(0, Input.GetAxis("Horizontal"), 0.2f, Input.GetButtonDown("Jump"));

        // Get raw mouse input for a cleaner reading on more sensitive mice.
        var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        // Scale input against the sensitivity setting and multiply that against the smoothing value.
        mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

        // Interpolate mouse movement over time to apply smoothing delta.
        _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
        _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

        walker_char.Look(_smoothMouse.x, _smoothMouse.y);

        var vert = Input.GetAxis("Vertical");

        if (vert > 0 && walking == 0) {
            walking = 1;
            right_leg.up_step = !left_leg.up_step;

            left_leg.walking = true;
            right_leg.walking = true;
        }
        if (vert < 0 && walking == 0) {
            walking = -1;
            left_leg.up_step = !right_leg.up_step;

            left_leg.walking = true;
            right_leg.walking = true;
        }

        if (vert == 0 && walking != 0) {
            walking = 0;
            left_leg.walking = false;
            right_leg.walking = false;

            //right_leg.up_step = false;
            //left_leg.up_step = false;
            //right_leg.walk_seq_step = 0;
            //left_leg.walk_seq_step = 0;
        }

        left_leg.direction = vert;
        right_leg.direction = vert;

        //left_leg.change_wf_size(left_leg.c + Input.GetAxis("Mouse ScrollWheel"));
        //right_leg.change_wf_size(right_leg.c + Input.GetAxis("Mouse ScrollWheel"));


    }
}
