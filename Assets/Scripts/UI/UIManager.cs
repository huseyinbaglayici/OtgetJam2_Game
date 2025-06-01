using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoSingleton<UIManager>
{
    public Image baseImage;
    public Image fillImage;
    private bool bUIBlack = false;

    public void SetFillAmount(float fillAmount)
    {
        fillAmount = Mathf.Clamp01(fillAmount); // 0-1 aralığında sınırla
        fillImage.fillAmount = fillAmount;
    }

    public void SetUIColor()
    {
        if (InputManager.Instance.bDoomWorldEnabled)
        {
            bUIBlack = false;
        }

        if (bUIBlack) return;
        baseImage.color = Color.black;
        fillImage.color = Color.black;
    }

    // private void Update()
    // {
    //     SetFillAmount(GetHeat());
    // }
    //
    // private float GetHeat()
    // {
    //     //return WeaponManager.Instance.GetCurrentWeaponHeat();
    // }
}
