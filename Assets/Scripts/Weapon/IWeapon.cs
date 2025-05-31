using UnityEngine;

namespace Weapon
{
    public interface IWeapon
    {
        // Properties
        WeaponData Data { get; }
        Transform FirePoint { get; }
        bool IsFiring { get; }
        bool IsOverheated { get; }
        bool CanFire { get; }
        float CurrentHeat { get; }
        float MaxHeat { get; }
        float HeatPercentage { get; }
        
        // Core weapon actions
        void Fire();
        void StartFiring();
        void StopFiring();
        
        // Weapon state management
        void Equip();
        void Unequip();
        void Reset();
    }
}