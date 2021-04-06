
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Thry.SAO
{
    public class IndexFingerTrigger : UdonSharpBehaviour
    {
        const string CONTENT_POSTFIX = "_Content";

        [Header("Hand Side")]
        public bool isRightHand;

        [Header("Needed References")]
        public Menu menuManager;
        public Gestures gestureScript;

        private Vector3 currentPosition;
        private Vector3 lastPosition;

        [Header("Settings")]
        public float pressTimeConstraint = 0.5f;
        public float scrollSpeed = 4;

        private Collider[] currentColliders = new Collider[5];
        private float[] currentCollidersEnterTime = new float[5];
        public void OnTriggerEnter(Collider other)
        {
            if (other == null)
                return;
            Debug.Log("[Thry] [Index Fingy] Collision enter: " + other.name);
            float earlierstEnter = float.MaxValue;
            int earliestIndex = 0;
            int foundIndex = -1;
            for (int i = 0; i < currentColliders.Length; i++)
            {
                if (currentColliders[i] == null)
                {
                    foundIndex = i;
                    break;
                }
                if (currentCollidersEnterTime[i] < earlierstEnter)
                {
                    earlierstEnter = currentCollidersEnterTime[i];
                    earliestIndex = i;
                }
            }
            if (foundIndex == -1)
                foundIndex = earliestIndex;
            currentColliders[foundIndex] = other;
            currentCollidersEnterTime[foundIndex] = Time.time;
        }

        public void OnTriggerExit(Collider other)
        {
            if (other == null)
                return;
            int index = -1;
            for (int i = 0; i < currentColliders.Length; i++)
                if (currentColliders[i] == other)
                    index = i;
            if (index == -1)
                return;
            Debug.Log("[Thry] [Index Fingy] Collision exit: " + other.name + " enter time: " + (Time.time - currentCollidersEnterTime[index]));
            if (Time.time - currentCollidersEnterTime[index] < pressTimeConstraint)
            {
                Press(other);
            }
            currentColliders[index] = null;
        }

        private void Press(Collider other)
        {
            Transform parent = other.transform.parent;
            bool isPartOfMenu = false;
            while (parent != null)
            {
                if (parent == transform.parent)
                    isPartOfMenu = true;
                parent = parent.parent;
            }
            bool blockInput = IsDoingGestureOpenHand(isRightHand) && !menuManager.TEST_VR && VRC.SDKBase.Networking.LocalPlayer != null;
            if (isPartOfMenu && !blockInput && IsBackwardMoving(other))
            {
                UdonBehaviour actionScript = (UdonBehaviour)other.gameObject.GetComponent(typeof(UdonBehaviour));
                UnityEngine.UI.Button button = (UnityEngine.UI.Button)other.gameObject.GetComponent(typeof(UnityEngine.UI.Button));
                if (button != null)
                {
                    menuManager.ButtonClicked(button);
                }
                if (actionScript != null)
                {
                    Vector3 intersect = transform.position + other.transform.forward.normalized * PlanePointDistance(other.transform.forward, other.transform.position, transform.position);
                    actionScript.SetProgramVariable("clickedPosition", intersect);
                    actionScript.SendCustomEvent("clicked");
                }
            }
        }

        private void HandleDrag()
        {
            Vector3 delta = currentPosition - lastPosition;
            for (int i = 0; i < currentColliders.Length; i++)
            {
                if (currentColliders[i] != null && Time.time - currentCollidersEnterTime[i] > pressTimeConstraint)
                {
                    Drag(currentColliders[i], delta);
                }
            }
        }

        private void Drag(Collider other, Vector3 delta)
        {
            UnityEngine.UI.ScrollRect scrollRect = (UnityEngine.UI.ScrollRect)other.GetComponent(typeof(UnityEngine.UI.ScrollRect));
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition += -delta.y * gestureScript.local_player_height * scrollSpeed;
            }
        }

        private bool IsForwardMoving(Collider other)
        {
            Vector3 movementVec = (currentPosition - lastPosition).normalized;
            float dot = Vector3.Dot(movementVec, other.transform.forward);
            return dot > 0.5;
        }

        private bool IsBackwardMoving(Collider other)
        {
            Vector3 movementVec = (currentPosition - lastPosition).normalized;
            float dot = Vector3.Dot(movementVec, other.transform.forward);
            return dot < -0.5;
        }

        private float PlanePointDistance(Vector3 n, Vector3 p0, Vector3 p)
        {
            float a = n.x;
            float b = n.y;
            float c = n.z;
            float d = -(a * p0.x) - (b * p0.y) - (c * p0.z);
            return Mathf.Abs(a * p.x + b * p.y + c * p.z + d) / n.magnitude;
        }

        const float GESTURE_REQUIRED_BUTTON_FORCE = 0.9f;
        const float GESTURE_NO_PRESS_MAX_BUTTON_FORCE = 0.9f;

        private const string OCULUS_INDEX_TRIGGER_R = "Oculus_CrossPlatform_SecondaryIndexTrigger";
        private const string OCULUS_INDEX_TRIGGER_L = "Oculus_CrossPlatform_PrimaryIndexTrigger";
        private const string OCULUS_HAND_TRIGGER_R = "Oculus_CrossPlatform_SecondaryHandTrigger";
        private const string OCULUS_HAND_TRIGGER_L = "Oculus_CrossPlatform_PrimaryHandTrigger";
        private const string OCULUS_THUMBSTICK_VERTICAL_R = "Oculus_CrossPlatform_SecondaryThumbstickVertical";
        private const string OCULUS_THUMBSTICK_VERTICAL_L = "Oculus_CrossPlatform_PrimaryThumbstickVertical";
        private const string OCULUS_THUMBSTICK_HORIZONTAL_R = "Oculus_CrossPlatform_SecondaryThumbstickHorizontal";
        private const string OCULUS_THUMBSTICK_HORIZONTAL_L = "Oculus_CrossPlatform_PrimaryThumbstickHorizontal";
        private bool IsDoingGestureOpenHand(bool rightHand)
        {
            if (rightHand)
                return IsDoingGestureOpenHandConcrete(OCULUS_HAND_TRIGGER_R, OCULUS_INDEX_TRIGGER_R);
            else
                return IsDoingGestureOpenHandConcrete(OCULUS_HAND_TRIGGER_L, OCULUS_INDEX_TRIGGER_R);
        }
        private bool IsDoingGestureOpenHandConcrete(string oculusHandTrigger, string oculusIndexTrigger)
        {
            VRCInputMethod inputMethod = VRC.SDKBase.InputManager.GetLastUsedInputMethod();
            if (inputMethod == VRCInputMethod.Oculus)
                return UnityEngine.Input.GetAxis(oculusHandTrigger) < GESTURE_NO_PRESS_MAX_BUTTON_FORCE && UnityEngine.Input.GetAxis(oculusIndexTrigger) < GESTURE_NO_PRESS_MAX_BUTTON_FORCE;
            else if (inputMethod == VRCInputMethod.Vive)
                return UnityEngine.Input.GetAxis(oculusHandTrigger) > GESTURE_REQUIRED_BUTTON_FORCE;
            else if ((int)inputMethod == 10)
                return UnityEngine.Input.GetAxis(oculusHandTrigger) < GESTURE_NO_PRESS_MAX_BUTTON_FORCE && UnityEngine.Input.GetAxis(oculusIndexTrigger) < GESTURE_NO_PRESS_MAX_BUTTON_FORCE;
            return true;
        }

        private void Update()
        {
            if (!menuManager.TEST_VR)
            {
                if (VRC.SDKBase.Networking.LocalPlayer == null || Networking.LocalPlayer.IsUserInVR() == false)
                    return;
                if (isRightHand)
                    SetToMostPreciseRightIndexFingerPosition(this.gameObject);
                else
                    SetToMostPreciseLeftIndexFingerPosition(this.gameObject);
            }
            HandleDrag();
            lastPosition = currentPosition;
            currentPosition = this.transform.position;
        }

        private void SetToMostPreciseLeftIndexFingerPosition(GameObject target)
        {
            SetToMostPreciseFingerPosition(target, Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand), HumanBodyBones.LeftIndexDistal, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexProximal);
        }

        private void SetToMostPreciseRightIndexFingerPosition(GameObject target)
        {
            SetToMostPreciseFingerPosition(target, Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand), HumanBodyBones.RightIndexDistal, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexProximal);
        }

        private void SetToMostPreciseFingerPosition(GameObject target, VRCPlayerApi.TrackingData trackingData, HumanBodyBones bone1, HumanBodyBones bone2, HumanBodyBones bone3)
        {
            Vector3 position = VRC.SDKBase.Networking.LocalPlayer.GetBonePosition(bone1);
            if (position != Vector3.zero)
            {
                target.transform.position = position;
                target.transform.rotation = VRC.SDKBase.Networking.LocalPlayer.GetBoneRotation(bone1);
                return;
            }
            else
                position = VRC.SDKBase.Networking.LocalPlayer.GetBonePosition(bone2);
            if (position != Vector3.zero)
            {
                target.transform.position = position;
                target.transform.rotation = VRC.SDKBase.Networking.LocalPlayer.GetBoneRotation(bone1);
                return;
            }
            else
                position = VRC.SDKBase.Networking.LocalPlayer.GetBonePosition(bone3);
            if (position != Vector3.zero)
            {
                target.transform.position = position;
                target.transform.rotation = VRC.SDKBase.Networking.LocalPlayer.GetBoneRotation(bone1);
                return;
            }
            target.transform.position = trackingData.position;
            target.transform.rotation = trackingData.rotation;
        }
    }
}