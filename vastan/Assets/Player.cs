using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Player : NetworkBehaviour {

    public GameObject plasma_fab;

    private PlayerState ps;
    private Look look;
    private List<Leg> legs;

    public Transform cockpit;
    public Transform plasma_1;
    public Transform plasma_2;
    public Transform walker;
    
    // Use this for initialization
    void Start () {
	    ps = GetComponent<PlayerState>();
        look = cockpit.gameObject.GetComponent<Look>();
        legs = new List<Leg>(GetComponents<Leg>());
    }

    public override void OnStartServer() {
        base.OnStartServer();
        
    }

    public override void OnStartLocalPlayer() {
        // attach camera
        var cam = Camera.main;
        if (cam == null) {
            cam = new Camera();
        }
        var pos = transform.position;
        pos += transform.forward * -5;
        //pos += transform.up ;
        pos.y += 2.2f;
        cam.transform.position = pos;
        cam.transform.LookAt(cockpit, Vector3.up);
        pos -= transform.forward * -5.1f;
        //pos -= transform.up;

        cam.transform.position = pos;
        cam.transform.SetParent(cockpit);
        
    }

	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer)
            return;

        var rot = Input.GetAxis("Horizontal") * 60.0f * Time.deltaTime;
        var forward = Input.GetAxis("Vertical") * 6.5f * Time.deltaTime;

        transform.Rotate(new Vector3(0, rot, 0));
        transform.position += transform.forward * forward;

        if (forward != 0) {
            ps.walking = true;
        }
        else {
            ps.walking = false;
        }
        ps.head_rot = cockpit.localRotation;

        update_legs();

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) {
            // called from client, but invoked on server (??)
            if (ps.can_shoot())
                Cmd_fire_plasma();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl)) {
            var rb = GetComponent<Rigidbody>();
            
            rb.AddForce(Vector3.up * 1200.0f, ForceMode.Impulse);
        }
        

    }

    private void update_legs() {
        foreach (Leg l in legs) {
            l.walking = ps.walking;
        }
    }

    // Decoration for "network commands"
    // the "Cmd" prefix is required by law
    // this runs on the SERVER
    [Command]
    void Cmd_fire_plasma() {

        // spawn the object on the SERVER
        Transform cannon_bone;
        if (ps.firing == 1) {
            cannon_bone = plasma_1;
        }
        else {
            cannon_bone = plasma_2;
        }
        
        var plasma = (GameObject)Instantiate(
            plasma_fab,
            cannon_bone.position,
            cannon_bone.rotation);
        plasma.GetComponent<Rigidbody>().velocity = cannon_bone.forward * 25;

        var plasma_component = plasma.GetComponent<Plasma>();

        // ???
        plasma_component.energy = ps.shoot_and_return_energy();

        // pass object to be spawned on CLIENT(S)
        NetworkServer.Spawn(plasma);

        // remove after 5 seconds
        Destroy(plasma, 5.0f);
    }
}
