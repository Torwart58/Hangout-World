
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Thry.SAO
{
    public class TimeDisplay : UdonSharpBehaviour
    {

        public UnityEngine.UI.Text textComponent;

        void Update()
        {
            textComponent.text = System.DateTime.Now.ToShortTimeString();
            if (System.DateTime.Now.Millisecond > 500)
                textComponent.text = textComponent.text.Replace(":", " ");
        }
    }
}