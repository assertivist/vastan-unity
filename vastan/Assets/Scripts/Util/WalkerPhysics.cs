using UnityEngine;
using System.Collections;

public class WalkerPhysics : Integrator {
    public Vector3 forward_vector;
    public Transform transform;
    public bool on_ground = false;
    public bool airborne = true;
    public bool will_jump;
    public float friction = .02f;
    private static float max_head_height = 1.75f;
    private static float min_head_height = .75f;

    private const float bob_amount = .12f;
    public const float crouch_dist = .0083f;
    public float crouch = 0f;

    public float stance = max_head_height;
    private float max_stance = max_head_height;
    private float min_stance = min_head_height;
    public float elevation = max_head_height;
    private float jump_base_power = 50f;

    float max_energy = 5f;
    public float energy = 5f;
    float energy_charge = .030f;

    float shield_charge = .030f;
    float max_shield = 3f;
    public float shield = 3f;

    float max_plasma_power = .8f;
    public float min_plasma_power = .25f;
    float plasma_charge = .035f;

    public float plasma1 = .8f;
    public float plasma2 = .8f;

    byte max_grenades = 6;
    public int grenades = 6;

    int max_missiles = 4;
    public int missiles = 4;

    int max_boosters = 3;
    int boosters = 3;
    bool boosting = false;
    float boost_timer = 0;
    float boost_time = 0;

    float gravity = -9800f;

    DampenedSpring crouch_spring = new DampenedSpring(0);

    public static float base_mass = 140f;

    public WalkerPhysics(
        float mass,
        Transform transform,
        Vector3 velocity,
        Vector3 momentum,
        float angle) :
        base(mass, transform.position, velocity, momentum, angle) {
        this.transform = transform;
        this.pos = transform.position;
        crouch_spring.stable_pos = 0;
    }

    public bool can_fire_plasma() {
        return (plasma1 > min_plasma_power || plasma2 > min_plasma_power);
    }

    public bool can_fire_grenade() {
        return grenades > 0;
    }

    public bool can_fire_missile() {
        return missiles > 0;
    }

    float get_total_mass() {
        return base_mass + grenades + missiles + (boosters * 4);
    }

    private float plasma_update(float dt, float plasma) {
        float guncharge = ((energy + energy_charge) * plasma_charge) / max_energy;
        float new_energy = plasma;
        if (plasma < max_plasma_power) {
            energy -= guncharge * dt;
            if (plasma > min_plasma_power) {
                new_energy = plasma + (guncharge * .850f * dt);
            }
            else {
                new_energy = plasma + (guncharge * 1.050f * dt);
            }
            if (new_energy > max_plasma_power)
                new_energy = max_plasma_power;
        }
        return new_energy;
    }

    public void react_to_contact(ControllerColliderHit h, Vector3 move) {
        var pos_adjust = h.normal * move.magnitude;
        Debug.DrawLine(h.point, h.point + pos_adjust);
        pos = transform.position;

        var ground_angle = Vector3.Angle(h.normal, Vector3.up);
        if (ground_angle < 45f) { // Controller.SlopeLimit
            on_ground = true;
        }
        if (ground_angle < 90f) {
            //momentum.y /= 2;
            return;
        }

        velocity = velocity - (h.normal * Vector3.Dot(velocity, h.normal));
        momentum = momentum - (h.normal * Vector3.Dot(momentum, h.normal));
    }

    public void energy_update(float dt) {
        plasma1 = plasma_update(dt, plasma1);
        plasma2 = plasma_update(dt, plasma2);

        if (shield < max_shield) {
            float regen = (shield_charge * energy) / max_energy;

            if (boosting)
                shield += shield_charge * dt;

            shield += (regen / 8f) * dt;

            shield = Mathf.Min(shield, max_shield);
            energy -= regen * dt;
        }

        energy += energy_charge * dt;
        if (boosting)
            energy += energy_charge * 4 * dt;

        energy = Mathf.Min(max_energy, energy);
        energy = Mathf.Max(energy, 0f);
    }

    public override Vector3 acceleration(float dt, InputTuple i) {
        mass = get_total_mass();
        forward_vector = transform.TransformDirection(Vector3.forward) * -1;
        var jump = i.jump;
        var v = forward_vector * i.forward;
        elevation = stance - crouch;


        float jumpdt = dt * Game.AVARA_FPS;
        //var resting = crouch_spring.stable_pos == crouch_spring.pos;
        //crouch_spring.stable_pos = 0f + base_crouch_factor + (bob_factor * bob_amount);
        //if (resting) crouch_spring.pos = crouch_spring.stable_pos;
        //crouch_spring.calculate(duration);
        //crouch_factor = Mathf.Round(Mathf.Clamp(crouch_spring.pos, -1.2f, 1.2f) * 100) / 100f;
        //crouch_factor = crouch_spring.pos;

        if (jump) {
            //crouch_factor = Mathf.Min(1.0f - bob_amount, crouch_factor + crouch_dt);
            //crouch_spring.vel += 400f * duration;
            if (!will_jump) {
                crouch += (stance - crouch - min_stance) / 8f * jumpdt;
            }
            else {
                crouch += (stance - crouch - min_stance) / 4f * jumpdt;
            }
            will_jump = true;
        }
        else {
            if (will_jump && on_ground) {
                var spd = (((crouch / 2f) + jump_base_power) * base_mass) / get_total_mass();
                momentum.y = spd;
                airborne = true;
                on_ground = false;
            }
            crouch = Mathf.Max((crouch / 1.01f) * jumpdt, 0);
            will_jump = false;
        }

        if (!on_ground && airborne) {
            airborne = false;
        }

        if (on_ground) {
            friction = 1f;
            momentum.y = 0;
        }
        else {
            friction = .02f;
        }

        var d = new Vector3(accel.x, 0, accel.z) - v;
        if (d.magnitude > friction / 5f) {
            d = d.normalized;
            d *= friction * 40000 * dt;
        }
        else {
            d *= friction * 40000 * dt;
        }


        d.x -= momentum.x * friction * 2500f * dt;
        d.z -= momentum.z * friction * 2500f * dt;

        accel.y = gravity * dt;
        d.y = accel.y;
        return d;
    }

    public override float torque(float dt, InputTuple i) {
        spin = i.turn * 24000f * dt;
        return spin;
    }
}