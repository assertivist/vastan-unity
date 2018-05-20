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
    private AI3D ai3d;
    public Camera cam;
    private bool cam_is_static = true;
    private SceneCharacter3D walker_char;
    public GameObject TriangleExplosionPrefab;

    public List<Projectile> Projectiles { get; set; }

    public AudioSource grenade_explode;
    public AudioSource wall_hit;

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

    public float t = 0;

    GameObject current_l_root;
    Level current_level;

    private string[] levels = {
        "bwadi",
        "icebox-classic",
        "iceboxClassic.pict",
        "Tesla.pict"
        /*"abtf",
        "bodhi",
        "coromoran",
        "errant",
        "IYA",
        "phosphorus",
        "nightsky",
        "on-the-rocks",
        "quell",
        "stratocaster",
        "vatnajokull",
        "naloxone",
        "nightsky"*/
    };

    private void OnGUI() {
        GUILayout.BeginVertical();
        foreach(string level in levels) {
            if (GUILayout.Button(level)) {
                switch_level(level);
                walker_char.transform.position = current_level.get_incarn().position;
                Transform incarn = current_level.get_incarn();
                walker_char.state.pos = incarn.position;
                walker_char.state.angle = incarn.rotation.eulerAngles.y;
                walker_char.transform.SetPositionAndRotation(incarn.position, incarn.rotation);
            }
        }
    }

    // Use this for initialization
    void Start() {
        walker_char = walker.GetComponent<SceneCharacter3D>();
        // Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
        walker_char.recolor_walker(new Color(.7f, 0f, .3f));
        walker_char.BaseCharacter.Id = 1000;

        //Color c = new Color(.2f, .2f, .2f);
        //Mesh m = floor.GetComponent<MeshFilter>().sharedMesh;
        //var colors = from n in Enumerable.Range(0, m.vertices.Length) select c;
        //m.colors = colors.ToArray();

        ai3d = ai.GetComponent<AI3D>();
        ai3d.Target = walker_char;
        ai3d.state.on_ground = true;

        Projectiles = new List<Projectile>();

        switch_level("phosphorus");
    }

    void switch_level(string level) {
        if (current_l_root) {
            Destroy(current_l_root);
        }
        Level l = new Level(GetComponent<Camera>());
        l.load(level);
        current_l_root = l.game_object();
        current_l_root.transform.SetParent(side_spot.transform);
        current_level = l;
    }

    void fire_grenade(SceneCharacter3D character) {
        if (character.grenades < 1) {
            return;
        }
        //character.grenades--;
        Projectiles.Add(Grenade.Fire(character, grenade_prefab));
    }

    void fire_plasma(SceneCharacter3D character) {
        if (!character.can_fire_plasma()) {
            return;
        }
        Projectiles.Add(Plasma.Fire(character, plasma_prefab));
    }

    void set_text(GameObject t, float text) {
        if (text < 1) {
            text = Mathf.Round (text * 100);
        }
        t.GetComponent<Text>().text = string.Format("{0}: {1}", t.name, text);
    }

    // Update is called once per frame
    void Update() {
        ai3d.RunAtTarget();
        ai3d.state.on_ground = true;
        ai3d.recolor_walker(Color.green);
        set_text (energy_text, walker_char.energy / 5f);
        set_text (shield_text, walker_char.shield / 3f);
        set_text (plasma1_text, walker_char.plasma1 / .8f);
        set_text (plasma2_text, walker_char.plasma2 / .8f);
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
            fire_grenade(walker_char);
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            switch_level("indra");
        }

        HandleProjectiles();

        t += Time.deltaTime;
        if (t > 2) {
            t = 0;
            //walker_char.was_hit();
            //fire_plasma(ai3d);
        }

        //walker_char.glow_walker(Color.Lerp(Color.black, Color.white, t));
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
        walker_char.energy_update(Time.fixedDeltaTime * Game.AVARA_FPS);
        ai3d.energy_update(Time.fixedDeltaTime * Game.AVARA_FPS);
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
                var pos = p.transform.position;
                foreach (var c in p.exp_colors) {
                    var exp = (GameObject)GameObject.Instantiate(
                        TriangleExplosionPrefab,
                        pos,
                        Quaternion.identity);
                    exp.GetComponent<Explosion>().set_color(c);
                }
                if (p.GetType().Equals(typeof(Grenade)))
                    Instantiate(grenade_explode, pos, Quaternion.identity);
                //GameClient.PlayClipAt(grenade_explode, pos);
                if (p.hit_wall) {
                    Instantiate(wall_hit, pos, Quaternion.identity);
                    //GameClient.PlayClipAt(wall_hit, pos);
                }
            }
            if (!p.alive) {
                Destroy(p.gameObject);
            }
        }
        Projectiles = (from p in Projectiles where p.alive select p).ToList();
    }

    
}
