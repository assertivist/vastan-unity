using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

public class PlayerState : NetworkBehaviour {

    public const int max_health = 100;
    // this decoration syncs the
    // property over the network 
    [SyncVar]
    public int health = max_health;

    public const int max_energy = 100;
    [SyncVar]
    public int energy = max_energy;

    public const int max_plasma = 100;
    [SyncVar]
    public int plasma_1 = max_plasma;
    [SyncVar]
    public int plasma_2 = max_plasma;

    [SyncVar]
    public int firing = 0;

    [SyncVar]
    public Color color;

    [SyncVar]
    public bool walking;

    [SyncVar]
    public Quaternion head_rot;

    private PlayerUI sliders;

    private float respawn = 0;
    private float respawn_max = 10.0f;

    void Start() {
        sliders = GameObject.Find("Canvas").GetComponent<PlayerUI>();
        if (isLocalPlayer) {
            Cmdrandom_color();
        }
        head_rot = Quaternion.identity;
    }


    public bool can_shoot() {
        return plasma_1 > 39 || plasma_2 > 39;
    }

    public void take_damage(int amount) {
        if (!isServer) {
            return;
        }

        health -= amount;

        if (health <= 0) {
            Debug.Log("You blew up gg");
            RpcRespawn();
        }
    }

    public int shoot_and_return_energy() {
        int energy_out = 0;
        if (plasma_1 >= plasma_2) {
            energy_out = plasma_1;
            if (isServer) {
                plasma_1 = 0;
                firing = 1;
            }
        }
        else {
            energy_out = plasma_2;
            if (isServer) {
                plasma_2 = 0;
                firing = 2;
            }
        }
        return energy_out;
    }
    
    [ClientRpc]
    void RpcRespawn() {
        if (isLocalPlayer) {
            transform.position = Vector3.zero;
            /*nm.GetStartPosition();
            transform.position = new_start.position;
            transform.rotation = new_start.rotation;*/
        }
    }

    [Command]
    void Cmdrandom_color() {
        color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
    }


    void Update() {
        if (isServer) {
            if (health < max_health) {
                health += (int)Math.Ceiling(10 * Time.deltaTime);
                energy -= 6;
            }

            if (plasma_1 < max_plasma) {
                plasma_1 += (int)Math.Ceiling(10 * Time.deltaTime);
                energy -= 4;
            }

            if (plasma_1 < max_plasma) {
                plasma_2 += (int)Math.Ceiling(10 * Time.deltaTime);
                energy -= 4;
            }

            if (energy < 1) {
                energy = 1;
            }
            energy += 2;
            
        }
        float head_x = head_rot.eulerAngles.x;
        sliders.update_sliders(health, 
            energy, plasma_1, plasma_2, head_x);
    }
}
