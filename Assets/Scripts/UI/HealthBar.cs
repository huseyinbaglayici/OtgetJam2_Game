using System;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoSingleton<HealthBar>
{
    public Image fillImage; // Dolu bar Image componenti

    // 0 ile 1 arasında bir değer alır (0 = boş, 1 = dolu)
    public void SetFillAmount(float fillAmount)
    {
        fillAmount = Mathf.Clamp01(fillAmount); // 0-1 aralığında sınırla
        fillImage.fillAmount = fillAmount;
    }

    private void Update()
    {
        SetFillAmount(GetHeat());
    }

    private float GetHeat()
    {
        return WeaponManager.Instance.GetCurrentWeaponHeat();
    }
}