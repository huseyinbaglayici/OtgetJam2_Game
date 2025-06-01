using UnityEngine;

namespace Weapon
{
    [CreateAssetMenu(fileName = "NewWeaponData", menuName = "Weapon/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        public string weaponName;
        
        public float maxHeat;
        public float heatIncreaseRate;
        public float heatDecreaseRate;
        public float coolDowntime;
        public float cooldownThreshold; // overheat olduktan sonra kullanabilmek icin inmesi gereken deger

        public float pullRadius;
        public float pullStrenght;

    }
}