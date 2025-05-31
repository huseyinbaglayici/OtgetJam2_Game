using UnityEngine;
using System.Collections.Generic;
using Enemy;

public class EnemyVacuumManager : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private float destructionDistance = 1.5f;
    [SerializeField] private GameObject destructionEffect;
    [SerializeField] private AudioClip destructionSound;
    [SerializeField] private bool only2DSpriteDestruction = true;

    [Header("2D Sprite Detection")]
    [SerializeField] private bool enableSpriteDetection = true;
    [SerializeField] private LayerMask spriteLayerMask = -1;
    [SerializeField] private float raycastDistance = 15f;
    [SerializeField] private int raycastCount = 12;
    [SerializeField] private float raycastRadius = 0.1f;
    [SerializeField] private float detectionAngle = 45f;

    private VacuumWeapon vacuumWeapon;
    private AudioSource audioSource;
    private List<GameObject> pulledEnemies = new List<GameObject>();

    void Start()
    {
        vacuumWeapon = GetComponent<VacuumWeapon>();
        audioSource = GetComponent<AudioSource>();

        if (vacuumWeapon == null)
        {
            Debug.LogError("VacuumWeapon component not found!");
            return;
        }

        vacuumWeapon.OnVacuumCollidersDetected += OnVacuumHit;
        vacuumWeapon.OnVacuumStopped += OnVacuumStopped;
    }

    void Update()
    {
        if (vacuumWeapon == null) return;

        if (vacuumWeapon.IsFiring)
        {
            if (enableSpriteDetection)
            {
                DetectSpriteEnemiesWithRaycast();
            }

            PullEnemies();
            CheckDestruction();
        }
    }

    private void DetectSpriteEnemiesWithRaycast()
    {
        List<GameObject> currentSpriteEnemies = new List<GameObject>();
        Vector3 firePointPos = vacuumWeapon.FirePoint.position;
        Vector3 forwardDirection = vacuumWeapon.FirePoint.forward;

        for (int i = 0; i < raycastCount; i++)
        {
            float horizontalAngle = ((float)i / raycastCount) * 360f - (detectionAngle / 2f);
            if (Mathf.Abs(horizontalAngle) > detectionAngle / 2f)
                continue;

            for (int j = -1; j <= 1; j++)
            {
                float verticalAngle = j * 15f;

                Quaternion rotation = Quaternion.AngleAxis(horizontalAngle, Vector3.up) * Quaternion.AngleAxis(verticalAngle, Vector3.right);
                Vector3 rayDirection = rotation * forwardDirection;

                RaycastHit[] hits = Physics.SphereCastAll(firePointPos, raycastRadius, rayDirection, raycastDistance, spriteLayerMask);

                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider != null)
                    {
                        ProcessSpriteHit(hit.collider.gameObject, currentSpriteEnemies);
                    }
                }
            }
        }

        CleanupSpriteEnemies(currentSpriteEnemies);
    }

    private void ProcessSpriteHit(GameObject hitObject, List<GameObject> currentSpriteEnemies)
    {
        IVacuumable vacuumable = hitObject.GetComponent<IVacuumable>();
        if (vacuumable != null && vacuumable.CanBeVacuumed)
        {
            currentSpriteEnemies.Add(hitObject);

            if (!pulledEnemies.Contains(hitObject))
            {
                pulledEnemies.Add(hitObject);
                vacuumable.OnVacuumStart();
                Debug.Log($"Started vacuuming sprite enemy: {hitObject.name}");
            }
        }
    }

    private void CleanupSpriteEnemies(List<GameObject> currentSpriteEnemies)
    {
        for (int i = pulledEnemies.Count - 1; i >= 0; i--)
        {
            if (pulledEnemies[i] == null)
            {
                pulledEnemies.RemoveAt(i);
                continue;
            }

            bool isSprite = ((1 << pulledEnemies[i].layer) & spriteLayerMask) != 0;
            if (isSprite && !currentSpriteEnemies.Contains(pulledEnemies[i]))
            {
                IVacuumable vacuumable = pulledEnemies[i].GetComponent<IVacuumable>();
                vacuumable?.OnVacuumEnd();
                pulledEnemies.RemoveAt(i);
            }
        }
    }

    private void OnVacuumHit(Collider[] colliders)
    {
        List<GameObject> current3DEnemies = new List<GameObject>();

        foreach (var collider in colliders)
        {
            if (collider == null) continue;

            bool isSprite = ((1 << collider.gameObject.layer) & spriteLayerMask) != 0;
            if (isSprite && enableSpriteDetection) continue;

            IVacuumable vacuumable = collider.GetComponent<IVacuumable>();
            if (vacuumable != null && vacuumable.CanBeVacuumed)
            {
                current3DEnemies.Add(collider.gameObject);

                if (!pulledEnemies.Contains(collider.gameObject))
                {
                    pulledEnemies.Add(collider.gameObject);
                    vacuumable.OnVacuumStart();
                }
            }
        }

        for (int i = pulledEnemies.Count - 1; i >= 0; i--)
        {
            if (pulledEnemies[i] == null)
            {
                pulledEnemies.RemoveAt(i);
                continue;
            }

            bool isSprite = ((1 << pulledEnemies[i].layer) & spriteLayerMask) != 0;
            if (!isSprite && !current3DEnemies.Contains(pulledEnemies[i]))
            {
                IVacuumable vacuumable = pulledEnemies[i].GetComponent<IVacuumable>();
                vacuumable?.OnVacuumEnd();
                pulledEnemies.RemoveAt(i);
            }
        }
    }

    private void PullEnemies()
    {
        foreach (GameObject enemy in pulledEnemies)
        {
            if (enemy == null) continue;

            IVacuumable vacuumable = enemy.GetComponent<IVacuumable>();
            Vector3 direction = (vacuumWeapon.FirePoint.position - enemy.transform.position).normalized;
            float force = vacuumWeapon.PullStrength;

            vacuumable?.OnVacuumPull(direction, force * Time.deltaTime);
        }
    }

    private void CheckDestruction()
    {
        for (int i = pulledEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = pulledEnemies[i];
            if (enemy == null)
            {
                pulledEnemies.RemoveAt(i);
                continue;
            }

            if (only2DSpriteDestruction)
            {
                bool isSprite = ((1 << enemy.layer) & spriteLayerMask) != 0;
                if (!isSprite) continue;
            }

            float distance = Vector3.Distance(vacuumWeapon.FirePoint.position, enemy.transform.position);
            if (distance <= destructionDistance)
            {
                DestroyEnemy(enemy);
                pulledEnemies.RemoveAt(i);
            }
        }
    }

    private void DestroyEnemy(GameObject enemy)
    {
        if (destructionEffect != null)
            Instantiate(destructionEffect, enemy.transform.position, enemy.transform.rotation);

        if (destructionSound != null && audioSource != null)
            audioSource.PlayOneShot(destructionSound);

        Destroy(enemy);
    }

    private void OnVacuumStopped()
    {
        foreach (GameObject enemy in pulledEnemies)
        {
            // if (enemy != null)
            // {
            //     IVacuumable vacuumable = enemy.GetComponent<IVacuumable>();
            //     vacuumable?.OnVacuumEnd();
            // }
        }

        pulledEnemies.Clear();
    }

    void OnDestroy()
    {
        if (vacuumWeapon != null)
        {
            vacuumWeapon.OnVacuumCollidersDetected -= OnVacuumHit;
            vacuumWeapon.OnVacuumStopped -= OnVacuumStopped;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (vacuumWeapon == null || vacuumWeapon.FirePoint == null) return;

        if (enableSpriteDetection)
        {
            Vector3 firePointPos = vacuumWeapon.FirePoint.position;
            Vector3 forwardDirection = vacuumWeapon.FirePoint.forward;

            Gizmos.color = Color.red;

            for (int i = 0; i < raycastCount; i++)
            {
                float horizontalAngle = ((float)i / raycastCount) * 360f - (detectionAngle / 2f);
                if (Mathf.Abs(horizontalAngle) > detectionAngle / 2f)
                    continue;

                Quaternion rotation = Quaternion.AngleAxis(horizontalAngle, Vector3.up);
                Vector3 rayDirection = rotation * forwardDirection;

                Gizmos.DrawRay(firePointPos, rayDirection * raycastDistance);
                Gizmos.DrawWireSphere(firePointPos + rayDirection * raycastDistance, raycastRadius);
            }

            Gizmos.color = Color.yellow;
            Vector3 rightEdge = Quaternion.AngleAxis(detectionAngle / 2f, Vector3.up) * forwardDirection * raycastDistance;
            Vector3 leftEdge = Quaternion.AngleAxis(-detectionAngle / 2f, Vector3.up) * forwardDirection * raycastDistance;

            Gizmos.DrawLine(firePointPos, firePointPos + rightEdge);
            Gizmos.DrawLine(firePointPos, firePointPos + leftEdge);
        }
    }
}
