using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Thry.SAO.Button
{
    public class Toggle : UdonSharpBehaviour
    {
        public UdonBehaviour targetScrpt;
        public string valueName;
        public bool setValueOnlyByMethod;
        public string methodName;
        public string paramName;

        private UnityEngine.UI.Toggle toggle;

        private void Start()
        {
            toggle = GetComponent<UnityEngine.UI.Toggle>();
            if (valueName == "")
                Debug.LogWarning("[Thry][Toggle] Toggle " + name + " does not have value specified.");
        }

        private void OnEnable()
        {
            toggle = GetComponent<UnityEngine.UI.Toggle>();
            toggle.isOn = (bool)targetScrpt.GetProgramVariable(valueName);
        }

        public void clicked()
        {
            toggle.isOn = !toggle.isOn;
            clickedDesktop();
        }

        public void clickedDesktop()
        {
            if (valueName != "" && !setValueOnlyByMethod)
                targetScrpt.SetProgramVariable(valueName, toggle.isOn);
            if (methodName != "")
            {
                if (paramName != "")
                    targetScrpt.SetProgramVariable(paramName, toggle.isOn);
                targetScrpt.SendCustomEvent(methodName);
            }
        }
    }
}