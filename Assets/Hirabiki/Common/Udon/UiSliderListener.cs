namespace Hirabiki.Udon.Works
{
    using UdonSharp;
    using UnityEngine;
    using UnityEngine.UI;
    using VRC.SDKBase;
    using VRC.Udon;

    public class UiSliderListener : UdonSharpBehaviour
    {
        private Slider slider;
        private Text valueText;
        public string stringFormat = "";
        public UdonBehaviour target;
        public string variableName;

        void Start()
        {
            slider = transform.GetComponent<Slider>();
            Transform tryFind = transform.Find("[ValueText]");
            if(tryFind != null)
            {
                valueText = tryFind.GetComponent<Text>();
            }

            if(target != null)
            {
                float readValue = (float)target.GetProgramVariable(variableName);
                if(slider.value == readValue)
                {
                    UpdateValue();
                } else
                {
                    slider.value = readValue;
                }
            } else
            {
                UpdateValue();
            }
        }

        public void UpdateValue()
        {
            if(valueText != null)
            {
                valueText.text = slider.value.ToString(stringFormat);
            }
            if(target != null)
            {
                target.SetProgramVariable(variableName, slider.value);
            }
        }
    }
}