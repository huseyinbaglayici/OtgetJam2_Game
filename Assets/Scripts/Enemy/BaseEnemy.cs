using UnityEngine;

namespace Enemy
{
    public class BaseEnemy : MonoBehaviour
    {
        #region Variables
        [SerializeField] private float rotationSpeed = 2f;
        protected Transform playerTransform;
        #endregion
        
        [SerializeField] private float vacuumResistance = 0.2f;
        public Rigidbody rb;
    
        public float VacuumResistance => vacuumResistance;
        
        protected virtual void Start()
        {
            GetPlayerTransform();
        }
        
        protected virtual void Update()
        {
            RotateTowardsPlayer();
        }
        
        protected Transform GetPlayerTransform()
        {
            if (playerTransform == null && PlayerMovementController.Instance != null)
            {
                playerTransform = PlayerMovementController.Instance.transform;
            }
            
            return playerTransform;
        }
        
        protected void RotateTowardsPlayer()
        {
            if (GetPlayerTransform() == null) return;
            
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            direction.y = 0; // Y ekseni rotasyonunu engelle
            
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    
        public virtual void OnVacuumStart() { }
        public virtual void OnVacuumEnd() { }
        public virtual void OnVacuumPull(Vector3 direction, float force)
        {
            if (rb == null) return;
            rb.AddForce(direction * (force * (1f - vacuumResistance)));
        }
    }
}