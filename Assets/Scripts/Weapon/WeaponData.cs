using UnityEngine;

namespace Weapon
{
    [CreateAssetMenu(fileName = "WeaponData", menuName = "Weapon/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [Header("General")]
        public string weaponName;
        public GameObject weaponPrefab;

        [Header("Weapon Properties")]
        public float maxHeat = 100f;
        public float heatIncreaseRate = 10f;
        public float heatDecreaseRate = 15f;
        public float cooldownTime = 3f;
        public double cooldownThreshold = 30f; // overheat olduktan sonra bu degerden sonra kullanilabilir

        [Header("Vacuum Weapon Properties")]
        public float pullRadius = 5f; // Vacuum'un etkili olduğu mesafe
        public float pullStrength = 100f; // Çekme gücü

        [Header("Effects")]
        public GameObject muzzleFlashPrefab;
    }
}