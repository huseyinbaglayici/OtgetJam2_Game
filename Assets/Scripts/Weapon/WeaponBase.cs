using UnityEngine;

namespace Weapon
{
    public abstract class WeaponBase : MonoBehaviour
    {
        public bool canPerformAttack = false;
        public float cooldownTime = 1f;
        protected bool isAttacking = false;
        protected float lastAttackTime = -Mathf.Infinity;
        
        public Animator weaponAnimator;
        public AudioSource audioSource;

        public virtual void StartAttack()
        {
            if (!canPerformAttack || Time.time < lastAttackTime + cooldownTime)
                return;
            isAttacking = true;
        }
    }
}