
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Thry.SAO
{
    public class Slider : UdonSharpBehaviour
    {

        public Vector3 clickedPosition;

        public void clicked()
        {
            float width = transform.lossyScale.x * ((BoxCollider)gameObject.GetComponent(typeof(BoxCollider))).size.x;

            Vector3 relativeClickedPosition = transform.position - clickedPosition;
            float direction = Vector3.Dot(relativeClickedPosition, transform.right);
            direction = direction / Mathf.Abs(direction) * -1;
            float distance = relativeClickedPosition.magnitude * direction;

            float absoluteDistance = distance + width / 2;

            float relativeValue = absoluteDistance / width;

            UnityEngine.UI.Slider slider = ((UnityEngine.UI.Slider)gameObject.GetComponent(typeof(UnityEngine.UI.Slider)));
            float value = slider.minValue + relativeValue * slider.maxValue;

            slider.value = value;
        }

    }
}