using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Projectile : MonoBehaviour {

    public bool alive = true;
    public bool hit_something = false;
    public List<Color> exp_colors;

    public void peterout() {
        alive = false;
    }

    public void asplode() {
        alive = false;
        hit_something = true;
    }

}
