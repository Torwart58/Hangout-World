
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class ButtonToggle : UdonSharpBehaviour
{
    public GameObject Toggle;
    public void ToggleObject()
    {
        Toggle.SetActive(!Toggle.activeSelf);
    }
}
