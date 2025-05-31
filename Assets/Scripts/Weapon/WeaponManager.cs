using UnityEngine;

public class WeaponManager : MonoSingleton<WeaponManager>
{
    [Header("Weapon")]
    public VacuumWeapon vacuumWeapon;
    
    [Header("Settings")]
    public bool hasVacuumWeapon = false; // Oyun başında false, collect edilince true
    
    // Properties
    public bool HasVacuumWeapon => hasVacuumWeapon;
    public bool IsUsingVacuum => hasVacuumWeapon;
    
    void Start()
    {
        InitializeWeapon();
    }
    
    void Update()
    {
        HandleInput();
    }
    
    private void InitializeWeapon()
    {
        // Vacuum başlangıçta deaktif
        if (vacuumWeapon != null)
        {
            vacuumWeapon.gameObject.SetActive(hasVacuumWeapon);
        }
    }
    
    // ReSharper disable Unity.PerformanceAnalysis
    private void HandleInput()
    {
        // Vacuum silahı yoksa input almayı bırak
        if (!hasVacuumWeapon) return;
        
        // Left click kontrolü
        if (InputManager.Instance.BLeftClick())
        {
            StartFiring();
        }
        else
        {
            StopFiring();
        }
    }
    
    private void StartFiring()
    {
        if (vacuumWeapon != null && hasVacuumWeapon)
        {
            vacuumWeapon.StartFiring();
        }
    }
    
    private void StopFiring()
    {
        if (vacuumWeapon != null)
        {
            vacuumWeapon.StopFiring();
        }
    }
    
    // Vacuum silahını collect etmek için
    public void CollectVacuumWeapon()
    {
        hasVacuumWeapon = true;
        
        if (vacuumWeapon != null)
        {
            vacuumWeapon.gameObject.SetActive(true);
        }
        
        Debug.Log("Vacuum Weapon Collected!");
    }
    
    
    public float GetCurrentWeaponHeat()
    {
        if (hasVacuumWeapon && vacuumWeapon != null)
        {
            return vacuumWeapon.HeatPercentage;
        }
        return 0f;
    }
    
    public bool IsCurrentWeaponOverheated()
    {
        if (hasVacuumWeapon && vacuumWeapon != null)
        {
            return vacuumWeapon.IsOverheated;
        }
        return false;
    }
}