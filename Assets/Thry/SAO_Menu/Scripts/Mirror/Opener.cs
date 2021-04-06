
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Thry.Mirror
{
    public class Opener : UdonSharpBehaviour
    {
        public Animator mirror;

        public Transform[] mirrorIndicators;

        private Transform[] available_positions;

        private Transform activeTransform;

        private GameObject[] sprite_objects;

        private void Start()
        {
            sprite_objects = new GameObject[mirrorIndicators.Length];
            available_positions = new Transform[mirrorIndicators.Length];
            for (int i = 0; i < mirrorIndicators.Length; i++)
            {
                sprite_objects[i] = mirrorIndicators[i].GetChild(1).gameObject;
                available_positions[i] = mirrorIndicators[i].GetChild(0);
            }
        }

        public void ToggleSprites(bool on)
        {
            foreach (GameObject o in sprite_objects)
                o.SetActive(on);
        }

        public void ToggleMirror()
        {
            Transform lookedAt = TransformLookedAt();
            if ((lookedAt != null && lookedAt == activeTransform) || (lookedAt == null && activeTransform != null))
            {
                CloseMirror();
            }
            else if (lookedAt != null)
            {
                OpenMirror(lookedAt);
            }
        }

        private Transform TransformLookedAt()
        {
            float closestTransform = float.MaxValue;
            Transform selected = null;
            VRCPlayerApi.TrackingData trackingData = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

            Vector3 lookDirection = (trackingData.rotation * Vector3.forward).normalized;
            Ray lookRay = new Ray(trackingData.position, lookDirection);
            foreach (Transform t in available_positions)
            {
                Plane mirrorPlane = new Plane(t.rotation * Vector3.back, t.position);
                float intersectionDistance;
                if (mirrorPlane.Raycast(lookRay, out intersectionDistance) == false)
                    continue;

                Vector3 intersection = trackingData.position + lookDirection * intersectionDistance;

                Vector3 toIntersectionVector = intersection - t.position;
                toIntersectionVector = Quaternion.Inverse(t.rotation) * toIntersectionVector;

                if (Mathf.Abs(toIntersectionVector.x) > t.localScale.x / 2 || Mathf.Abs(toIntersectionVector.y) > t.localScale.y / 2)
                    continue;

                if (intersectionDistance < closestTransform)
                {
                    closestTransform = intersectionDistance;
                    selected = t;
                }
            }
            return selected;
        }

        public void CloseMirror()
        {
            mirror.ResetTrigger("off");
            mirror.SetTrigger("off");
            activeTransform = null;
        }

        public void OpenMirror(Transform t)
        {
            activeTransform = t;
            mirror.transform.SetPositionAndRotation(t.position, t.rotation);
            mirror.transform.localScale = t.localScale;

            mirror.ResetTrigger("on");
            mirror.SetTrigger("on");
        }

        public void Test0()
        {
            mirror.ResetTrigger("on");
            mirror.SetTrigger("on");
        }
    }
}