using UnityEngine;
using System.Collections;

public class WalkerTest : MonoBehaviour {

    public GameObject walker;
    private SceneCharacter3D walker_char;

    public Leg left_leg;
    public Leg right_leg;

    private const float bob_amount = .08f;
    private const float crouch_dist = .9f;
    public float crouch_factor = 0f;

    public Vector2 sensitivity = new Vector2(3, 3);
    public Vector2 smoothing = new Vector2(3, 3);

    private float head_rest;

    Vector2 _smoothMouse;

    int walking = 0;

    // Use this for initialization
    void Start () {
        walker_char = walker.GetComponent<SceneCharacter3D>();
        var legs = walker.GetComponents<Leg>();
        left_leg = legs[0];
        right_leg = legs[1];
        head_rest = walker_char.head.transform.localPosition.z;
    }
	
	// Update is called once per frame
	void Update () {
        var turn = Input.GetAxis("Horizontal") * 100 * Time.deltaTime;
        walker_char.Move(0, turn, 0.2f, Input.GetButtonDown("Jump"));

        // Get raw mouse input for a cleaner reading on more sensitive mice.
        var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        // Scale input against the sensitivity setting and multiply that against the smoothing value.
        mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

        // Interpolate mouse movement over time to apply smoothing delta.
        _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
        _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

        walker_char.Look(_smoothMouse.x, _smoothMouse.y);

        var crouch_dt = 5f * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftShift)) {
            crouch_factor = Mathf.Min(1.0f - bob_amount, crouch_factor + crouch_dt);
        }
        else {
            crouch_factor = Mathf.Max(0f, crouch_factor - crouch_dt);
        }
        
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
        }

        if (walking != 0) {
            var bob_factor = Mathf.Abs(left_leg.walk_seq_step) / 300f;
            if (bob_factor > 1) Debug.Log("AAAAAAAAA" + bob_factor);
            if (bob_factor < 0) Debug.Log("WTF");
            crouch_factor = Mathf.Min(1.0f, crouch_factor + (bob_amount * bob_factor));
        }
        
        left_leg.direction = vert;
        right_leg.direction = vert;

        left_leg.crouch_factor = crouch_factor;
        right_leg.crouch_factor = crouch_factor;

        var temp = walker_char.head.transform.localPosition;
        temp.z = head_rest - crouch_factor * crouch_dist;
        walker_char.head.transform.localPosition = temp;

    }
}
