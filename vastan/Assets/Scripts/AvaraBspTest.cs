using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AvaraBspTest : MonoBehaviour
{
    public float speed = 200f;
    public GameObject static_fab;
    string[] shapes = {
        //"3000_bspFlag.avarabsp",
        "203_Missile.avarabsp",
        //"411_w1x1.avarabsp",
        "102_Avara A.avarabsp",
        "215_Small HECTOR.avarabsp",
        "460_Sphere16.avarabsp",
        //"601_GroundArrow.avarabsp",
        //"705_Mushroom.avarabsp",
        //"610_Vines.avarabsp",
        "1004_Shark4.avarabsp",
        /*"1005_Cloud7.avarabsp",
        "1008_Steering.avarabsp",
        "1010_Ship.avarabsp",
        "1015_Laser.avarabsp",
        "1016_Police.avarabsp",
        "1017_Subway.avarabsp",
        "1018_Tickets.avarabsp",
        "1019_Door.avarabsp",
        "1020_Bookcase.avarabsp",
        "1021_Guy.avarabsp",
        "1022_Commish.avarabsp",
        "1023_Safe.avarabsp",
        "1024_Sun.avarabsp",
        "1025_Cow.avarabsp",
        "1026_Door2.avarabsp",
        "1027_Bat.avarabsp"*/
    };
    List<GameObject> objs = new List<GameObject>();
	// Use this for initialization
	void Start () {
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        var count = 0;
		foreach(string file in shapes) {
            TextAsset ta = (TextAsset)Resources.Load(file);
            if (!ta) {
                Debug.Log("Error, couldn't read " + file);
                continue;
            }
            var gb = new GeomBuilder();
            gb.init();
            gb.add_avara_bsp(ta.text, Color.red, Color.white);
            Mesh m = gb.get_mesh();
            Vector3 pos = new Vector3(count * 5f, 0, 0);
            GameObject c = (GameObject)GameObject.Instantiate(static_fab, pos, Quaternion.identity);
            c.GetComponent<MeshFilter>().mesh = m;
            c.AddComponent<DrawNormals>();
            c.transform.SetParent(transform.parent);
            objs.Add(c);
            count++;
        }
	}
	
	// Update is called once per frame
	void Update () {
        var dt = Time.deltaTime;
		foreach(GameObject s in objs) {
            //s.transform.Rotate(0, speed * dt, 0);
        }
	}
}
