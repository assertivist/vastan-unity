using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class AvaraBspTest : MonoBehaviour
{
    public float speed = 200f;
    public GameObject static_fab;
    string[] shapes = {
       /* "1000_bspIO.avarabsp",
"1001_bspMartianStar.avarabsp",
"1001_Godzilla.avarabsp",
"1001_K.avarabsp",
"1002_bspMartianStarD.avarabsp",
"1002_Silo.avarabsp",
"1002_SpinRect.avarabsp",
"1003_bspPlate.avarabsp",
"1003_bspSolarSystem.avarabsp",
"1003_Hand.avarabsp",
"1004_Shark4.avarabsp",
"1005_bspSquareVeins.avarabsp",
"1005_Cloud7.avarabsp",
"1006_bspSun.avarabsp",
"1007_bsp1007.avarabsp",
"1008_bsp1008.avarabsp",
"1008_Steering.avarabsp",
"1009_bsp1009.avarabsp",
"100_NeoAvara.avarabsp",
"1010_bsp1010.avarabsp",
//"1010_Ship.avarabsp",
"1011_bsp1011.avarabsp",
"1012_bspHill.avarabsp",
"1013_bspHill2.avarabsp",
"1015_Laser.avarabsp",
//"1016_Police.avarabsp",
//"1017_Subway.avarabsp",
//"1018_Tickets.avarabsp",
"1019_Door.avarabsp",
"101_MComplete.avarabsp",
"1020_Bookcase.avarabsp",
"1021_Guy.avarabsp",
"1022_Commish.avarabsp",
"1023_Safe.avarabsp",
"1024_Sun.avarabsp",
"1025_Cow.avarabsp",
"1026_Door2.avarabsp",
"1027_Bat.avarabsp",
"102_Avara A.avarabsp",
"1200_Mine-Flag.avarabsp",
"1201_bspShockBall.avarabsp",
//"1264_Big Missile (active).avarabsp",
"1265_Shrub.avarabsp",
"1270_Coffee Cup.avarabsp",
"129_Shark.avarabsp",
"1311_bspSettingSun.avarabsp",
"1350_Blip Gem.avarabsp",
"1441_Arrow.avarabsp",
"1442_block pattern.avarabsp",
"1443_target goal.avarabsp",
"200_grenadeSight.avarabsp",
"201_GrenadeSight.top.avarabsp",
"202_marker.avarabsp",
"203_Missile.avarabsp",
"204_DirInd.avarabsp",
"205_TargetOff.avarabsp",
"206_TargetOk.avarabsp", */
"207_SmartHairs.avarabsp",
"208_SmartSight.avarabsp",
"210_Walker.Head.BBox.avarabsp",
"211_Walker.Leg.High.avarabsp",
"212_Walker.Leg.Low.avarabsp",
"215_Small HECTOR.avarabsp",
"216_Mid HECTOR.avarabsp",
"217_Large HECTOR.avarabsp",
"220_ScoutCopter.avarabsp",
"230_transport.avarabsp",
"2368_bspRepair.avarabsp",
"240_powerup.avarabsp",
"250_football.avarabsp",
"251_goal2.avarabsp",
"252_Pill.avarabsp",
"2843_bspHealth.avarabsp",
"3000_bspFlag.avarabsp",
"3001_bspSpinny.avarabsp",
"3002_bspPool.avarabsp",
"300_patchsphere.dxf.avarabsp",
"310_mine.asleep.avarabsp",
"311_mine.active.avarabsp",
"3639_bspTrain.avarabsp",
"400_BoxTemplate.geom.avarabsp",
"401_UnitRect.geom.avarabsp",
"411_w1x1.avarabsp",
"421_w2x1.avarabsp",
"422_w2x2.avarabsp",
"431_w3x1.avarabsp",
"432_w3x2.avarabsp",
"433_w3x3.avarabsp",
"441_w4x1.avarabsp",
"4497_bspLiteNuke.avarabsp",
"451_w5x1.avarabsp",
"460_Sphere16.avarabsp",
"461_Sphere16.avarabsp",
"462_Sphere4.avarabsp",
"463_Sphere4.avarabsp",
"464_sphere2.avarabsp",
"465_sphere2.avarabsp",
"4888_bspCamoHead.avarabsp",
"5001_Golden Light.avarabsp",
"5002_Golden Med.avarabsp",
"5003_Golden Heavy.avarabsp",
"500_sliver-small.avarabsp",
"501_Sliver0.avarabsp",
"502_Sliver1.avarabsp",
"503_Sliver2.avarabsp",
"5495_Colored Crate.avarabsp",
"550_SimpleDoor.avarabsp",
"560_switchOff.avarabsp",
"561_switchOn.avarabsp",
"562_wallSwitchOff.avarabsp",
"563_wallSwitchOn.avarabsp",
"600_GroundStar.avarabsp",
"601_GroundArrow.avarabsp",
"602_GroundArrowLeft.avarabsp",
"610_Vines.avarabsp",
"611_Crack.avarabsp",
"650_TriPyramid.avarabsp",
"6556_Anera logo.avarabsp",
"6999_sleekShip.avarabsp",
"701_ON.avarabsp",
"702_OFF.avarabsp",
"703_Tower.avarabsp",
"704_Grid10.avarabsp",
"705_Mushroom.avarabsp",
"706_HingeDoor.avarabsp",
"707_Flower.avarabsp",
"708_Tree.avarabsp",
"709_Grid7.5.avarabsp",
"710_Hill.avarabsp",
"711_Street.avarabsp",
"712_Turn.avarabsp",
"713_Lock.avarabsp",
"714_DeadTree.avarabsp",
"715_BigIce.avarabsp",
"716_Ice.avarabsp",
"717_Shell.avarabsp",
"720_emptycube.avarabsp",
"721_doublecube.avarabsp",
"722_FloorFrame.avarabsp",
"723_GobbleRect.geom.avarabsp",
"724_Triangle.geom.avarabsp",
"7478_bspLazerHull.avarabsp",
"7875_bspGunThing.avarabsp",
"800_GuardGun.avarabsp",
"801_Bolt.avarabsp",
"802_Smart Missile.avarabsp",
"803_Seeker.avarabsp",
"804_igloo.avarabsp",
"806_dome.avarabsp",
"807_Parasite.avarabsp",
"808_superufo.avarabsp",
"809_shuriken.avarabsp",
"812_plat.dxf.avarabsp",
"820_grenade.avarabsp",
"830_starfighter.avarabsp",
"831_Tractortower.avarabsp",
"832_Shooter2.avarabsp",
"8401_jungleFlat.avarabsp",
"8888_Goody Holder.avarabsp",
"8889_Extra Life Holder.avarabsp",
"9000_Extra Life.avarabsp",
"9261_bspSmallGem.avarabsp",

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
            Vector3 pos = new Vector3(count * 3f, 0, 0);
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
