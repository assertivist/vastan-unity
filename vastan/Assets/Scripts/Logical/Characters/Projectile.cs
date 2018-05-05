using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ServerSideCalculations.Characters {
    [Serializable]
    public class Projectile {
        public int Id { get; set; }

        // types:
        // 0 - plasma
        // 1 - missile
        // 2 - grenade
        public int Type { get; set; }
        public bool IsActive { get; set; }
        public Vector3 Pos { get; set; }
        public Vector3 Dir { get; set; }

        public Projectile() : this(0) { }
        public Projectile(int type) {
            Type = type;
        }
    }
}
