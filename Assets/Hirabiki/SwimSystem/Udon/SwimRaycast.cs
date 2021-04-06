namespace Hirabiki.Udon.Works
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.Udon;

    public class SwimRaycast : UdonSharpBehaviour
    {
        /// <summary>
        /// Water body detection and external physics (depth detection)
        /// </summary>
        [Tooltip("Script updates every this many frames")]
        [SerializeField] private int updateInterval = 2;
        private int updateDelay;

        [Tooltip("Enable or disable swimming?")]
        public bool swimmingEnabled = true;

        //[Header("Settings")]
        //[Tooltip("[DEPRECATED: Always enable]\nShould avatar automatically tread water when not facing down?")]
        [System.NonSerialized] public bool autoTreadWater = true;

        [Header("Raycast collision layer settings")]
        [Tooltip("Layer for the surface of water")]
        [SerializeField] private int waterSurfaceLayer = 23;
        [Tooltip("Layer for the volume of water\nor the bottom of water body")]
        [SerializeField] private int waterBottomLayer = 24;
        [Header("Do not use layer 5, 9, 10, 12")]

        [Header("Underwater Post Processing")]
        [Tooltip(@"Post Processing when underwater (optional)
Leave blank for non-global Post Process Volume or if you have your own solution to underwater Post Process")]
        public GameObject postProcessTarget;

        private VRCPlayerApi you;
        [System.NonSerialized] public Vector3 surfacePos;

        void Start()
        {
            you = Networking.LocalPlayer;
        }

        void Update()
        {
            // Every tick
            if(--updateDelay > 0) return;
            // Every nth ticks
            updateDelay = updateInterval;
            surfacePos.y = -1048576f;

            if(!swimmingEnabled) return;
            if(you == null)
            {
                you = Networking.LocalPlayer;
                return;
            }

            // push the check a bit above the floor - player is 0.005 below collider
            Vector3 playerPos = you.GetPosition() + Vector3.up * 0.01f;
            // If water surface layer hit, that's where the surface of water is (it is the top of water volume)
            // If water bottom  layer hit, discard it and move on (it is the bottom of water volume)

            RaycastHit hit; // Can't inline with UdonSharp yet
            if(Physics.Raycast(playerPos, Vector3.up, out hit, 1048576f, 1 << waterSurfaceLayer | 1 << waterBottomLayer))
            {
                //Do not use Player, Playerlocal, or UI, as it's used by player, which VRC nulls it out!

                if(hit.transform.gameObject.layer == waterSurfaceLayer)
                {
                    surfacePos = hit.point;
                }
            }

            if(postProcessTarget != null)
            {
                bool test = you.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position.y < surfacePos.y;
                if(test != postProcessTarget.activeSelf)
                {
                    postProcessTarget.SetActive(test);
                }
            }
        }

        public float GetRatioOffWater()
        {
            if(you == null) return 0f;
            bool isGeneric = you.GetBonePosition(HumanBodyBones.Spine).Equals(Vector3.zero);

            Vector3 rearPos = isGeneric
                ? Vector3.Lerp(you.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position, you.GetPosition(), 0.3f)
                : Vector3.Lerp(you.GetBonePosition(HumanBodyBones.LeftUpperLeg), you.GetBonePosition(HumanBodyBones.RightUpperLeg), 0.5f);
            float headTop = GetTopOfHeadPos().y;
            float row = Mathf.Clamp01(Mathf.InverseLerp(headTop, rearPos.y, surfacePos.y));
            if(autoTreadWater)
            {
                return row * row;
            } else
            {
                float headBottom = isGeneric
                    ? Vector3.Lerp(you.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position, you.GetPosition(), 0.820f).y
                    : you.GetBonePosition(HumanBodyBones.Neck).y;
                return row * row * Mathf.Clamp01(Mathf.InverseLerp(headTop, headBottom, surfacePos.y));
            }
        }

        // FIXME: Refactor this duplicated calculation
        public Vector3 GetNosePos()
        {
            if(you == null) return Vector3.zero;
            bool isGenericRig = you.GetBonePosition(HumanBodyBones.Spine).Equals(Vector3.zero);
            bool isNoEyeBones = you.GetBonePosition(HumanBodyBones.LeftEye).Equals(Vector3.zero);

            VRCPlayerApi.TrackingData eyeTransform = you.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            Vector3 headBonePos = isGenericRig ? Vector3.Lerp(you.GetPosition(), eyeTransform.position, 0.820f) : you.GetBonePosition(HumanBodyBones.Head);
            Vector3 eyeBonesPos = isNoEyeBones ? eyeTransform.position : Vector3.Lerp(you.GetBonePosition(HumanBodyBones.LeftEye), you.GetBonePosition(HumanBodyBones.RightEye), 0.5f);

            Vector3 eyePos = (headBonePos - eyeBonesPos).magnitude < 0.001f ? eyeTransform.position : eyeBonesPos;
            // An attempt to estimate the position of the mouth/nose
            float headBoneToEyeLength = (eyePos - headBonePos).magnitude;


            Vector3 nosePos = Vector3.Lerp(eyePos, headBonePos, 0.333333333f) +
                (eyeTransform.rotation * Vector3.forward * headBoneToEyeLength * 0.666666666f);

            return nosePos;
        }
        public Vector3 GetTopOfHeadPos()
        {
            if(you == null) return Vector3.zero;
            bool isGenericRig = you.GetBonePosition(HumanBodyBones.Spine).Equals(Vector3.zero);
            bool isNoEyeBones = you.GetBonePosition(HumanBodyBones.LeftEye).Equals(Vector3.zero);

            VRCPlayerApi.TrackingData eyeTransform = you.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            Vector3 headBonePos = isGenericRig ? Vector3.Lerp(you.GetPosition(), eyeTransform.position, 0.820f) : you.GetBonePosition(HumanBodyBones.Head);
            Vector3 eyeBonesPos = isNoEyeBones ? eyeTransform.position : Vector3.Lerp(you.GetBonePosition(HumanBodyBones.LeftEye), you.GetBonePosition(HumanBodyBones.RightEye), 0.5f);

            Vector3 eyePos = (headBonePos - eyeBonesPos).magnitude < 0.001f ? eyeTransform.position : eyeBonesPos;
            // An attempt to estimate the position of the mouth/nose
            float headBoneToEyeLength = (eyePos - headBonePos).magnitude;


            // An attempt to estimate the top of the head
            Vector3 aboveEyePos = eyePos + Vector3.up * headBoneToEyeLength * 2f;
            if(autoTreadWater)
            {
                // Artificial lowering for automatic floating
                aboveEyePos.y -= Mathf.Min((eyeTransform.rotation * Vector3.forward * headBoneToEyeLength * 2.1f).y, 0f) + headBoneToEyeLength * 1.5f;
            }

            return aboveEyePos;
        }
    }
}