using UnityEngine;
using System.Collections;

public class Leg : MonoBehaviour {
    public Transform walker;
    public Transform hip;
    public Transform top;
    public Transform bottom;
    public Transform foot;

    public bool on_ground = false;
    public float crouch_factor = 0;

    // min/max radius of ellipse
    private const float min_walkfunc_size_factor = .001f;
    private const float max_walkfunc_size_factor = 1.0f;

    // steps (points on the ellipse)
    public const int walkfunc_steps = 25;

    private float top_length = 1;
    private float bottom_length = 1.21f;

    // current point on ellipse
    public int walk_seq_step = 0;

    // up or down step? (are we lifting
    // this foot or using it to move fw)
    public bool up_step = false;

    // walkfunc ellipse italicized amount
    private static float A = -.27f;

    //current x
    private float wf_x = 0;

    // size parameter
    public float c = min_walkfunc_size_factor;

    // domain of walkfunc at current size
    private float wf_x_max;

    // forwards or backwards?
    public float direction = 0;

    // these offsets are for calculating
    // the foot targets--when walkers go
    // forward they kinda lean forward and
    // the opposite when moving backward
    private GameObject rest_target;
    private GameObject front_target;
    private GameObject back_target;
    private GameObject offset_target;
    // the maximum amount that these targets 
    // will get offset
    private float max_lean = .8f;
    // ultimate goal offset that gets
    // lerped for smoothness
    private Vector3 offset;
    private float offset_ratio = 0;

    // yo we walkin'?
    public bool walking = false;

    // the ultimate final target for the foot
    private Vector3 target_pos;

    // Use this for initialization
    void Start () {
        top_length = (bottom.position - hip.position).magnitude;
        bottom_length = (foot.position - bottom.position).magnitude;

        //offset = foot.position - hip.position;

        rest_target = new GameObject();
        rest_target.transform.position = foot.position;
        rest_target.transform.SetParent(walker);

        front_target = new GameObject();
        front_target.transform.position = foot.position + (foot.right * max_lean);
        front_target.transform.SetParent(walker);

        back_target = new GameObject();
        back_target.transform.position = foot.position + (foot.right * max_lean * -1);
        back_target.transform.SetParent(walker);

        offset_target = new GameObject();
        offset_target.transform.SetParent(walker);
        offset_target.transform.rotation = walker.rotation;
        offset_target.transform.position = foot.position;

        Debug.Log(top_length);
        Debug.Log(bottom_length);
        recompute_wf_domain();

    }

    float ellipse(float x, bool top) { 
        var first_term = (3 * x) * Mathf.Cos(A) * Mathf.Sin(A);
        var under_sqrt_term1 = Mathf.Pow(c, 2) * 
            ((4 * Mathf.Pow(Mathf.Cos(A), 2)) + Mathf.Pow(Mathf.Sin(A), 2));
        var under_sqrt_term2 = 4 * Mathf.Pow((x), 2) 
            * Mathf.Pow((Mathf.Pow(Mathf.Cos(A), 2) 
            + Mathf.Pow(Mathf.Sin(A), 2)), 2);
        var under_sqrt = under_sqrt_term1 - under_sqrt_term2;
        var bottom_term = ((4 * Mathf.Pow(Mathf.Cos(A), 2)) 
            + Mathf.Pow(Mathf.Sin(A), 2));

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
        wf_x = ((float)Mathf.Abs(walk_seq_step) 
            * (wf_x_max)) / (float) walkfunc_steps;
        if (walk_seq_step < 0) wf_x *= -1;
    }

    void recompute_wf_domain()
    {
        wf_x_max = Mathf.Sqrt(Mathf.Pow(c, 2.0f) 
            * ((3.0f * Mathf.Cos(2.0f * A)) + 5.0f)) 
            / (2.0f * Mathf.Sqrt(2.0f));
    }

    Vector3 get_target_pos() {
        float wf_y = ellipse(wf_x, up_step);
        Vector3 pos = new Vector3(wf_x, wf_y, 0f);
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
        
        var ellipse_target = get_target_pos();

        var pos = offset_target.transform.localPosition;
        pos.x += ellipse_target.x;
        pos.y += ellipse_target.y;
        //offset_target.transform.localPosition = pos;

        target_pos = offset_target.transform.position;

        Debug.DrawLine(rest_target.transform.position, target_pos);

        Debug.DrawLine(offset_target.transform.position, offset_target.transform.up, Color.green);
        Debug.DrawLine(offset_target.transform.position, offset_target.transform.forward, Color.blue);

        Vector3 floor_pos = get_floor_spot();
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
        if (walking)
        {
            var offset_dt = Time.deltaTime * 1.5f;
            if (direction > 0)
            {
                // walkin forwards
                increment_walk_seq_step(-1);
                
                offset_ratio -= offset_dt;
                if (offset_ratio < -1) {
                    offset_ratio = -1;
                }
            }
            else
            {
                //walkin backwards
                increment_walk_seq_step(1);

                offset_ratio += offset_dt;
                if (offset_ratio > 1) {
                    offset_ratio = 1;
                }
            }

            change_wf_size(c *= (1.250f));
        }
        else
        {
            change_wf_size(c *= (0.750f));
            if (offset_ratio > 0 ) {
                offset_ratio -= .1f;
            }
            if (offset_ratio < 0 ) {
                offset_ratio += .1f;
            }
        }

        //offset = hip.transform.TransformPoint(new Vector3(.35f,-1.635f,0));
        if (offset_ratio > 0) {
            offset = Vector3.Lerp(
                rest_target.transform.position, 
                back_target.transform.position, 
                offset_ratio);
        }
        if (offset_ratio < 0) {
            offset = Vector3.Lerp(
                rest_target.transform.position, 
                front_target.transform.position, 
                Mathf.Abs(offset_ratio));
        }

        offset_target.transform.position = offset;
        //offset_target.transform.rotation = hip.rotation;

        recompute_wf_x();
        ik_leg();

        Debug.DrawLine(hip.position, target_pos, Color.magenta);

        Debug.DrawLine(rest_target.transform.position, front_target.transform.position, Color.white);
        Debug.DrawLine(rest_target.transform.position, back_target.transform.position, Color.gray);


    }

    void LateUpdate()
    {
        
    }
}
