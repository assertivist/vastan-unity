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
    public Vector3 forward_vector = Vector3.zero;
    public bool on_ground;

    public WalkerPhysics(
        float mass, 
        Vector3 pos, 
        Vector3 velocity, 
        Vector3 momentum, 
        float angle) :
        base(mass, pos, velocity, momentum, angle) {

    }

    public override Vector3 acceleration(InputTuple i) {

        var v = get_forward() * i.forward;

        // friction
        if (on_ground) {
            friction = 1f;
        }
        else {
            friction = .02f;
        }

        var d = accel - v;
        if (d.magnitude > friction / 5f) {
            d = d.normalized;
            d *= friction * 70f;
        } 
        else {
            d *= friction * 70f;
        }

        if (!on_ground) {
            d.y -= 9.8f;
        }
        else {
            d.x -= momentum.x * friction * 5f;
            d.z -= momentum.z * friction * 5f;
            d.y = 0;
        }
        return d;
    }

    public override float torque (InputTuple i) {
        spin = i.turn * 40f;
        return spin;
    }

    private Vector3 get_forward() {
        return forward_vector * -1;
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
    public float maxSpeed = 1f;

    public void recalculate() {
        velocity = momentum * (1f / mass);
       

        var mag_sq = Math.Pow(velocity.magnitude, 2f);
        if (mag_sq > Math.Pow(maxSpeed, 2f) && mag_sq > 0) {
            var ratio = maxSpeed / velocity.magnitude;
            velocity *= ratio;
            momentum *= ratio;
        }
        if (angle > 359) angle -= 359f;
        if (angle < 0) angle = 359f - angle;
    }

    private Derivative evaluate1(float t, InputTuple i) {
        return new Derivative(velocity, acceleration(i), torque(i));
    }

    private Derivative evaluate2(float t, float dt, Derivative d, InputTuple i) {
        var p = pos + (d.velocity * dt);
        var m = momentum + (d.force * dt);
        var a = angle + d.torque * dt;
        var s = new Integrator(mass, p, Vector3.zero, m, a);
        s.recalculate();
        return new Derivative(s.velocity, acceleration(i), torque(i));
    }

    public void integrate(float current_t, float dt, InputTuple i) {
        t = current_t;

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

    
    public virtual float torque(InputTuple i) { return 0; }
    public virtual Vector3 acceleration(InputTuple i) { return Vector3.zero; }

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
