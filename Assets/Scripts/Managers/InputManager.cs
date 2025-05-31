using Unity.Mathematics;
using UnityEngine;
public class InputManager : MonoSingleton<InputManager>
{
    public bool bInputEnabled = true;


    #region InputEnableDisable

    public void EnableInput()
    {
        bInputEnabled = true;
    }
    
    public void DisableInput()
    {
        bInputEnabled = false;
    }
    

    #endregion

    public float2 GetMovementInput()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        return new float2(moveX, moveY);
    }
    public float2 GetMousePosition()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        return new float2(mouseX, mouseY);
    }

    public bool BLeftClick()
    {
        return Input.GetMouseButton(0); // Basılı tutma
    }
}
