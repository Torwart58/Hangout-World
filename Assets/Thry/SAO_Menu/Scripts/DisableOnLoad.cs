
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Thry
{
    public class DisableOnLoad : UdonSharpBehaviour
    {
        public GameObject[] list;

        void Start()
        {
            foreach (GameObject o in list) o.SetActive(false);
        }
    }
}