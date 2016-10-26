using UnityEngine;
using System.Collections;

public class Leg : MonoBehaviour {
    public Transform hip;
    public Transform top;
    public Transform bottom;
    public Transform foot;

    public Transform test_target;

    private Vector3 hip_rest;

    private float top_target;
    private Quaternion bottom_target;


    public bool on_ground = false;
    public float crouch_factor = 0;

    private const float min_walkfunc_size_factor = .001f;
    private const float max_walkfunc_size_factor = 22.0f;
    private const int walkfunc_steps = 14;

    private const float top_length = 1;
    private const float bottom_length = 1.21f;

    private int walk_seq_step = 0;
    private bool up_step = false;

    private static int wf_ellipse_mag_coefficient = 37;

    private float wf_x = 0;
    public float wf_sizep = 22.0f; // min_walkfunc_size_factor;
    private float wf_x_max;

    private float direction = 0;

    private Vector3 foot_ref;

    public bool walking = false;

    // Use this for initialization
    void Start () {
        top_target = top.eulerAngles.x;
        Debug.Log(top_target);

        //bottom_target = bottom.eulerAngles.x;
        Debug.Log(bottom_target);

        bottom_target = bottom.localRotation;

        hip_rest = hip.position;
        Debug.Log(hip_rest);

        foot_ref = foot.position;

        wf_x_max = Mathf.Sqrt(wf_sizep / wf_ellipse_mag_coefficient);
	}

    // i just made this and it is right??
    // returns a Y for any X
    // and a top/bottom arg
    /*
    float ellipse(float x, bool top)
    {
        var A = wf_ellipse_italic_mag;
        var c = wf_sizep;

        var first_term = (3 * x) * Mathf.Cos(A) * Mathf.Sin(A);
        var under_sqrt_term1 = Mathf.Pow(c, 2) * (4 * Mathf.Pow(Mathf.Cos(A), 2) + Mathf.Pow(Mathf.Sin(A), 2));
        var under_sqrt_term2 = Mathf.Pow((4 * x), 2) * Mathf.Pow((Mathf.Pow(Mathf.Cos(A), 2) + Mathf.Pow(Mathf.Sin(A), 2)), 2);
        var under_sqrt = under_sqrt_term1 - under_sqrt_term2;
        var bottom_term = ((4 * Mathf.Pow(Mathf.Cos(A), 2)) + Mathf.Pow(Mathf.Sin(A), 2));

        if (top) {
            return (first_term - Mathf.Sqrt(under_sqrt)) / bottom_term;
        }
        else {
            return (first_term + Mathf.Sqrt(under_sqrt)) / bottom_term;
        }
    }
    */

    float ellipse(float x, bool top)
    {
        var term1 = (-wf_x);
        var term2 = Mathf.Sqrt(75.0f * ((-wf_ellipse_mag_coefficient * Mathf.Pow(wf_x, 2)) + wf_sizep)) / 100.0f;
        if (top) {
            return term1 + term2;
        }
        else {
            return term1 - term2;
        }
    }

    void increment_walk_seq_step(int dir) {
        direction = dir;
        if (!((-walkfunc_steps < walk_seq_step) && 
            (walk_seq_step  < walkfunc_steps))) {
            up_step = !up_step;
        }
        if (up_step) {
            walk_seq_step -= 1 * Mathf.FloorToInt(direction);
        }
        else {
            walk_seq_step += 1 * Mathf.FloorToInt(direction);
        }
    }

    void recompute_wf_x()
    {
        wf_x_max = Mathf.Sqrt(wf_sizep / wf_ellipse_mag_coefficient);
        Debug.Log("new max: " + wf_x_max);
        wf_x = ((float)Mathf.Abs(walk_seq_step) * wf_x_max) / (float) walkfunc_steps;
        wf_x = (wf_x * wf_sizep) / max_walkfunc_size_factor;
        if (walk_seq_step < 0) wf_x *= -1;
        Debug.Log("new wf_x: " + wf_x);
    }

    Vector3 get_target_pos() {
        float wf_y = ellipse(wf_x, up_step);
        Vector3 pos = new Vector3();
        pos.x = wf_x;
        pos.y = wf_y;
        Debug.Log("pos.x: " + pos.x);
        Debug.Log("pos.y: " + pos.y);
        return pos;
    }
    
    float bottom_resting_pos() {
        return bottom.position.y + crouch_factor * -50;
    }

    float top_resting_pos() {
        return top.position.y + crouch_factor * -50;
    }

    Vector3 get_floor_spot() {
        Vector3 from = foot.position;
        from.y += 1;
        Ray r = new Ray(from, Vector3.down);

        var result = new RaycastHit();
        bool hit = Physics.Raycast(r, out result);

        if (hit) {
            return result.point;
        }
        else return new Vector3(0,100000);
        
    }

    void ik_leg() {
        Vector3 floor_pos = get_floor_spot();
        Vector3 target_pos = get_target_pos();
        Vector3 hip_pos = hip.position;

        Vector3 target_vector;

        if (floor_pos.y < 10 && target_pos.y < floor_pos.y){
            floor_pos.x = target_pos.x;
            target_vector = hip_pos - floor_pos;
            on_ground = true;
        }
        else {
            target_vector = hip_pos - target_pos;
            on_ground = false;
        }

        float pt_length = target_vector.magnitude;
        if (.01 < pt_length && pt_length < (top_length + bottom_length)) {
            float tt_angle_cos = (Mathf.Pow(top_length, 2) + 
                Mathf.Pow(pt_length, 2) -
                Mathf.Pow(bottom_length, 2)) / (2 * top_length * pt_length);
            float target_top_angle;
            try {
                target_top_angle = Mathf.Rad2Deg * Mathf.Acos(tt_angle_cos);
            }
            catch {
                Debug.Log("couldn't acos angle " + tt_angle_cos);
                return;
            }
            Vector3 t_normal = target_vector.normalized;
            float delta = Vector3.Angle(target_vector, Vector3.up);
            top.Rotate(0, 90 - target_top_angle + delta, 0);

            float tb_angle_cos = ((Mathf.Pow(top_length, 2) + 
                Mathf.Pow(bottom_length, 2) -
                Mathf.Pow(pt_length, 2)) / (2 * top_length * bottom_length));
            float target_bottom_angle = Mathf.Rad2Deg * Mathf.Acos(tb_angle_cos);
            bottom.Rotate(0, (180 - target_bottom_angle) * -1, 0);
        }
        else return;
    }
	// Update is called once per frame
	void Update () {
        increment_walk_seq_step(1);
        recompute_wf_x();
        var thePos = get_target_pos();
        
        test_target.transform.position = thePos;
        Debug.Log("walk_seq_step: " + walk_seq_step);

        /*
        if (walking) {

            bottom.Rotate(Vector3.up, -10);
        }
        else {
            bottom.localRotation = bottom_target;
        }
        */
	}
}
