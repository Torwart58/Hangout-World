
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class DualToggle : UdonSharpBehaviour
{
    public GameObject Toggle;
    public GameObject Toggle2;
    public void ToggleObject()
    {
        Toggle.SetActive(!Toggle.activeSelf);
        Toggle2.SetActive(!Toggle2.activeSelf);
    }
}
