using System;

namespace Vastan.Config
{
    [System.Serializable]
    class PlayerConfig
    {
        string name;
        short team;
    }

    class HullConfig
    {
        short id;
        short max_missiles;
        short max_grenades;
        short max_boosters;
        float mass;
        float max_energy;
        float energy_charge;
        float max_shields;
        float shield_charge;
        float min_shot;
        float max_shot;
        float shot_charge;
        float riding_height;
        float acceleration;
        float jump_power;
    }
}