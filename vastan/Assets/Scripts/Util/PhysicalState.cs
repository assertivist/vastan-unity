using System;
using UnityEngine;



public struct InputTuple {
    public float forward;
    public float turn;
    public bool jump;
    public InputTuple(float forward, float turn, bool jump) {
        this.forward = forward;
        this.turn = turn;
        this.jump = jump;
    }
}

public struct PositionAndVelocity {
    private readonly Vector3 _position;
    private readonly Vector3 _velocity;
    public PositionAndVelocity(Vector3 pos, Vector3 vel) {
        _position = pos;
        _velocity = vel;
    }

    public Vector3 Position { get { return _position; } }
    public Vector3 Velocity { get { return _velocity; } }
}

public class Integrator {
    protected Vector3 _accel;
    private float _factor = 2.0f;
    private float _divisor = 1.0f / 6.0f;

    public Integrator(Vector3 acceleration) {
        _accel = acceleration;
    }

    public virtual Vector3 Acceleration(Vector3 x, Vector3 v, float dt){
        return _accel;
    }

    public PositionAndVelocity evaluate(Vector3 x, Vector3 v, float dt, Vector3 dx, Vector3 dv) {
        x = x + dx * dt;
        v = v + dv * dt;

        dx = v;
        dv = Acceleration(x, v, dt);

        return new PositionAndVelocity(dx, dv);
    }

    public PositionAndVelocity integrate(Vector3 x, Vector3 v, float dt) {
        var dpva = evaluate(x, v, 0, Vector3.zero, Vector3.zero);
        var dpvb = evaluate(x, v, dt * 0.5f, dpva.Position, dpva.Velocity);
        var dpvc = evaluate(x, v, dt * 0.5f, dpvb.Position, dpvb.Velocity);
        var dpvd = evaluate(x, v, dt, dpvc.Position, dpvc.Velocity);
        
        var dxdt = (dpva.Position + (dpvb.Position + dpvc.Position) * _factor + dpvd.Position) * _divisor;
        var dvdt = (dpva.Velocity + (dpvb.Velocity + dpvc.Velocity) * _factor + dpvd.Velocity) * _divisor;
        return new PositionAndVelocity(x + dxdt * dt, v + dvdt * dt);
    }
}

public class Friction : Integrator {
    private float _friction;

    public Friction(Vector3 accel, float friction) : base(accel) {
        _friction = friction;
    }

    public override Vector3 Acceleration(Vector3 x, Vector3 v, float dt) {
        var direction = _accel - v;
        if (direction.magnitude > _friction / 5.0f) {
            direction = direction.normalized;
            return direction * _friction * 70f;
        }
        else
            return direction * _friction * 70f;
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
