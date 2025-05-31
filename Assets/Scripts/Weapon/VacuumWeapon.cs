using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System;

public class VacuumWeapon : MonoBehaviour
{
    [Header("Vacuum Settings")] 
    public float pullRadius = 5f;
    public float pullStrength = 100f;
    public LayerMask vacuumLayers = -1;
    public Transform firePoint;

    [Header("Heat System")] 
    public float maxHeat = 100f;
    public float heatIncreaseRate = 30f;
    public float heatDecreaseRate = 20f;
    public float cooldownTime = 3f;
    public float cooldownThreshold = 20f;

    [Header("Effects")] 
    public ParticleSystem vacuumEffect;
    public AudioSource vacuumSound;
    public LineRenderer vacuumLine;

    // Events - ESKİ EVENT KALDIRILDI, YENİ EVENT EKLENDİ
    public event Action<Collider[]> OnVacuumCollidersDetected;  // YENİ EVENT
    public event Action OnVacuumStarted;
    public event Action OnVacuumStopped;
    public event Action OnOverheated;
    public event Action OnCooledDown;

    // Private variables
    private bool isFiring = false;
    private bool isOverheated = false;
    private float currentHeat = 0f;
    private float lastOverheatTime = -999f;

    [FormerlySerializedAs("heatIncreaseRateMultiplier")] [SerializeField]
    private float heatMultiplier = 1;

    // Public properties
    public bool IsFiring => isFiring;
    public bool IsOverheated => isOverheated;
    public bool CanFire => !isOverheated && (lastOverheatTime == 0f || Time.time - lastOverheatTime > cooldownTime);
    public float CurrentHeat => currentHeat;
    public float HeatPercentage => currentHeat / maxHeat;
    public float PullRadius => pullRadius;
    public float PullStrength => pullStrength;
    public Transform FirePoint => firePoint;
    public float Range => pullRadius; // EnemyVacuumManager için eklendi

    void Start()
    {
        InitializeVacuum();
    }

    void Update()
    {
        HandleHeatSystem();

        if (isFiring && CanFire)
        {
            PerformVacuumScan();
            IncreaseHeat();
        }
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
        
        Debug.Log("Vacuum weapon activated");
        isFiring = true;
        ShowVacuumEffects();
        PlayVacuumSound();
        
        OnVacuumStarted?.Invoke();
    }

    public void StopFiring()
    {
        Debug.Log("Vacuum weapon deactivated");
        isFiring = false;
        HideVacuumEffects();
        StopVacuumSound();
        
        OnVacuumStopped?.Invoke();
    }

    // DEĞİŞTİRİLEN METHOD - Artık OverlapSphere kullanıyor
    private void PerformVacuumScan()
    {
        // OverlapSphere ile etrafındaki tüm collider'ları bul
        Collider[] colliders = Physics.OverlapSphere(
            firePoint.position, 
            pullRadius, 
            vacuumLayers
        );

        // Event ile collider'ları diğer sistemlere bildir
        OnVacuumCollidersDetected?.Invoke(colliders);
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
            OnOverheated?.Invoke();
        }

        // Overheat'den çık
        if (isOverheated && currentHeat <= cooldownThreshold)
        {
            isOverheated = false;
            OnCooledDown?.Invoke();
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

    // Heat sistemi için public metodlar
    public void SetHeatMultiplier(float multiplier)
    {
        heatMultiplier = multiplier;
    }

    public void AddHeat(float amount)
    {
        currentHeat = Mathf.Min(maxHeat, currentHeat + amount);
    }

    public void ReduceHeat(float amount)
    {
        currentHeat = Mathf.Max(0f, currentHeat - amount);
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