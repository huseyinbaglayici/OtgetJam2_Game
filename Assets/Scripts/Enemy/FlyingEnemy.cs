using UnityEngine;

namespace Enemy
{
    public class FlyingEnemy : BaseEnemy
    {
        [Header("Flying Enemy Settings")]
        public float MoveSpeed = 5f;
        public float Damage = 10f;

        protected override void Start()
        {
            rb = GetComponent<Rigidbody>();
            base.Start();
        }

        protected override void Update()
        {
            RotateTowardsPlayer();
            base.Update();
        }
    }
}