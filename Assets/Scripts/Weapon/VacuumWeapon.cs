using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class VacuumWeapon : MonoBehaviour
{
    [Header("Vacuum Settings")] public float pullRadius = 5f;
    public float pullStrength = 100f;
    public LayerMask vacuumLayers = -1;
    public Transform firePoint;

    [Header("Heat System")] public float maxHeat = 100f;
    public float heatIncreaseRate = 30f;
    public float heatDecreaseRate = 20f;
    public float cooldownTime = 3f;
    public float cooldownThreshold = 20f;

    [Header("Effects")] public ParticleSystem vacuumEffect;
    public AudioSource vacuumSound;
    public LineRenderer vacuumLine;

    // Private variables
    private bool isFiring = false;
    private bool isOverheated = false;
    private float currentHeat = 0f;
    private float lastOverheatTime = -999f;
    private List<Rigidbody> objectsInRange = new List<Rigidbody>();

    [FormerlySerializedAs("heatIncreaseRateMultiplier")] [SerializeField]
    private float heatMultiplier = 1;


    // Public properties
    public bool IsFiring => isFiring;
    public bool IsOverheated => isOverheated;
    public bool CanFire => !isOverheated && (lastOverheatTime == 0f || Time.time - lastOverheatTime > cooldownTime);
    public float CurrentHeat => currentHeat;
    public float HeatPercentage => currentHeat / maxHeat;

    void Start()
    {
        InitializeVacuum();
    }

    void Update()
    {
        DebugStats();
        HandleHeatSystem();

        if (isFiring && CanFire)
        {
            PerformVacuum();
            IncreaseHeat();
        }
    }

    private void DebugStats()
    {
        Debug.LogWarning("Current Heat: " + currentHeat);
    }

    private void InitializeVacuum()
    {
        // Line Renderer setup
        if (vacuumLine == null)
            vacuumLine = GetComponent<LineRenderer>();

        if (vacuumLine == null)
            vacuumLine = gameObject.AddComponent<LineRenderer>();

        vacuumLine.startWidth = 0.1f;
        vacuumLine.endWidth = 0.05f;
        vacuumLine.material = new Material(Shader.Find("Sprites/Default"));

        // Renk ayarı
        vacuumLine.startColor = Color.cyan;
        vacuumLine.endColor = Color.cyan;

        vacuumLine.enabled = false;

        // FirePoint kontrolü
        if (firePoint == null)
            firePoint = transform;
    }


    public void StartFiring()
    {
        if (!CanFire) return;
        Debug.LogWarning("working");
        isFiring = true;
        ShowVacuumEffects();
        PlayVacuumSound();
    }

    public void StopFiring()
    {
        Debug.LogWarning("stopped");
        isFiring = false;
        HideVacuumEffects();
        StopVacuumSound();
    }

    private void PerformVacuum()
    {
        // SphereCast ile nesneleri bul
        RaycastHit[] hits = Physics.SphereCastAll(
            firePoint.position,
            pullRadius,
            firePoint.forward,
            pullRadius * 2f,
            vacuumLayers
        );

        objectsInRange.Clear();

        foreach (var hit in hits)
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                objectsInRange.Add(rb);
                ApplyVacuumForce(rb);
            }
        }
    }

    private void ApplyVacuumForce(Rigidbody targetRb)
    {
        Vector3 direction = (firePoint.position - targetRb.position).normalized;
        float distance = Vector3.Distance(firePoint.position, targetRb.position);

        // Mesafe ile ters orantılı güç
        float forceMagnitude = pullStrength / (distance + 1f);

        targetRb.AddForce(direction * forceMagnitude * Time.deltaTime, ForceMode.Force);

        // Hava direnci efekti
        targetRb.linearVelocity *= 0.98f;
    }

    private void HandleHeatSystem()
    {
        if (!isFiring)
        {
            // Soğutma
            currentHeat = Mathf.Max(0f, currentHeat - heatDecreaseRate * Time.deltaTime * heatMultiplier);
        }

        // Overheat kontrolü
        if (currentHeat >= maxHeat && !isOverheated)
        {
            isOverheated = true;
            lastOverheatTime = Time.time;
            StopFiring();
        }

        // Overheat'den çık
        if (isOverheated && currentHeat <= cooldownThreshold)
        {
            isOverheated = false;
        }
    }

    private void IncreaseHeat()
    {
        currentHeat = Mathf.Min(maxHeat, currentHeat + heatIncreaseRate * Time.deltaTime * heatMultiplier);
    }

    private void ShowVacuumEffects()
    {
        if (vacuumLine != null)
        {
            vacuumLine.enabled = true;
            vacuumLine.SetPosition(0, firePoint.position);
            vacuumLine.SetPosition(1, firePoint.position + firePoint.forward * pullRadius * 2f);
        }

        if (vacuumEffect != null && !vacuumEffect.isPlaying)
        {
            vacuumEffect.Play();
        }
    }

    private void HideVacuumEffects()
    {
        if (vacuumLine != null)
        {
            vacuumLine.enabled = false;
        }

        if (vacuumEffect != null)
        {
            vacuumEffect.Stop();
        }
    }

    private void PlayVacuumSound()
    {
        if (vacuumSound != null && !vacuumSound.isPlaying)
        {
            vacuumSound.Play();
        }
    }

    private void StopVacuumSound()
    {
        if (vacuumSound != null)
        {
            vacuumSound.Stop();
        }
    }

    // Debug
    private void OnDrawGizmosSelected()
    {
        if (firePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(firePoint.position, pullRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(firePoint.position, firePoint.forward * pullRadius * 2f);
        }
    }
}