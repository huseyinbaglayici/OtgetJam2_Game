using System;
using UnityEngine;
using UnityEngine.Events;

namespace Weapon
{
    public abstract class WeaponBase : MonoBehaviour, IWeapon
    {
        [Header("Weapon Setup")]
        public WeaponData data;
        public Transform firePoint;
        public Animator weaponAnimator;
        
        // Weapon state
        protected bool isReloading = false;
        protected bool isFiring = false;
        protected bool isEquipped = false;
        
        // Heat System
        protected float currentHeat = 0f;
        protected bool isOverheated = false;
        protected bool isCoolingDown = false;
        
        // Effects
        private GameObject muzzleFlashInstance;
        
        // Events
        public UnityAction OnFire;
        public UnityAction OnOverheat;
        public UnityAction OnCooldown;
        public UnityAction OnHeatChanged;
        public UnityAction OnEquip;
        public UnityAction OnUnequip;

        #region IWeapon Properties
        public WeaponData Data => data;
        public Transform FirePoint => firePoint;
        public bool IsFiring => isFiring;
        public bool IsOverheated => isOverheated;
        public bool CanFire => isEquipped && !isOverheated && !isCoolingDown;
        public float CurrentHeat => currentHeat;
        public float MaxHeat => data.maxHeat;
        public float HeatPercentage => currentHeat / data.maxHeat;
        #endregion

        protected virtual void Start()
        {
            InitializeWeapon();
        }

        protected virtual void InitializeWeapon()
        {
            currentHeat = 0f;
            
            // Muzzle flash setup
            if(data.muzzleFlashPrefab != null && firePoint != null)
            {
                muzzleFlashInstance = Instantiate(data.muzzleFlashPrefab, firePoint.position, firePoint.rotation);
                muzzleFlashInstance.SetActive(false);
            }
        }

        protected virtual void Update()
        {
            HandleHeatSystem();
        }

        #region Abstract Methods
        protected abstract void PerformFire();
        protected abstract RaycastHit[] PerformRaycast();
        #endregion

        #region Public Methods
        public virtual void Fire()
        {
            if (!CanFire) return;
            
            PerformFire();
            ShowMuzzleFlash();
            PlayFireAnimation();
            
            OnFire?.Invoke();
        }

        public virtual void StartFiring()
        {
            if (!CanFire) return;
            isFiring = true;
        }

        public virtual void StopFiring()
        {
            isFiring = false;
        }

        public virtual void Equip()
        {
            isEquipped = true;
            gameObject.SetActive(true);
            PlayEquipAnimation();
            OnEquip?.Invoke();
        }

        public virtual void Unequip()
        {
            isEquipped = false;
            StopFiring();
            gameObject.SetActive(false);
            OnUnequip?.Invoke();
        }

        public virtual void Reset()
        {
            isFiring = false;
            currentHeat = 0f;
            isOverheated = false;
            isCoolingDown = false;
            OnHeatChanged?.Invoke();
        }
        #endregion

        #region Protected Helper Methods
        protected virtual void HandleHeatSystem()
        {
            if (isCoolingDown) return; // Cooldown sırasında heat sistemi durur
            
            if (isFiring)
            {
                AddHeat(data.heatIncreaseRate * Time.deltaTime);
            }
            else
            {
                CoolDown(data.heatDecreaseRate * Time.deltaTime);
            }
        }

        protected virtual void AddHeat(float amount)
        {
            currentHeat = Mathf.Min(currentHeat + amount, data.maxHeat);
            OnHeatChanged?.Invoke();

            if (currentHeat >= data.maxHeat && !isOverheated)
            {
                TriggerOverheat();
            }
        }

        protected virtual void CoolDown(float amount)
        {
            currentHeat = Mathf.Max(currentHeat - amount, 0f);
            OnHeatChanged?.Invoke();
        }

        protected virtual void TriggerOverheat()
        {
            isOverheated = true;
            isFiring = false;
            StartCoroutine(CooldownCoroutine());
            OnOverheat?.Invoke();
        }

        protected virtual void ShowMuzzleFlash()
        {
            if (muzzleFlashInstance != null)
            {
                muzzleFlashInstance.SetActive(true);
                Invoke(nameof(HideMuzzleFlash), 0.1f);
            }
        }

        protected virtual void HideMuzzleFlash()
        {
            if (muzzleFlashInstance != null)
                muzzleFlashInstance.SetActive(false);
        }

        protected virtual void PlayFireAnimation()
        {
            if (weaponAnimator != null)
                weaponAnimator.SetTrigger("Fire");
        }

        protected virtual void PlayEquipAnimation()
        {
            if (weaponAnimator != null)
                weaponAnimator.SetTrigger("Equip");
        }

        protected virtual void PlayReloadAnimation()
        {
            if (weaponAnimator != null)
                weaponAnimator.SetTrigger("Reload");
        }
        #endregion

        #region Coroutines
        protected virtual System.Collections.IEnumerator CooldownCoroutine()
        {
            isCoolingDown = true;
            
            // Cooldown süresince bekle
            yield return new WaitForSeconds(data.cooldownTime);
            
            // Heat'i threshold seviyesine düşür
            currentHeat = (float)data.cooldownThreshold;
            
            // Sistemleri yeniden aktif et
            isOverheated = false;
            isCoolingDown = false;
            
            OnCooldown?.Invoke();
            OnHeatChanged?.Invoke();
        }
        #endregion
    }
}