
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Thry.SAO
{
    public class Gestures : UdonSharpBehaviour
    {
        [Tooltip("SAO Menu script reference")]
        public Menu menu;
        [HideInInspector]
        public float local_player_height = 1;

        [Header("Optional")]
        [Tooltip("Debug Text Object. Used for development.")]
        public UnityEngine.UI.Text debugText;

        const float SLOW_UPDATE_RATE = 0.5f;
        private float last_slow_update = 0;

        private void Update()
        {
            UpdateGestureControl();

            if (Time.time - last_slow_update > SLOW_UPDATE_RATE)
            {
                last_slow_update = Time.time;
                SlowUpdate();
            }
        }

        private void SlowUpdate()
        {
            UpdateLocalPlayerHeight();
        }

        private void UpdateLocalPlayerHeight()
        {
            local_player_height = GetLocalAvatarHeight();
        }
        public float GetAvatarHeight(VRCPlayerApi player)
        {
            float height = 0;
            Vector3 postition1 = player.GetBonePosition(HumanBodyBones.Head);
            Vector3 postition2 = player.GetBonePosition(HumanBodyBones.Neck);
            height += (postition1 - postition2).magnitude;
            postition1 = postition2;
            postition2 = player.GetBonePosition(HumanBodyBones.Hips);
            height += (postition1 - postition2).magnitude;
            postition1 = postition2;
            postition2 = player.GetBonePosition(HumanBodyBones.RightLowerLeg);
            height += (postition1 - postition2).magnitude;
            postition1 = postition2;
            postition2 = player.GetBonePosition(HumanBodyBones.RightFoot);
            height += (postition1 - postition2).magnitude;
            return height;
        }
        private float GetLocalAvatarHeight()
        {
            if (Networking.LocalPlayer == null)
                return 1;
            return GetAvatarHeight(Networking.LocalPlayer);
        }

        //-----------Gesture tracking---------------

        private const string OCULUS_INDEX_TRIGGER_R = "Oculus_CrossPlatform_SecondaryIndexTrigger";
        private const string OCULUS_INDEX_TRIGGER_L = "Oculus_CrossPlatform_PrimaryIndexTrigger";
        private const string OCULUS_HAND_TRIGGER_R = "Oculus_CrossPlatform_SecondaryHandTrigger";
        private const string OCULUS_HAND_TRIGGER_L = "Oculus_CrossPlatform_PrimaryHandTrigger";
        private const string OCULUS_THUMBSTICK_VERTICAL_R = "Oculus_CrossPlatform_SecondaryThumbstickVertical";
        private const string OCULUS_THUMBSTICK_VERTICAL_L = "Oculus_CrossPlatform_PrimaryThumbstickVertical";
        private const string OCULUS_THUMBSTICK_HORIZONTAL_R = "Oculus_CrossPlatform_SecondaryThumbstickHorizontal";
        private const string OCULUS_THUMBSTICK_HORIZONTAL_L = "Oculus_CrossPlatform_PrimaryThumbstickHorizontal";

        const float GESTURE_REQUIRED_BUTTON_FORCE = 0.9f;
        const float GESTURE_INDEX_REQUIRED_BUTTON_FORCE = 0.7f;
        const float GESTURE_NO_PRESS_MAX_BUTTON_FORCE = 0.9f;
        const float GESTURE_VIVE_Y_MIN = 0.7f;

        const int GESTURE_FINGER_POINT = 0;
        const int GESTURE_HAND_OPEN = 1;

        public float MAX_ANGLE = 30f;

        public float REQUIRED_OPENING_DISTANCE = 0.2f;
        public float REQUIRED_CLOSING_DISTANCE = 0.2f;

        private Vector3 rightIndexFingerPosition;
        private Vector3 prev_RightIndexFingerPosition;
        private Vector3 start_RightIndexFingerPosition;
        private Vector3 leftIndexFingerPosition;
        private Vector3 prev_LeftIndexFingerPosition;
        private Vector3 start_LeftIndexFingerPosition;
        private VRCPlayerApi.TrackingData rightHand;
        private VRCPlayerApi.TrackingData prev_rightHand;
        private VRCPlayerApi.TrackingData leftHand;
        private VRCPlayerApi.TrackingData prev_leftHand;
        private VRCPlayerApi.TrackingData head;

        private float[] movementDistance;
        private bool isPlayerMoving = false;

        private Vector3 leftMovement = Vector3.zero;
        private Vector3 rightMovement = Vector3.zero;

        private void UpdateGestureControl()
        {
            if (VRC.SDKBase.Networking.LocalPlayer == null)
                return;
            if (VRC.SDKBase.Networking.LocalPlayer.IsUserInVR())
            {
                PopulateFields();
                if (isPlayerMoving == false)
                {
                    if (DidOpenGestureLeft())
                        menu.OpenMenu(false);
                    else if (DidOpenGestureRight())
                        menu.OpenMenu(true);
                    if (DidCloseGesture())
                        menu.CloseMenu();
                }
                else
                {
                    menu.CloseMenu();
                }
                prev_RightIndexFingerPosition = rightIndexFingerPosition;
                prev_LeftIndexFingerPosition = leftIndexFingerPosition;
                prev_rightHand = rightHand;
                prev_leftHand = leftHand;
            }
            else
            {
                if (UnityEngine.Input.GetKeyDown("m"))
                {
                    menu.OpenMenu(false);
                    menu.CloseMenu();
                }
            }
        }

        private void PopulateFields()
        {
            rightHand = VRC.SDKBase.Networking.LocalPlayer.GetTrackingData(VRC.SDKBase.VRCPlayerApi.TrackingDataType.RightHand);
            leftHand = VRC.SDKBase.Networking.LocalPlayer.GetTrackingData(VRC.SDKBase.VRCPlayerApi.TrackingDataType.LeftHand);
            head = VRC.SDKBase.Networking.LocalPlayer.GetTrackingData(VRC.SDKBase.VRCPlayerApi.TrackingDataType.Head);
            rightIndexFingerPosition = GetMostPreciseRightIndexFingerPosition();
            leftIndexFingerPosition = GetMostPreciseLeftIndexFingerPosition();
            isPlayerMoving = IsPlayerMoving();

            leftMovement = leftHand.position - prev_leftHand.position;
            rightMovement = rightHand.position - prev_rightHand.position;
        }

        private bool IsPlayerMoving()
        {
            return Networking.LocalPlayer.GetVelocity().magnitude > 0;
        }

        private float openMovementLeft = 0;
        private float openMovementRight = 0;
        private bool DidOpenGestureLeft()
        {
            float angle = Vector3.Angle(Vector3.down, leftMovement);
            if (debugText != null)
            {
                string d = "";
                d += "Angle: " + angle + " < " + MAX_ANGLE + "\n";
                d += "Fingerpoint: " + IsDoingGestureFingerpoint(false) + "\n";
                d += "Movement: " + openMovementLeft + " > " + (REQUIRED_OPENING_DISTANCE * local_player_height) + "\n";
                d += "Strength: " + UnityEngine.Input.GetAxis(OCULUS_HAND_TRIGGER_L);
                debugText.text = d;
            }
            if (angle < MAX_ANGLE && IsDoingGestureFingerpoint(false))
            {
                openMovementLeft += leftMovement.magnitude;
                if (openMovementLeft > REQUIRED_OPENING_DISTANCE * local_player_height)
                {
                    openMovementLeft = 0;
                    return true;
                }
            }
            else
            {
                openMovementLeft = 0;
            }
            return false;
        }
        private bool DidOpenGestureRight()
        {
            float angle = Vector3.Angle(Vector3.down, rightMovement);
            if (angle < MAX_ANGLE && IsDoingGestureFingerpoint(true))
            {
                openMovementRight += rightMovement.magnitude;
                if (openMovementRight > REQUIRED_OPENING_DISTANCE * local_player_height)
                {
                    openMovementRight = 0;
                    return true;
                }
            }
            else
            {
                openMovementRight = 0;
            }
            return false;
        }

        private bool DidCloseGesture()
        {
            return DidCloseGestureLeft() | DidCloseGestureRight();
        }
        private float closeMovementLeft = 0;
        private float closeMovementRight = 0;
        private bool DidCloseGestureLeft()
        {
            float angle = Vector3.Angle(Vector3.up, leftMovement);
            if (angle < MAX_ANGLE && IsDoingGestureOpenHand(false))
            {
                closeMovementLeft += leftMovement.magnitude;
                if (closeMovementLeft > REQUIRED_CLOSING_DISTANCE * local_player_height)
                {
                    closeMovementLeft = 0;
                    return true;
                }
            }
            else
            {
                closeMovementLeft = 0;
            }
            return false;
        }
        private bool DidCloseGestureRight()
        {
            float angle = Vector3.Angle(Vector3.up, rightMovement);
            if (angle < MAX_ANGLE && IsDoingGestureOpenHand(true))
            {
                closeMovementRight += rightMovement.magnitude;
                if (closeMovementRight > REQUIRED_CLOSING_DISTANCE * local_player_height)
                {
                    closeMovementRight = 0;
                    return true;
                }
            }
            else
            {
                closeMovementRight = 0;
            }
            return false;
        }

        private bool IsDoingGestureFingerpoint(bool rightHand)
        {
            if (rightHand)
                return IsDoingGestureFingerpointConcrete(OCULUS_HAND_TRIGGER_R, OCULUS_INDEX_TRIGGER_R, OCULUS_THUMBSTICK_VERTICAL_R, OCULUS_THUMBSTICK_HORIZONTAL_R);
            else
                return IsDoingGestureFingerpointConcrete(OCULUS_HAND_TRIGGER_L, OCULUS_INDEX_TRIGGER_R, OCULUS_THUMBSTICK_VERTICAL_L, OCULUS_THUMBSTICK_HORIZONTAL_L);
        }
        private bool IsDoingGestureFingerpointConcrete(string oculusHandTrigger, string oculusIndexTrigger, string oculusThumbstickVertical, string oculusThumbstickHorionzal)
        {
            VRCInputMethod inputMethod = VRC.SDKBase.InputManager.GetLastUsedInputMethod();
            if (inputMethod == VRCInputMethod.Oculus)
                return UnityEngine.Input.GetAxis(oculusHandTrigger) > GESTURE_REQUIRED_BUTTON_FORCE && UnityEngine.Input.GetAxis(oculusIndexTrigger) < GESTURE_NO_PRESS_MAX_BUTTON_FORCE;
            else if (inputMethod == VRCInputMethod.Vive)
                return UnityEngine.Input.GetAxis(oculusThumbstickVertical) > GESTURE_VIVE_Y_MIN && UnityEngine.Input.GetAxis(oculusThumbstickHorionzal) > -0.35 && UnityEngine.Input.GetAxis(oculusThumbstickHorionzal) < 0.35;
            else if ((int)inputMethod == 10)
                return UnityEngine.Input.GetAxis(oculusHandTrigger) > GESTURE_INDEX_REQUIRED_BUTTON_FORCE && UnityEngine.Input.GetAxis(oculusIndexTrigger) < GESTURE_NO_PRESS_MAX_BUTTON_FORCE;
            return true;
        }

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

        private bool IsLookingAtHand(VRCPlayerApi.TrackingData hand)
        {
            Quaternion rotation = Quaternion.LookRotation(hand.position - head.position, Vector3.up);
            Quaternion headrot = Quaternion.LookRotation(head.rotation * Vector3.forward, Vector3.up);
            Quaternion difference = headrot * Quaternion.Inverse(rotation);
            bool result = (difference.eulerAngles.x > 320 || difference.eulerAngles.x < 40) && (difference.eulerAngles.y > 325 || difference.eulerAngles.y < 35);
            return result;
        }

        private Vector3 GetRightFingerPostion(int gesture)
        {
            switch (gesture)
            {
                case GESTURE_FINGER_POINT:
                    return rightIndexFingerPosition;
            }
            return rightHand.position;
        }

        private Vector3 GetPrevRightFingerPostion(int gesture)
        {
            switch (gesture)
            {
                case GESTURE_FINGER_POINT:
                    return prev_RightIndexFingerPosition;
            }
            return prev_rightHand.position;
        }

        private Vector3 GetLeftFingerPostion(int gesture)
        {
            switch (gesture)
            {
                case GESTURE_FINGER_POINT:
                    return leftIndexFingerPosition;
            }
            return leftHand.position;
        }

        private Vector3 GetPrevLeftFingerPostion(int gesture)
        {
            switch (gesture)
            {
                case GESTURE_FINGER_POINT:
                    return prev_LeftIndexFingerPosition;
            }
            return prev_leftHand.position;
        }

        private Vector3 GetMostPreciseLeftIndexFingerPosition()
        {
            return GetMostPreciseFingerPosition(leftHand, HumanBodyBones.LeftIndexDistal, HumanBodyBones.LeftIndexIntermediate, HumanBodyBones.LeftIndexProximal);
        }

        private Vector3 GetMostPreciseRightIndexFingerPosition()
        {
            return GetMostPreciseFingerPosition(rightHand, HumanBodyBones.RightIndexDistal, HumanBodyBones.RightIndexIntermediate, HumanBodyBones.RightIndexProximal);
        }

        private Vector3 GetMostPreciseFingerPosition(VRCPlayerApi.TrackingData trackingData, HumanBodyBones bone1, HumanBodyBones bone2, HumanBodyBones bone3)
        {
            Vector3 position = VRC.SDKBase.Networking.LocalPlayer.GetBonePosition(bone1);
            if (position != Vector3.zero)
                return position;
            else
                position = VRC.SDKBase.Networking.LocalPlayer.GetBonePosition(bone2);
            if (position != Vector3.zero)
                return position;
            else
                position = VRC.SDKBase.Networking.LocalPlayer.GetBonePosition(bone3);
            if (position != Vector3.zero)
                return position;
            return trackingData.position;
        }
    }
}