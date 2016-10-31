using UnityEngine;
using System.Collections;

public class Leg : MonoBehaviour {
    public Transform hip;
    public Transform top;
    public Transform bottom;
    public Transform foot;

    private Vector3 hip_rest;

    private float top_target;
    private float bottom_target;


    public bool on_ground = false;
    public float crouch_factor = 0;

    private const float min_walkfunc_size_factor = .001f;
    private const float max_walkfunc_size_factor = 1.0f;
    public const int walkfunc_steps = 25;

    private float top_length = 1;
    private float bottom_length = 1.21f;

    public int walk_seq_step = 0;
    public bool up_step = false;

    // walkfunc ellipse italicized amount
    private static float A = -.27f;

    //current x
    private float wf_x = 0;

    // size parameter
    public float c = 1.0f; // min_walkfunc_size_factor;

    private float wf_x_max;

    private float direction = 0;

    private Vector3 foot_rest;
    private Vector3 foot_back;

    public bool walking = false;
    private Vector3 leg_target;


    // Use this for initialization
    void Start () {
        top_target = top.localEulerAngles.x;
        Debug.Log(top.localEulerAngles);
        

        bottom_target = bottom.localEulerAngles.x;
        Debug.Log(bottom.localEulerAngles);
        

        hip_rest = hip.position;
        Debug.Log(hip_rest);

        top_length = (bottom.position - hip.position).magnitude;
        bottom_length = (foot.position - bottom.position).magnitude;

        Debug.Log(top_length);
        Debug.Log(bottom_length);

        // do all this stuff in order to get vectors that point to
        // foot rest areas in 'back' and 'front' for forward
        // and backward walking respectively
        var temp = foot.localPosition;
        temp.x += .5f;
        foot.localPosition = temp;
        foot_rest = foot.position - hip.position;
        temp.x -= .5f;
        foot.localPosition = temp;
        
        //test_target.SetParent(foot);
        //test_target.Rotate(0, 0, 90);
        
        recompute_wf_domain();

    }

    float ellipse(float x, bool top)
    { 
        var first_term = (3 * x) * Mathf.Cos(A) * Mathf.Sin(A);
        var under_sqrt_term1 = Mathf.Pow(c, 2) * ((4 * Mathf.Pow(Mathf.Cos(A), 2)) + Mathf.Pow(Mathf.Sin(A), 2));
        var under_sqrt_term2 = 4 * Mathf.Pow((x), 2) * Mathf.Pow((Mathf.Pow(Mathf.Cos(A), 2) + Mathf.Pow(Mathf.Sin(A), 2)), 2);
        var under_sqrt = under_sqrt_term1 - under_sqrt_term2;
        var bottom_term = ((4 * Mathf.Pow(Mathf.Cos(A), 2)) + Mathf.Pow(Mathf.Sin(A), 2));

        if (top) {
            return (first_term - Mathf.Sqrt(under_sqrt)) / bottom_term;
        }
        else {
            return (first_term + Mathf.Sqrt(under_sqrt)) / bottom_term;
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

    public void change_wf_size(float new_size)
    {
        c = new_size;
        if (c > max_walkfunc_size_factor)
        {
            c = max_walkfunc_size_factor;
        }
        else if (c < min_walkfunc_size_factor)
        {
            c = min_walkfunc_size_factor;
        }
        recompute_wf_domain();
    }

    void recompute_wf_x()
    {
        wf_x = ((float)Mathf.Abs(walk_seq_step) * (wf_x_max)) / (float) walkfunc_steps;
        if (walk_seq_step < 0) wf_x *= -1;
    }

    void recompute_wf_domain()
    {
        wf_x_max = Mathf.Sqrt(Mathf.Pow(c, 2.0f) * ((3.0f * Mathf.Cos(2.0f * A)) + 5.0f)) / (2.0f * Mathf.Sqrt(2.0f));
    }

    Vector3 get_target_pos() {
        float wf_y = ellipse(wf_x, up_step);
        Vector3 pos = new Vector3(wf_x, wf_y);
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
        Debug.DrawRay(from, Vector3.down);
        var result = new RaycastHit();
        bool hit = Physics.Raycast(r, out result);

        if (hit) {
            return result.point;
        }
        else return new Vector3(0,100000);
        
    }

    // the name is a little spare but this
    // places the legs according to the target spot
    // while not putting the feet through any object

    void ik_leg() {
        Vector3 floor_pos = get_floor_spot();
        Vector3 target_pos = leg_target;
        Vector3 hip_pos = hip.position;
        
        Vector3 target_vector;

        if (floor_pos.y < 10 && target_pos.y < floor_pos.y) {
            floor_pos.x = target_pos.x;
            target_vector = hip_pos - floor_pos;
            on_ground = true;

            Debug.DrawLine(hip_pos, floor_pos, Color.yellow);
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

            Vector2 target2 = new Vector2(target_vector.x, target_vector.y);
            float delta = Vector2.Angle(target2, Vector2.up);

            var angles = top.localEulerAngles;
            angles.y = 180.0f - target_top_angle;
            if (target2.x < 0) {
                angles.y -= delta;
            }
            else {
                angles.y += delta;
            }
            top.localEulerAngles = angles;

            float tb_angle_cos = ((Mathf.Pow(top_length, 2) + 
                Mathf.Pow(bottom_length, 2) -
                Mathf.Pow(pt_length, 2)) / (2 * top_length * bottom_length));
            float target_bottom_angle = Mathf.Rad2Deg * Mathf.Acos(tb_angle_cos);

            angles = bottom.localEulerAngles;
            angles.y = (180.0f - target_bottom_angle);
            bottom.localEulerAngles = angles;
        }
        else return;
    }

	// Update is called once per frame
	void Update () {
        increment_walk_seq_step(-1);
        recompute_wf_x();
        leg_target = hip.position + foot_rest + get_target_pos();
        
        ik_leg();

        Debug.DrawLine(hip.position, leg_target, Color.magenta);
        Debug.DrawLine(hip.position, bottom.position, Color.green);
        Debug.DrawLine(bottom.position, foot.position, Color.blue);

    }

    void LateUpdate()
    {
        
    }
}
