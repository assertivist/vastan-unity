using System;
using UnityEngine;



public class InputTuple
{
    public float forward;
    public float turn;
    public InputTuple(float forward, float turn) {
        this.forward = forward;
        this.turn = turn;
    }
}

public class WalkerPhysics : Integrator
{
    public Vector3 forward_vector;
    public Transform transform;
    public bool on_ground;

    public WalkerPhysics(
        float mass, 
        Transform transform, 
        Vector3 velocity, 
        Vector3 momentum, 
        float angle) :
        base(mass, transform.position, velocity, momentum, angle) {
        this.transform = transform;
    }

    public override Vector3 acceleration(float dt, InputTuple i) {
        forward_vector = transform.TransformDirection(Vector3.forward) * -1;

        var v = forward_vector * i.forward;

        // friction
        if (on_ground) {
            friction = 1f;
        }
        else {
            friction = .02f;
        }

        var d = new Vector3(accel.x, 0, accel.z) - v;
        if (d.magnitude > friction / 5f) {
            d = d.normalized;
            d *= friction * 50000f * dt;
        } 
        else {
            d *= friction * 50000f * dt;
        }

        if (!on_ground) {
            accel.y = -9800f * dt;
        }
        else {
            d.x -= momentum.x * friction * 2500f * dt;
            d.z -= momentum.z * friction * 2500f * dt;
            //accel.y = Mathf.Max(accel.y, 0);
            velocity.y = 0f;
            momentum.y = Math.Max(momentum.y, 0);
        }

        d.y = accel.y;
        return d;
    }

    public override float torque (float dt, InputTuple i) {
        spin = i.turn * 6000f * dt;
        return spin;
    }
}

public class Integrator
{
    class Derivative
    {
        public Vector3 velocity;
        public Vector3 force;
        public float torque;

        public Derivative(Vector3 velocity, Vector3 force, float torque) {
            this.velocity = velocity;
            this.force = force;
            this.torque = torque;
        }
    }

    

    public Integrator(float mass, Vector3 pos, Vector3 velocity, Vector3 momentum, float angle) {
        this.mass = mass;
        this.pos = pos;
        this.velocity = velocity;
        this.momentum = momentum;
        this.angle = angle;
    }
    
    private float t;
    public Vector3 pos;
    public Vector3 momentum;
    public Vector3 velocity;
    public float angle;
    public float mass;
    public Vector3 accel;
    public float spin;
    public float friction = .02f;
    public float maxSpeed = 100f;

    public void recalculate() {
        velocity = momentum * (1f / mass);
       

        var mag_sq = Mathf.Pow(velocity.magnitude, 2f);
        if (mag_sq > Mathf.Pow(maxSpeed, 2f) && mag_sq > 0) {
            var ratio = maxSpeed / velocity.magnitude;
            velocity *= ratio;
            momentum *= ratio;
        }
        if (angle > 359) angle -= 359f;
        if (angle < 0) angle = 359f - angle;
    }

    private Derivative evaluate1(float t, InputTuple i) {
        return new Derivative(velocity, acceleration(0, i), torque(0,i));
    }

    private Derivative evaluate2(float t, float dt, Derivative d, InputTuple i) {
        var p = pos + (d.velocity * dt);
        var m = momentum + (d.force * dt);
        var a = angle + d.torque * dt;
        var s = new Integrator(mass, p, Vector3.zero, m, a);
        s.recalculate();
        return new Derivative(s.velocity, acceleration(dt, i), torque(dt, i));
    }

    public void integrate(float t, float dt, InputTuple i) {
        var a = evaluate1(t, i);
        var b = evaluate2(t, dt * .05f, a, i);
        var c = evaluate2(t, dt * .05f, b, i);
        var d = evaluate2(t, dt, c, i);

        var factor = 2f;
        var divisor = (1f / 6f);

        var dposdt = ((a.velocity + ((b.velocity + c.velocity) * factor)) + d.velocity) * divisor;
        var dmomenumdt = ((a.force + ((b.force + c.force) * factor)) + d.force) * divisor;
        pos += dposdt;
        momentum += (dmomenumdt * dt);
        angle += divisor * dt * (a.torque + (factor * (b.torque + c.torque)) + d.torque);
        recalculate();
    }

    
    public virtual float torque(float dt, InputTuple i) { return 0; }
    public virtual Vector3 acceleration(float dt, InputTuple i) { return Vector3.zero; }

    public static Integrator interpolate(Integrator previous_state, Integrator new_state, float alpha) {
        var int_state = new_state;
        var new_pos = (new_state.pos * alpha) + (previous_state.pos * (1f - alpha));
        var new_velocity = (new_state.velocity * alpha) + (previous_state.velocity * (1f - alpha));
        var new_momentum = (new_state.momentum * alpha) + (previous_state.momentum * (1f - alpha));
        var new_angle = (new_state.angle * alpha) + (previous_state.angle * (1f - alpha));
        int_state.pos = new_pos;
        int_state.velocity = new_velocity;
        int_state.momentum = new_momentum;
        int_state.angle = new_angle;
        int_state.recalculate();
        return int_state;
    }
}

public class DampenedSpring {

    const float epsilon = 0.0001f;
    public float pos = 0;
    public float vel = 0;
    public float stable_pos = 0;
    float damping_ratio = .75f;
    float angular_freq = 18f;

    public DampenedSpring (float init_pos) {
        pos = init_pos;
        stable_pos = init_pos;
    }

    public void calculate(float dt) {
        float initial_pos = pos - stable_pos;
        float initial_vel = vel;
        /*
        if (damping_ratio > 1.0f + epsilon) {
            //overdamp

            float za = -angular_freq * damping_ratio;
            float zb = angular_freq * Mathf.Sqrt(damping_ratio * damping_ratio - 1.0f);
            float z1 = za - zb;
            float z2 = za + zb;
            float expterm1 = Mathf.Exp(z1 * dt);
            float expterm2 = Mathf.Exp(z2 * dt);

            float c1 = (initial_vel - initial_pos * z2) / (-2.0f * zb);
            float c2 = initial_pos - c1;

        }
        else*/
        if (damping_ratio > 1.0f - epsilon) {
            // critical damp
            
            float exp_term = Mathf.Exp(-angular_freq * dt);
            float c1 = initial_vel + angular_freq * initial_pos;
            float c2 = initial_pos;
            float c3 = (c1 * dt + c2) * exp_term;
            pos = stable_pos + c3;
            vel = (c1 * exp_term) - (c3 * angular_freq);
        }
        else {
            // underdamp

            float omega_zeta = angular_freq * damping_ratio;
            float alpha = angular_freq * Mathf.Sqrt(1.0f - Mathf.Pow(damping_ratio, 2));
            float exp_term = Mathf.Exp(-omega_zeta * dt);
            float cos_term = Mathf.Cos(alpha * dt);
            float sin_term = Mathf.Sin(alpha * dt);

            float c1 = initial_pos;
            float c2 = (initial_vel + omega_zeta * initial_pos) / alpha;

            pos = stable_pos + exp_term * (c1 * cos_term + c2 * sin_term);
            vel = -exp_term * ((c1 * omega_zeta - c2 * alpha) * cos_term +
                (c1 * alpha + c2 * omega_zeta) * sin_term);
        }
    }
}
