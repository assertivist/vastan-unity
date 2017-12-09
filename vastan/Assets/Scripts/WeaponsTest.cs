using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class WeaponsTest : MonoBehaviour {
    public GameObject walker;
    public GameObject floor;
    public GameObject side_spot;
    public GameObject ai;
    public Camera cam;
    private bool cam_is_static = true;
    private SceneCharacter3D walker_char;

    public GameObject TriangleExplosionPrefab;

    public List<Projectile> Projectiles { get; set; }

    public AudioClip grenade_explode;
    public AudioClip plasma_hit;

    public GameObject plasma_prefab;
    public GameObject grenade_prefab;

	public GameObject energy_text;
	public GameObject shield_text;
	public GameObject plasma1_text;
	public GameObject plasma2_text;
	public GameObject missiles_text;
	public GameObject grenades_text;

    Vector2 _smoothMouse;
    public Vector2 sensitivity = new Vector2(3, 3);
    public Vector2 smoothing = new Vector2(3, 3);
    
    // Use this for initialization
    void Start() {
        walker_char = walker.GetComponent<SceneCharacter3D>();
        // Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        walker_char.recolor_walker(new Color(.7f, 0f, .3f));

        //Color c = new Color(.2f, .2f, .2f);
        //Mesh m = floor.GetComponent<MeshFilter>().sharedMesh;
        //var colors = from n in Enumerable.Range(0, m.vertices.Length) select c;
        //m.colors = colors.ToArray();

        var ai3d = ai.GetComponent<AI3D>();
        ai3d.Target = walker_char;
        ai3d.state.on_ground = true;

        Projectiles = new List<Projectile>();

		Level l = new Level();
		l.load("indra");
		GameObject l_root = l.game_object();
		l_root.transform.SetParent(side_spot.transform);

    }

    GameObject fire_plasma(SceneCharacter3D character) {
		int gun = 1;
		float energy = 0;
		if (character.plasma1 < character.plasma2) {
			energy = character.plasma2;
			character.plasma2 = 0;
			gun = -1;
		} else {
			energy = character.plasma1;
			character.plasma1 = 0;
		}

        var pos = character.head.transform.position;
        pos += character.head.transform.forward * .95f;
		pos += character.head.transform.up * .35f * gun;
        var rot = character.head.transform.rotation;
        var proj = (GameObject)GameObject.Instantiate(
            plasma_prefab, 
            pos, 
            rot);
		var p = proj.GetComponent<Plasma>();
		p.set_energy(energy);
        Projectiles.Add(p);
        return proj;
    }

	void set_text(GameObject t, float text) {
		t.GetComponent<Text> ().text = string.Format ("{0}: {1}", t.name, text);
	}

    // Update is called once per frame
    void Update() {
        var ai3d = ai.GetComponent<AI3D>();
        ai3d.RunAtTarget();
        ai3d.state.on_ground = true;
		set_text (energy_text, walker_char.energy);
		set_text (shield_text, walker_char.shield);
		set_text (plasma1_text, walker_char.plasma1);
		set_text (plasma2_text, walker_char.plasma2);
		set_text (missiles_text, walker_char.missiles);
		set_text (grenades_text, walker_char.grenades);

        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (cam_is_static) {
                attach_cam_to_walker(walker_char);
            }
            else {
                cam.transform.position = side_spot.transform.position;
                cam.transform.eulerAngles = new Vector3(0, 90, 0);
                cam.transform.SetParent(side_spot.transform);
            }
            cam_is_static = !cam_is_static;
        }

        if (Input.GetMouseButtonDown(0)) {
			fire_plasma(walker_char);
        }
        if (Input.GetKeyDown(KeyCode.E)) {
            var pos = walker_char.head.transform.position;
            pos += walker_char.head.transform.forward * 1.1f;
            var rot = walker_char.head.transform.rotation.eulerAngles;
            var quat = Quaternion.Euler(0, rot.y, 0);
            var proj = (GameObject)GameObject.Instantiate(
                grenade_prefab,
                pos,
                quat); 
            var gren = proj.GetComponent<Grenade>();
            //proj.transform.Rotate(0, 0, -90);
            gren.attack_pos = gren.transform.position;
            gren.initial_speed += walker_char.state.velocity * 85;
            float ratio;
            if (rot.x > 274) {
                ratio = (360 - rot.x) / (360 - 275);
            }
            else {
                ratio = rot.x / 42.5f * -1;
            }
            gren.theta += 43f * ratio; 
            Debug.Log(ratio);
            gren.attack_time = Time.time;
            Projectiles.Add(gren);
        }
        HandleProjectiles();
    }

    void attach_cam_to_walker(SceneCharacter3D walker) {
        cam.transform.position = walker.head.transform.position;
        cam.transform.rotation = walker.head.transform.rotation;
        var angles = cam.transform.eulerAngles;
        angles.z -= 90;
        cam.transform.eulerAngles = angles;
        cam.transform.Translate(-.007f, 0, 0.07f);

        cam.transform.SetParent(walker.head.transform);
    }

	private void FixedUpdate() {
		var turn = Input.GetAxis("Horizontal");
		walker_char.Move(Input.GetAxis("Vertical"), turn, Time.fixedDeltaTime, Input.GetButton("Jump"));
		walker_char.energy_update(Time.fixedDeltaTime);
		// Get raw mouse input for a cleaner reading on more sensitive mice.
		var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

		// Scale input against the sensitivity setting and multiply that against the smoothing value.
		mouseDelta = Vector2.Scale(mouseDelta, new Vector2(sensitivity.x * smoothing.x, sensitivity.y * smoothing.y));

		// Interpolate mouse movement over time to apply smoothing delta.
		_smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
		_smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);
		walker_char.Look(_smoothMouse.x, _smoothMouse.y);
	}

    private void HandleProjectiles() {
        foreach (var p in Projectiles) {

            if (p.hit_something) {
                foreach (var c in p.exp_colors) {
                    var pos = p.transform.position;
                    var exp = (GameObject)GameObject.Instantiate(
                        TriangleExplosionPrefab,
                        pos,
                        Quaternion.identity);
                    exp.GetComponent<Explosion>().set_color(c);
                    AudioSource.PlayClipAtPoint(grenade_explode, pos);
                }
            }
            if (!p.alive) {
                Destroy(p.gameObject);
            }
        }
        Projectiles = (from p in Projectiles where p.alive select p).ToList();
    }
}
