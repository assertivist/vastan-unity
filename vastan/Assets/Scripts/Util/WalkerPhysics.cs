using UnityEngine;
using System.Collections;

public class WalkerPhysics {
    public Vector3 forward_vector;
    public Transform transform;
    public bool on_ground = false;
    public bool will_jump = false;
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
    private float jump_base_power = 6f;

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

    Vector3 gravity = new Vector3(0, -9.81f, 0);

    public Vector3 velocity = Vector3.zero;

    DampenedSpring crouch_spring = new DampenedSpring(0);

    public static float base_mass = 140f;

    public WalkerPhysics(
        Transform transform,
        float angle) {
        this.transform = transform;
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
        //pos = transform.position;

        var ground_angle = Vector3.Angle(h.normal, Vector3.up);
        if (ground_angle < 45f) { // Controller.SlopeLimit
            //on_ground = true;
        }
        if (ground_angle < 90f) {
            //momentum.y /= 2;
            return;
        }

        //velocity = velocity - (h.normal * Vector3.Dot(velocity, h.normal));
        //momentum = momentum - (h.normal * Vector3.Dot(momentum, h.normal));
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

    public void update(CharacterController controller, float dt, InputTuple i) {
        if (i.turn != 0) {
            var turn = i.turn * 67f * dt;
            //Debug.Log("Turn: " + turn);
            transform.Rotate(0, turn, 0);
        }

        RaycastHit hit;
        var did_hit = Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 1.1f);
        if (velocity.y <= 0 && did_hit) {
            Debug.DrawRay(transform.position + Vector3.up, Vector3.down, Color.cyan);
            Debug.Log(hit.distance);
            if (hit.distance <= 1.1f) {
                on_ground = true;
                //crouch_impulse = self.velocity.y;
                velocity.y = 0;
                if(hit.point.y > transform.position.y)
                //transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
                controller.Move(new Vector3(0, hit.point.y - transform.position.y, 0));
            }
        }
        else {
            on_ground = false;
            var current_y = new Vector3(0, transform.position.y);
            var current_y_vel = new Vector3(0, velocity.y, 0);
            PositionAndVelocity newpv_y = new Integrator(gravity).integrate(current_y, current_y_vel, dt);
            velocity.y = newpv_y.Velocity.y;
            //transform.position = new Vector3(transform.position.x, newpv_y.Position.y, transform.position.z);
            controller.Move(new Vector3(0, newpv_y.Position.y - transform.position.y, 0));
        }

        float jumpdt = dt * Game.AVARA_FPS;
        if (i.jump) {
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
                velocity.y = spd;
                on_ground = false;
            }
            crouch = Mathf.Max(crouch / 2f, 0);
            will_jump = false;
        }

        float friction = 1f;
        if (!on_ground) {
            friction = .02f;
        }

        var xz_velocity = new Vector3(velocity.x, 0, velocity.z);
        PositionAndVelocity newpv = new Friction(
            transform.TransformDirection(Vector3.forward) * i.forward,
            friction).integrate(transform.position, xz_velocity, dt);
        velocity.x = newpv.Velocity.x;
        velocity.z = newpv.Velocity.z;
        var new_pos = transform.position;
        new_pos.x = newpv.Position.x;
        new_pos.z = newpv.Position.z;
        //transform.position = new_pos;
        controller.Move(new_pos - transform.position);

        elevation = stance - crouch;
        Debug.DrawRay(transform.position, velocity, Color.red);
    }
}