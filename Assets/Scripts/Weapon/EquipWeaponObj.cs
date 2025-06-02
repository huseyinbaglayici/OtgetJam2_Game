using UnityEngine;

namespace Weapon
{
    public class EquipWeaponObj : MonoBehaviour
    {
        
        private void OnTriggerStay(Collider other)
        {
            CollectItem(other);
        }

        private void CollectItem(Collider other)
        {
            if (other.tag == "Player" && InputManager.Instance.BEquipPressed())
            {
                WeaponManager.Instance.bWeaponEquipped = true;
                Destroy(gameObject);
            }
        }
    }
}