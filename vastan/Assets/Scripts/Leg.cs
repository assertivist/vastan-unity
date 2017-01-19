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

    private const float crouch_dist = .0083f;

    // min/max radius of ellipse
    private const float min_walkfunc_size_factor = .001f;
    private const float max_walkfunc_size_factor = 1.0f;

    // steps (points on the ellipse)
    private const int walkfunc_steps = 300;

    // length of our leg pieces (distance between joints)
    private float top_length;
    private float bottom_length;

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
    private GameObject foot_ref;
    private GameObject wf_target;

    // the maximum amount that these targets 
    // will get offset 
    private float max_lean = .4f;

    // current ratio, -1 is full backwards
    // and 1 is full forwards. we move between
    // these states smoothly
    private float offset_ratio = 0;

    // yo we walkin'?
    public bool walking = false;

    // how tall the leg is at rest
    // used for calculating crouch offset
    private float hip_rest;

    // initialization
    void Start () {
        // calculate our leg piece lengths
        top_length = (bottom.position - hip.position).magnitude;
        bottom_length = (foot.position - bottom.position).magnitude;

        hip_rest = hip.localPosition.z;
        
        // initial ellipse function domain
        recompute_wf_domain();

        // create our reference nodes
        foot_ref = new GameObject(GetInstanceID() + "_foot_ref");
        foot_ref.transform.SetParent(walker);
        foot_ref.transform.position = foot.transform.position;

        wf_target = new GameObject(GetInstanceID() + "_wf_target");
        wf_target.transform.SetParent(foot_ref.transform);
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
  
    void increment_walk_seq_step(int amt) {
        if (!((-walkfunc_steps < walk_seq_step) && 
            (walk_seq_step < walkfunc_steps))) {
            up_step = !up_step;
        }
        if (up_step) {
            walk_seq_step -= amt;
        }
        else {
            walk_seq_step += amt;
        }

        if (walk_seq_step < -walkfunc_steps) {
            walk_seq_step = -walkfunc_steps;
        }

        if (walk_seq_step > walkfunc_steps) {
            walk_seq_step = walkfunc_steps;
        }
    }

    public void change_wf_size(float new_size)
    {
        c = new_size;
        var max = max_walkfunc_size_factor - (.5f * crouch_factor);
        if (c > max) c = max;
        else if (c < min_walkfunc_size_factor) {
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
        // add lean offset
        pos.x += (offset_ratio * -max_lean * -(crouch_factor * max_lean));
        var transformed_pos = foot_ref.transform.TransformPoint(pos);
        if (!float.IsNaN(transformed_pos.x)) 
            wf_target.transform.position = transformed_pos; 
        return wf_target.transform.position;
    }
    
    public Vector3? get_floor_spot() {
        Vector3 from = foot.position;
        from.y += 1f;
        Ray r = new Ray(from, Vector3.down);
        var result = new RaycastHit();
        bool hit = Physics.Raycast(r, out result);
        if (hit) {
            return result.point;
        }
        else return null;
    }
    
    // places the legs according to the target spot
    // while not putting the feet through any object

    void place_leg() {

        var target_pos = get_target_pos();

        if (Debug.isDebugBuild)
            Debug.DrawLine(hip.position, target_pos, Color.magenta);

        var ray_result = get_floor_spot();
        Vector3 floor_pos;
        Vector3 hip_pos = hip.position;
        Vector3 target_vector;
        
        if (ray_result != null && target_pos.y < ((Vector3)ray_result).y) {
            floor_pos = (Vector3)ray_result;
            floor_pos.x = target_pos.x;
            floor_pos.z = target_pos.z;
            target_vector = hip_pos - floor_pos;
            on_ground = true;
            if (Debug.isDebugBuild)
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
            target_vector.Normalize();
            
            float delta = Vector3.Angle(target_vector, Vector2.up);

            var angles = top.localEulerAngles;
            var l_vec = foot_ref.transform.InverseTransformDirection(target_vector);
            angles.x = 180.0f - target_top_angle;
            
            if (l_vec.x < 0) {
                angles.x -= delta;
            }
            else angles.x += delta;
            angles.z = 0f;
            angles.y = 0f;

            top.localEulerAngles = angles;

            float tb_angle_cos = ((Mathf.Pow(top_length, 2) + 
                Mathf.Pow(bottom_length, 2) -
                Mathf.Pow(pt_length, 2)) / (2 * top_length * bottom_length));
            float target_bottom_angle = Mathf.Rad2Deg * Mathf.Acos(tb_angle_cos);

            angles = bottom.localEulerAngles;
            angles.x = (180.0f - target_bottom_angle);
            angles.z = 0;
            angles.y = 0;

            bottom.localEulerAngles = angles;
        }
        else return;
    }

	// called once per frame
	void Update () {
        // framerate independence
        var c_dt = Time.deltaTime * 5.0f;
        var offset_dt = Time.deltaTime * 5.0f;
        var walk_seq_dt = Mathf.Max(Mathf.FloorToInt(Time.deltaTime * 1000f), 1);

        if (walking) {
            if (direction > 0) {
                // walkin forwards
                increment_walk_seq_step(-walk_seq_dt);

                offset_ratio -= offset_dt;
                if (offset_ratio < -1) {
                    offset_ratio = -1;
                }
            }
            else {
                //walkin backwards
                increment_walk_seq_step(walk_seq_dt);

                offset_ratio += offset_dt;
                if (offset_ratio > 1) {
                    offset_ratio = 1;
                }
            }

            change_wf_size(c += c_dt);
        }
        else {
            // not walking anymore
            // move towards rest position
            change_wf_size(c -= c_dt);
            if (Mathf.Abs(offset_ratio) < offset_dt) {
                offset_ratio = 0;
            }
            if (offset_ratio > 0) {
                offset_ratio -= offset_dt;
            }
            if (offset_ratio < 0) {
                offset_ratio += offset_dt;
            }
            if (Mathf.Abs(walk_seq_step) < walk_seq_dt) {
                walk_seq_step = 0;
            }
            if (walk_seq_step > 0) {
                walk_seq_step -= walk_seq_dt;
            }
            if (walk_seq_step < 0) {
                walk_seq_step += walk_seq_dt;
            }
        }

        var temp = hip.transform.localPosition;
        temp.z = hip_rest - (crouch_factor * crouch_dist);
        hip.transform.localPosition = temp;

        recompute_wf_x();
        place_leg();

        if (Debug.isDebugBuild)
            Debug.DrawLine(
                foot_ref.transform.position, 
                wf_target.transform.position, 
                Color.cyan);

    }

    void LateUpdate()
    {
        
    }
}
