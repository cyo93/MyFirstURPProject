using UnityEngine;
using ScriptableObjectArchitecture;

namespace HeroicArcade.CC
{
    public class Weapon : MonoBehaviour
    {
        public FloatReference weaponMaxEnergy = null;
        public FloatReference weaponCurrentEnergy = null;
        public FloatReference weaponEnergyConsumption = null;
        public FloatReference hitImpactStrength = null;

        public Transform muzzle;
        public ParticleSystem muzzleFlash;
        public TrailRenderer bulletTracer;
        public ParticleSystem hitEffect; // This shouldn't be a prefab reference (it doesn't work.)
        public Transform hitPoint;
    }
}