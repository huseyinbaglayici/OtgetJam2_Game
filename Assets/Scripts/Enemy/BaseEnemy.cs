using UnityEngine;

namespace Enemy
{
    public class BaseEnemy : MonoBehaviour, IVacuumable
    {
        [Header("Vacuum Settings")] 
        [SerializeField] private bool canBeVacuumed = true;
        [SerializeField] [Range(0f, 1f)] private float vacuumResistance = 0.2f; // 0 = direnç yok, 1 = tam direnç

        private Rigidbody rb;

        public bool CanBeVacuumed => canBeVacuumed;
        public float VacuumResistance => vacuumResistance;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public virtual void OnVacuumStart()
        {
            // Vacuum başladığında yapılacaklar (ses, efekt, vs.)
        }

        public virtual void OnVacuumEnd()
        {
            // Vacuum bittiğinde yapılacaklar
        }

        public virtual void OnVacuumPull(Vector3 direction, float force)
        {
            if (rb == null) return;

            // Resistance'ı uygula - basit çarpım
            float finalForce = force * (1f - vacuumResistance);
            
            rb.AddForce(direction * finalForce, ForceMode.Force);
        }
    }
}