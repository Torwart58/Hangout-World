
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Thry.SAO.Button
{
    public class MethodCall : UdonSharpBehaviour
    {
        [Header("Script to call method on")]
        public UdonBehaviour targetScrpt;
        [Header("Method name")]
        public string methodName;
        [Header("Parameters(optional)")]
        public string[] parameterNames;
        public string[] parameters;

        public void clicked()
        {
            if (targetScrpt == null) return;
            for (int i = 0; i < parameters.Length; i++)
            {
                //value
                string pString = parameters[i];
                if (pString == "<name>")
                    pString = this.gameObject.name;
                //name
                string pName = methodName + "_param" + i;
                if (i < parameterNames.Length)
                    pName = parameterNames[i];
                targetScrpt.SetProgramVariable(pName, pString);
            }
            targetScrpt.SendCustomEvent(methodName);
        }
    }
}