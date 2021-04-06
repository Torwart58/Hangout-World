namespace Hirabiki.Udon.Works
{
    using UdonSharp;
    using UnityEngine;
    using UnityEngine.UI;
    using VRC.SDKBase;
    using VRC.Udon;

    public class UiToggleListener : UdonSharpBehaviour
    {
        private Toggle toggle;
        public UdonBehaviour target;
        public string variableName;
        public string onDisableEventName;
        public string onEnableEventName;

        void Start()
        {
            toggle = transform.GetComponent<Toggle>();
        }

        public void UpdateValue()
        {
            if(target != null)
            {
                target.SetProgramVariable(variableName, toggle.isOn);
            }
        }
        public void UpdateState()
        {
            if(target == null) return;
            target.SendCustomEvent(toggle.isOn ? onEnableEventName : onDisableEventName);
        }
    }
}