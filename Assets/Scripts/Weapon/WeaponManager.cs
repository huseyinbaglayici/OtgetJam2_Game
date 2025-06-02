namespace Weapon
{
    public class WeaponManager : MonoSingleton<WeaponManager>
    {
        public bool bWeaponEquipped = false;
        public Weapons weapons;
        public int currentWeaponIndex = 0;
        private Weapons currentWeapon;

        private void Start()
        {
            EquipWeapon(0);
        }

        private void EquipWeapon(int index)
        {
        }
    }
}