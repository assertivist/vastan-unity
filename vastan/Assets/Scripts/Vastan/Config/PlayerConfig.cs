using System;

namespace Vastan.Config
{
    [System.Serializable]
    class PlayerConfig
    {
        string name;
        short team;
		HullConfig hull;
    }

    class HullConfig
    {
        short id;
        short maxMissiles;
        short maxGrenades;
        short maxBoosters;
        float mass;
        float maxEnergy;
        float energyCharge;
        float maxShields;
        float shieldCharge;
        float minShot;
        float maxShot;
        float shotCharge;
        float ridingHeight;
        float acceleration;
        float jumpPower;
    }
}