using UnityEngine;
using System.Collections.Generic;

public class EnemyVacuumManager : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private float destructionDistance = 1.5f;
    [SerializeField] private GameObject destructionEffect;
    [SerializeField] private AudioClip destructionSound;
    [SerializeField] private bool only2DSpriteDestruction = true; // Sadece 2D sprite'ları yok et
    
    [Header("2D Sprite Detection")]
    [SerializeField] private bool enableSpriteDetection = true;
    [SerializeField] private LayerMask spriteLayerMask = -1;
    [SerializeField] private float raycastDistance = 15f;
    [SerializeField] private int raycastCount = 12; // 360 derece etrafında kaç raycast
    [SerializeField] private float raycastRadius = 0.1f; // SphereCast için
    [SerializeField] private float detectionAngle = 45f; // Vacuum'un açısı (derece)

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
            // 2D sprite detection için raycast
            if (enableSpriteDetection)
            {
                DetectSpriteEnemiesWithRaycast();
            }
            
            PullEnemies();
            CheckDestruction();
        }
    }

    // 2D sprite'ları raycast ile tespit et
    private void DetectSpriteEnemiesWithRaycast()
    {
        List<GameObject> currentSpriteEnemies = new List<GameObject>();
        
        Vector3 firePointPos = vacuumWeapon.FirePoint.position;
        Vector3 forwardDirection = vacuumWeapon.FirePoint.forward;
        
        // Cone şeklinde raycast atma
        for (int i = 0; i < raycastCount; i++)
        {
            // Horizontal açı hesaplama
            float horizontalAngle = ((float)i / raycastCount) * 360f - (detectionAngle / 2f);
            if (Mathf.Abs(horizontalAngle) > detectionAngle / 2f)
                continue;
                
            // Vertical açı için de birkaç raycast (3D uzayda 2D sprite'ları yakalamak için)
            for (int j = -1; j <= 1; j++)
            {
                float verticalAngle = j * 15f; // -15, 0, +15 derece
                
                // Raycast yönünü hesapla
                Quaternion rotation = Quaternion.AngleAxis(horizontalAngle, Vector3.up) * 
                                    Quaternion.AngleAxis(verticalAngle, Vector3.right);
                Vector3 rayDirection = rotation * forwardDirection;
                
                // SphereCast kullan - 2D sprite'ları daha iyi yakalar
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
        
        // Artık menzilde olmayan sprite enemy'leri temizle
        CleanupSpriteEnemies(currentSpriteEnemies);
    }
    
    private void ProcessSpriteHit(GameObject hitObject, List<GameObject> currentSpriteEnemies)
    {
        IVacuumable vacuumable = hitObject.GetComponent<IVacuumable>();
        if (vacuumable != null && vacuumable.CanBeVacuumed)
        {
            currentSpriteEnemies.Add(hitObject);
            
            // Yeni sprite enemy ise listeye ekle
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
        // Sprite enemy'leri temizle
        for (int i = pulledEnemies.Count - 1; i >= 0; i--)
        {
            if (pulledEnemies[i] == null)
            {
                pulledEnemies.RemoveAt(i);
                continue;
            }
            
            // Eğer sprite layer'ında ise ve artık tespit edilmiyorsa kaldır
            bool isSprite = ((1 << pulledEnemies[i].layer) & spriteLayerMask) != 0;
            if (isSprite && !currentSpriteEnemies.Contains(pulledEnemies[i]))
            {
                GameObject enemy = pulledEnemies[i];
                IVacuumable vacuumable = enemy.GetComponent<IVacuumable>();
                vacuumable?.OnVacuumEnd();
                pulledEnemies.RemoveAt(i);
            }
        }
    }

    // Orijinal 3D collider tespit sistemi (değiştirilmedi)
    private void OnVacuumHit(Collider[] colliders)
    {
        List<GameObject> current3DEnemies = new List<GameObject>();

        foreach (var collider in colliders)
        {
            if (collider == null) continue;

            // Sprite layer'ındaki objeleri atla (raycast ile hallediliyor)
            bool isSprite = ((1 << collider.gameObject.layer) & spriteLayerMask) != 0;
            if (isSprite && enableSpriteDetection) continue;

            IVacuumable vacuumable = collider.GetComponent<IVacuumable>();
            if (vacuumable != null && vacuumable.CanBeVacuumed)
            {
                current3DEnemies.Add(collider.gameObject);

                // Yeni 3D enemy ise OnVacuumStart çağır
                if (!pulledEnemies.Contains(collider.gameObject))
                {
                    pulledEnemies.Add(collider.gameObject);
                    vacuumable.OnVacuumStart();
                }
            }
        }

        // Artık menzilde olmayan 3D enemy'leri temizle
        for (int i = pulledEnemies.Count - 1; i >= 0; i--)
        {
            if (pulledEnemies[i] == null)
            {
                pulledEnemies.RemoveAt(i);
                continue;
            }

            // Sadece 3D objeleri kontrol et (sprite'lar raycast ile hallediliyor)
            bool isSprite = ((1 << pulledEnemies[i].layer) & spriteLayerMask) != 0;
            if (!isSprite && !current3DEnemies.Contains(pulledEnemies[i]))
            {
                GameObject enemy = pulledEnemies[i];
                IVacuumable vacuumable = enemy.GetComponent<IVacuumable>();
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

            vacuumable.OnVacuumPull(direction, force * Time.deltaTime);
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

            // Sadece 2D sprite'ları yok et (3D objeleri sadece çek)
            if (only2DSpriteDestruction)
            {
                bool isSprite = ((1 << enemy.layer) & spriteLayerMask) != 0;
                if (!isSprite) continue; // 3D obje ise destruction'ı atla
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
            if (enemy != null)
            {
                IVacuumable vacuumable = enemy.GetComponent<IVacuumable>();
                vacuumable?.OnVacuumEnd();
            }
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
    
    // Debug için raycast görselleştirme
    private void OnDrawGizmosSelected()
    {
        if (vacuumWeapon == null || vacuumWeapon.FirePoint == null) return;
        
        if (enableSpriteDetection)
        {
            Vector3 firePointPos = vacuumWeapon.FirePoint.position;
            Vector3 forwardDirection = vacuumWeapon.FirePoint.forward;
            
            Gizmos.color = Color.red;
            
            // Raycast'ları göster
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
            
            // Detection cone'unu göster
            Gizmos.color = Color.yellow;
            Vector3 rightEdge = Quaternion.AngleAxis(detectionAngle / 2f, Vector3.up) * forwardDirection * raycastDistance;
            Vector3 leftEdge = Quaternion.AngleAxis(-detectionAngle / 2f, Vector3.up) * forwardDirection * raycastDistance;
            
            Gizmos.DrawLine(firePointPos, firePointPos + rightEdge);
            Gizmos.DrawLine(firePointPos, firePointPos + leftEdge);
        }
    }
}