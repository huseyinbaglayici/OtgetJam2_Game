using UnityEngine;
using UnityEngine.Rendering;

public class VolumeController : MonoBehaviour
{
    public Volume volume;
    public VolumeProfile[] profiles;
    public short currentProfileIndex = 0;

    private void Start()
    {
        SetVolumeProfile(0);
    }


    private void Update()
    {
        if (InputManager.Instance.BChangebuttonPressed())
        {
            currentProfileIndex++;
            Debug.LogWarning("Current Profile Index: " + currentProfileIndex);
            if(currentProfileIndex >= profiles.Length)
                currentProfileIndex = 0;
            SetVolumeProfile(currentProfileIndex);
        } 
    }


    private void SetVolumeProfile(short index)
    {
        if (index < 0 || index >= profiles.Length) return;
        volume.profile = profiles[index];
        Debug.LogWarning("Volume profile set to: " + profiles[index].name);
    }
}