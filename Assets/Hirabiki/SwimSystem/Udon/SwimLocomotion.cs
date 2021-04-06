namespace Hirabiki.Udon.Works
{
    using UdonSharp;
    using UnityEngine;
    using UnityEngine.UI;
    using VRC.SDKBase;
    using VRC.Udon;

    /// <summary>
    /// This UdonSharpBehaviour is intended to separate out the locomotion and the abstract aspects of swim
    /// </summary>
    public class SwimLocomotion : UdonSharpBehaviour
    {
        [Tooltip("Script updates every this many frames")]
        [SerializeField] private int updateInterval = 1;
        [Header("Important configuration")]
        [Tooltip(@"Script to send event to when player exters/exits water
Assign this to Udon_BreathSystem inside BreathSystem prefab
List of Custom Events:
OnWaterEnter
OnWaterExit
OnUnderwaterEnter
OnUnderwaterExit")]
        // [HideInInspector] // Uncomment this line for SwimSystem + BreathSystem package
        [SerializeField] private UdonBehaviour breathSystemEvent;
        

        [Header("Script references")]
        [Tooltip("Script that detect position of water surface")]
        [SerializeField] private SwimRaycast swimPhysics;  // For determining surface of water

        [Tooltip("Script for updating player movement and position")]
        [SerializeField] private BasicLocomotion locomotion; // For interfacing with traditional VRC locomotion
        // HACK: UdonSharp does not support interface yet so it require changing class name
        
        [Header("Sound settings")]
        [Tooltip("AudioSource for underwater sound (loop)")]
        [SerializeField] private AudioSource underwaterSound;

        [Tooltip("AudioSource for sound effects")]
        [SerializeField] private AudioSource localOneShotSounds;

        [Tooltip("Sound effect when player jumps into water")]
        [SerializeField] private AudioClip bodySplashClip;

        [Tooltip("Sound effect when player hands splash into water (VR only)")]
        [SerializeField] private AudioClip handSplashClip;

        [Header("Locomotion settings")]

        [Tooltip(@"Should simplified hand swimming controls be used?
Disabled: Ability to swim backwards and sideways
Enabled: Trigger can be kept held when swimming")]
        public bool useSimpleHandSwim;

        [Tooltip(@"Set WASD/Thumbstick movement direction
Disabled: Head direction
Enabled: Hands direction (VR only)
Desktop always uses head direction")]
        public bool useHandsForDirection;

        [Tooltip(@"::Call DisableFootSwim to disable and
EnableSimpleFootSwim to enable from script::

Players with FBT setup can use their legs to swim.
if RealisticFullBodySwim is disabled, player can swim by simply swinging their legs.
Swimming locomotion for regular VR player with FBT swim enabled will not work correctly.
It is not possible to tell regular VR and FBT players apart so put a warning in your world.")]
        public bool useFullBodySwim;

        [Tooltip(@"::Call EnableRealFootSwim to enable from script::

UseFullBodySwim must be enabled to use this setting.
The locomotion of full-body leg swimming will be mostly realistic to real life.
This mode is for the most dedicated players with full-body setup")]
        public bool realisticFullBodySwim;

        [Header("Speed settings")]
        [Tooltip("Swimming speed with WASD keys or Thumbstick")]
        public float moveSwimSpeed = 1.5f;

        [Tooltip("Swimming speed with arms (VR only)")]
        public float handSwimSpeed = 1.5f;

        [Tooltip("Swimming speed with legs (VR only)")]
        public float fullBodyLegSwimSpeed = 1.0f;

        [Header("Physics settings")]
        [Tooltip("Movement friction in water")]
        public float waterDrag = 1.2f;

        [Tooltip("Negative buoyancy at depth")]
        public float sinkingGravity = 0.04f;

        [Tooltip("Positive buoyancy at water surface")]
        public float buoyancyGravity = 0.10f;

        [Tooltip("Rate of buoyancy falloff based on depth")]
        public float buoyancyFalloff = 0.25f;

        [Header("Advanced settings")]
        [Tooltip(@"Keep player moving along the floor without input
Unchecked: Apply small upward velocity
Checked: Teleport player slightly upward
Using teleport is more subtle, but may not be smooth from other player's view")]
        [SerializeField] private bool useTeleportTo = false;

        // Set by SwimSystem
        [System.NonSerialized] public float swimEnergy = 1.0f;
        [System.NonSerialized] public float buoyancyRatio = 1.0f;
        [Header("UGUI text debug output")]
        [SerializeField] private Text debugText;

        private VRCPlayerApi you;

        // Last positions on previous update
        private Vector3 prevHandLPos;
        private Vector3 prevHandRPos;
        private float prevFootLAngVel;
        private float prevFootRAngVel;
        // Calculated velocities
        private Vector3 handLVector;
        private Vector3 handRVector;
        private float footLVelocity;
        private float footRVelocity;

        private Vector3 prevPlayerVector;

        private bool isSwimming;
        private bool isUnderwater;

        private float diveTime;
        private float uwStartTime;
        private float uwEndTime;
        private float swimMoveTime;
        private float touchdownTime;
        private float jumpTime;
        private bool hasMenuOpen;
        private int updateDelay;
        private int emulatedSwimLockDelay;
        private int realSwimLockDelay;
        private int pushRetryDelay;

        void Start()
        {
            you = Networking.LocalPlayer;
        }

        void OnEnable()
        {
            if(isUnderwater)
            {
                if(underwaterSound != null) underwaterSound.Play();
            } else
            {
                if(underwaterSound != null) underwaterSound.Stop();
            }
        }

        void Update()
        {
            if(you == null) // Trying to ward off nasty edge-case bugs found with very small probability
            {
                you = Networking.LocalPlayer;
                return;
            }
            if(locomotion.IsOnGround() && Time.time - jumpTime > 0.2f)
            {
                touchdownTime = Time.time;
            }
            if(--updateDelay > 0) return;
            updateDelay = updateInterval;

            bool isGenericRig = you.GetBonePosition(HumanBodyBones.Spine).Equals(Vector3.zero);
            bool isNoEyeBones = you.GetBonePosition(HumanBodyBones.LeftEye).Equals(Vector3.zero);

            VRCPlayerApi.TrackingData eyeTransform = you.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            Vector3 headBonePos = isGenericRig ? Vector3.Lerp(you.GetPosition(), eyeTransform.position, 0.820f) : you.GetBonePosition(HumanBodyBones.Head);
            Vector3 eyeBonesPos = isNoEyeBones ? eyeTransform.position : Vector3.Lerp(you.GetBonePosition(HumanBodyBones.LeftEye), you.GetBonePosition(HumanBodyBones.RightEye), 0.5f);

            Vector3 eyePos = (headBonePos - eyeBonesPos).magnitude < 0.001f ? eyeTransform.position : eyeBonesPos;
            // An attempt to estimate the position of the mouth/nose
            float headBoneToEyeLength = (eyePos - headBonePos).magnitude;


            Vector3 hipsPos = isGenericRig
                ? Vector3.Lerp(you.GetPosition(), eyeTransform.position, 0.4f)
                : you.GetBonePosition(HumanBodyBones.Hips) + Vector3.down * headBoneToEyeLength * 0.5f;
            Vector3 nosePos = Vector3.Lerp(eyePos, headBonePos, 0.333333333f) +
                (eyeTransform.rotation * Vector3.forward * headBoneToEyeLength * 0.666666666f);

            bool nowSwimming = IsInWater(hipsPos);
            bool nowUnderwater = IsInWater(nosePos);
            if(nowSwimming != isSwimming && Time.time - diveTime > 0.1f)
            {
                isSwimming = nowSwimming;
                if(nowSwimming)
                {
                    if(breathSystemEvent != null) breathSystemEvent.SendCustomEvent("OnSwimEnter");
                    locomotion.Immobilize(true);

                    Vector3 pVel = locomotion.GetVelocity();
                    PlayWaterSplash(bodySplashClip, Vector3.Scale(pVel, new Vector3(0.5f, 1.0f, 0.5f)), 1.0f);
                    locomotion.SetVelocity(pVel * 0.75f);

                    diveTime = Time.time;
                } else
                {
                    if(breathSystemEvent != null) breathSystemEvent.SendCustomEvent("OnSwimExit");
                    locomotion.Immobilize(false);

                    locomotion.SetGravityStrength(1f);
                }
            }
            if(nowUnderwater != isUnderwater && Time.time - uwStartTime > 0.1f)
            {
                isUnderwater = nowUnderwater;
                if(nowUnderwater)
                {
                    if(breathSystemEvent != null) breathSystemEvent.SendCustomEvent("OnUnderwaterEnter");
                    if(underwaterSound != null) underwaterSound.Play();
                    uwStartTime = Time.time;
                } else
                {
                    if(breathSystemEvent != null) breathSystemEvent.SendCustomEvent("OnUnderwaterExit");
                    if(underwaterSound != null) underwaterSound.Stop();

                    locomotion.SetVelocity(Vector3.Scale(locomotion.GetVelocity(), new Vector3(1f, 0.5f, 1f)));
                }
            }
            if(isUnderwater) uwEndTime = Time.time;

            if(debugText != null)
            {
                Vector3 sv = GetSwimVector();
                debugText.text = $@"-- Swim Locomotion --
Buoyancy: {buoyancyGravity} ({GetBuoyancyVector().y})
Swim Energy: {swimEnergy}
Swim Input: {sv.ToString("F2")} = {sv.magnitude}
Dive Time: {uwEndTime - uwStartTime} / Depth: {GetDepth()}
P-GS: {you.GetGravityStrength()}
Surface Y pos: {swimPhysics.surfacePos.y}
isSwimming: {isSwimming} / isUnderwater: {isUnderwater}
FBT Simple: {useFullBodySwim} / FBT Real: {realisticFullBodySwim}";

            }

            // If not swimming, calculations end here
            if(!isSwimming) return;
            float deltaTimeStep = Time.deltaTime * updateInterval;

            // Fetch hand positions
            VRCPlayerApi.TrackingData handLTransform = you.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand);
            VRCPlayerApi.TrackingData handRTransform = you.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand);

            // Manual velocity calculation
            Vector3 newHandLPos = handLTransform.position - you.GetPosition();
            Vector3 newHandRPos = handRTransform.position - you.GetPosition();

            Vector3 newLegLDir = (you.GetBonePosition(HumanBodyBones.LeftUpperLeg) - you.GetBonePosition(HumanBodyBones.LeftLowerLeg)).normalized;
            Vector3 newLegRDir = (you.GetBonePosition(HumanBodyBones.RightUpperLeg) - you.GetBonePosition(HumanBodyBones.RightLowerLeg)).normalized;
            Vector3 newFootLDir = (you.GetBonePosition(HumanBodyBones.LeftLowerLeg) - you.GetBonePosition(HumanBodyBones.LeftFoot)).normalized;
            Vector3 newFootRDir = (you.GetBonePosition(HumanBodyBones.RightLowerLeg) - you.GetBonePosition(HumanBodyBones.RightFoot)).normalized;
            float newFootLAngVel = isGenericRig ? 0f : Mathf.Deg2Rad * Vector3.Angle(newLegLDir, newFootRDir);
            float newFootRAngVel = isGenericRig ? 0f : Mathf.Deg2Rad * Vector3.Angle(newLegRDir, newFootRDir);

            // Update velocities -- these are essential in GetSwimVector()
            handLVector = (newHandLPos - prevHandLPos) / deltaTimeStep;
            handRVector = (newHandRPos - prevHandRPos) / deltaTimeStep;

            footLVelocity = Mathf.Abs(prevFootLAngVel - newFootLAngVel) / deltaTimeStep;
            footRVelocity = Mathf.Abs(prevFootRAngVel - newFootRAngVel) / deltaTimeStep;

            if(you.IsUserInVR() && Time.time - diveTime > 0.1f)
            {
                if(!IsInWater(you.GetPosition() + prevHandLPos) && IsInWater(handLTransform.position))
                {
                    PlayWaterSplash(handSplashClip, handLVector, 0.5f);
                }
                if(!IsInWater(you.GetPosition() + prevHandRPos) && IsInWater(handRTransform.position))
                {
                    PlayWaterSplash(handSplashClip, handRVector, 0.5f);
                }
            }

            // Update old positions
            prevHandLPos = newHandLPos;
            prevHandRPos = newHandRPos;
            prevFootLAngVel = newFootLAngVel;
            prevFootRAngVel = newFootRAngVel;

            /*
            you.SetJumpImpulse(Mathf.Lerp(0.7f, originalJumpImpulse, outsideWaterFactor));
            you.SetWalkSpeed(Mathf.Lerp(1.5f, originalWalkSpeed, outsideWaterFactor * 2f - 1f));
            you.SetRunSpeed(Mathf.Lerp(2.0f, originalRunSpeed, outsideWaterFactor * 2f - 1f));
            */
            if(IsMenuButtonDown())
            {
                hasMenuOpen = true;
            }
            if(locomotion.GetMoveVector().sqrMagnitude > 0f)
            {
                hasMenuOpen = false;
            }

            Vector3 swimVector = GetSwimVector() * swimEnergy + GetBuoyancyVector();

            float finalGravity = Mathf.Lerp(0.000001f, 1f, swimPhysics.GetRatioOffWater());

            const float initVel = 0.1f;
            // Unstick the sticky VRC ground
            if(isSwimming && !hasMenuOpen && locomotion.IsUsingVRCLocomotion() && locomotion.IsOnGround()
                && Vector3.Scale(locomotion.GetVelocity(), new Vector3(1f, 0f, 1f)).sqrMagnitude > 0.001f)
            {
                locomotion.Immobilize(isUnderwater);

                if(useTeleportTo)
                {
                    you.TeleportTo(you.GetPosition() + Vector3.up * 0.003f, you.GetRotation(), VRC_SceneDescriptor.SpawnOrientation.Default, true);
                    locomotion.SetVelocity(Vector3.Scale(locomotion.GetVelocity(), new Vector3(1f, 0f, 1f)));
                } else
                {
                    Vector3 newVel = locomotion.GetVelocity();
                    newVel.y = Mathf.Max(0f, newVel.y);
                    locomotion.SetVelocity(newVel + new Vector3(0f, 0.1f, 0f));
                }
                locomotion.SetGravityStrength(0.000001f);
            } else
            {
                locomotion.Immobilize(isSwimming);
                locomotion.SetGravityStrength(isSwimming ? finalGravity * Mathf.Clamp01((Time.time - touchdownTime) * 2f) : finalGravity);
            }

            // Unstick initial horizontal movement
            Vector3 newVelocity;
            if(pushRetryDelay < 0
                && Vector3.Scale(locomotion.GetVelocity(), new Vector3(1f, 0f, 1f)).magnitude < initVel * 0.5f
                && Vector3.Scale(swimVector, new Vector3(1f, 0f, 1f)).magnitude > initVel)
            {
                pushRetryDelay = 2;
                newVelocity = locomotion.GetVelocity() + Vector3.Scale(Vector3.Scale(swimVector, new Vector3(1f, 0.2f, 1f)).normalized, new Vector3(initVel, 0f, initVel)) + swimVector.y * Vector3.up * deltaTimeStep;
            } else
            {
                newVelocity = locomotion.GetVelocity() + swimVector * deltaTimeStep;
            }

            // Set new velocity, with drag
            float diveDragAdd = 0f;
            if(isUnderwater)
            {
                diveDragAdd = Mathf.Clamp01(diveTime - Time.time + 1f);
                diveDragAdd = diveDragAdd * diveDragAdd * 3f;
            }

            float jumpForce = 0f;
            if(IsJumpButtonDownAndCanJump())
            {
                if(!locomotion.IsUsingVRCLocomotion() || locomotion.IsImmobile())
                {
                    jumpForce = Mathf.Max(you.GetJumpImpulse(), 1f) * (isUnderwater ? 0.5f : 1f) * swimEnergy;
                    jumpTime = Time.time;
                }
            }
            //locomotion.SetVelocity(newVelocity * Mathf.Pow(Mathf.Clamp01(waterDrag) * diveDecay, deltaTimeStep) + Vector3.up * jumpForce);
            locomotion.SetVelocity(newVelocity * (1f - deltaTimeStep * (waterDrag + diveDragAdd)) + Vector3.up * jumpForce);

            // Update locking timers
            pushRetryDelay -= 1;
            emulatedSwimLockDelay -= 1;
            realSwimLockDelay -= 1;

            prevPlayerVector = locomotion.GetVelocity();
        }

        // Public methods for SendCustomEvent
        public void DisableFootSwim()
        {
            useFullBodySwim = false;
            realisticFullBodySwim = false;
        }
        public void EnableSimpleFootSwim()
        {
            useFullBodySwim = true;
            realisticFullBodySwim = false;
        }
        public void EnableRealFootSwim()
        {
            useFullBodySwim = true;
            realisticFullBodySwim = true;
        }
        public void MuteAllSwimSounds()
        {
            if(underwaterSound != null) underwaterSound.mute = true;
            if(localOneShotSounds != null) localOneShotSounds.mute = true;
            breathSystemEvent.SendCustomEvent("MuteAllSounds");
        }
        public void UnmuteAllSwimSounds()
        {
            if(underwaterSound != null) underwaterSound.mute = false;
            if(localOneShotSounds != null) localOneShotSounds.mute = false;
            breathSystemEvent.SendCustomEvent("UnmuteAllSounds");
        }

        public bool IsSwimming()
        {
            return isSwimming;
        }

        // Input vector
        public Vector3 GetSwimVector()
        {
            // Init
            Vector3 handSwim = Vector3.zero;
            Vector3 legSwim = Vector3.zero;

            // VR hand swim locomotion
            if(you.IsUserInVR())
            {
                float inputLT = Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryIndexTrigger");
                float inputRT = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger");
                float aboveWaterLMult = Mathf.Lerp(1f, 0f, (you.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).position - swimPhysics.surfacePos).y * 5f);
                float aboveWaterRMult = Mathf.Lerp(1f, 0f, (you.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position - swimPhysics.surfacePos).y * 5f);
                float singleHandPenalty = isUnderwater ? Mathf.Lerp(0.75f, 1.0f, Mathf.Min(inputLT, inputRT)) : 1.0f;
                Vector3 inputHandLVector = handLVector * -inputLT * aboveWaterLMult * singleHandPenalty;
                Vector3 inputHandRVector = handRVector * -inputRT * aboveWaterRMult * singleHandPenalty;

                // Simplified swimming (can swim with trigger always held - bad new is: can't swim backwards)
                if(useSimpleHandSwim)
                {
                    inputHandLVector *= Quaternion.Angle(Quaternion.LookRotation(inputHandLVector.normalized, Vector3.up),
                        you.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation) < 105f ? 1.1f : 0f;
                    inputHandRVector *= Quaternion.Angle(Quaternion.LookRotation(inputHandRVector.normalized, Vector3.up),
                        you.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation) < 105f ? 1.1f : 0f;
                }
                // Separate left and right hand velocity
                Vector3 directSwimVector = inputHandLVector + inputHandRVector;


                // Combined left and right hand velocity's magnitude
                float combinedSwimMagnitude = inputHandLVector.magnitude + inputHandRVector.magnitude;
                handSwim = Vector3.Lerp(directSwimVector, directSwimVector.normalized * combinedSwimMagnitude, 0.5f) * handSwimSpeed;

                // FBT leg swim locomotion
                float legLength = Vector3.Distance(you.GetBonePosition(HumanBodyBones.LeftUpperLeg), you.GetBonePosition(HumanBodyBones.LeftLowerLeg))
                    + Vector3.Distance(you.GetBonePosition(HumanBodyBones.LeftLowerLeg), you.GetBonePosition(HumanBodyBones.LeftFoot));
                Vector3 legSwimVector = useFullBodySwim ? FullBodySwimDirection() * Vector3.forward * legLength * (footLVelocity + footRVelocity) : Vector3.zero;
                legSwim = 0.200f * fullBodyLegSwimSpeed * legSwimVector;
            }

            // Emulated swim locomotion
            Quaternion moveRot = you.IsUserInVR() && useHandsForDirection ? VRSwimDirection() : you.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            Vector3 emulatedSwim = moveRot * locomotion.GetMoveVector() * 2f +
                (Input.GetButton("Jump") ? Vector3.up : Vector3.zero);

            // Prevent using multiple swim methods at the same time
            if(handSwim.sqrMagnitude + legSwim.sqrMagnitude > 0f)
            {
                // Disable emulated swim when hands or leg move
                // Essentially locks out emulated swim input with leg swim enabled
                emulatedSwimLockDelay = 4;
            }

            // Even though emulatedSwim is suppressed, the avatar itself still changes animation
            if(emulatedSwim.sqrMagnitude > 0f)
            {
                // Reject full-body leg swim when there's an attempted emulated swim input
                // Emulated movement messes with feet positions due to animation change
                realSwimLockDelay = 4;
            }

            emulatedSwim *= emulatedSwimLockDelay <= 0 ? 1f : 0f;
            legSwim *= realSwimLockDelay <= 0 ? 1f : 0f;

            if(emulatedSwim.sqrMagnitude == 0f)
            {
                // Assisting in making emulated swim's accel curve reset when stopped
                swimMoveTime = Time.time;
            }
            emulatedSwim = emulatedSwim.normalized * Mathf.Min(emulatedSwim.magnitude, 1f) * moveSwimSpeed
                * (isUnderwater ? (Mathf.Cos((Time.time - swimMoveTime) * 5f) + 1f) : 1f);

            Vector3 swimVector = emulatedSwim + handSwim + legSwim;

            // Artificial clamping of upward velocity when already on surface - if using auto float
            if(swimPhysics.autoTreadWater)
            {
                swimVector.y *= swimVector.y < 0 ? 1f : Mathf.Lerp(1f, 0f, swimPhysics.GetRatioOffWater() * 4f);
            }

            return swimVector;
        }

        private bool IsMenuButtonDown()
        {
            if(you.IsUserInVR())
            {
                return Input.GetButtonDown("Oculus_CrossPlatform_Button4") || Input.GetButtonDown("Oculus_CrossPlatform_Button2");
            } else
            {
                // XBox start button is not accessible but the users are few and far between
                return Input.GetKeyDown(KeyCode.Escape);
            }
        }

        private bool IsJumpButtonDownAndCanJump()
        {
            if(!isSwimming || Time.time - touchdownTime > 0.1f) return false;
            VRCInputMethod inputMethod = InputManager.GetLastUsedInputMethod();
            if(inputMethod == VRCInputMethod.Controller) return Input.GetButtonDown("Fire1"); // XBox A button
            if(Input.GetButtonDown("Jump")) return true;
            if(!you.IsUserInVR()) return false;
            // If user is on desktop, stop the check now

            bool isTouchOrKnuckle = inputMethod == VRCInputMethod.Oculus || (int)inputMethod == 10;
            if(isTouchOrKnuckle)
            {
                // Right stick click
                return Input.GetButtonDown("Oculus_CrossPlatform_SecondaryThumbstick");
            } else
            {
                Vector2 hv = new Vector2(
                    Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickHorizontal"),
                    Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickVertical"));
                // Vive or WMR touchpad
                if(hv == Vector2.zero || Mathf.Abs(Mathf.Atan2(hv.x, hv.y)) * Mathf.Rad2Deg < 45f)
                {
                    return Input.GetButtonDown("Oculus_CrossPlatform_SecondaryThumbstick");
                }
            }
            return false;
        }

        public Vector3 GetBuoyancyVector()
        {
            return -Physics.gravity * (buoyancyRatio * buoyancyGravity / (GetDepth() * Mathf.Max(0f, buoyancyFalloff) + 1f) - sinkingGravity);
        }

        private float GetDepth()
        {
            return Mathf.Max(0f, (swimPhysics.surfacePos - swimPhysics.GetTopOfHeadPos()).y);
        }

        private bool IsInWater(Vector3 point)
        {
            return swimPhysics.surfacePos.y > point.y;
        }

        private void PlayWaterSplash(AudioClip clip, Vector3 vector, float threshold)
        {
            if(localOneShotSounds != null && vector.magnitude > threshold)
            {
                Vector3 scaled = Vector3.Scale(vector, new Vector3(0.5f, 1f, 0.5f));
                localOneShotSounds.PlayOneShot(clip, scaled.magnitude * 0.125f);
            }
        }

        private Quaternion VRSwimDirection()
        {
            Quaternion eyeRot = you.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            // Strip off the roll of hand rotation too
            Quaternion handLRot = Quaternion.LookRotation(you.GetTrackingData(VRCPlayerApi.TrackingDataType.LeftHand).rotation * Vector3.forward, Vector3.up);
            Quaternion handRRot = Quaternion.LookRotation(you.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).rotation * Vector3.forward, Vector3.up);
            // If both hands angle difference is high, use more of eye rotation instead starting from 90 deg
            float angleDiff = Quaternion.Angle(handLRot, handRRot); // degrees
            float eyeDirLerp = Mathf.Clamp01(angleDiff / 90f - 1f);

            return Quaternion.Slerp(Quaternion.Slerp(handLRot, handRRot, 0.5f), eyeRot, eyeDirLerp);
        }

        private Quaternion FullBodySwimDirection()
        {
            if(!realisticFullBodySwim)
            {
                return useHandsForDirection ? VRSwimDirection() : you.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).rotation;
            }
            Vector3 avgLegPos = Vector3.Lerp(you.GetBonePosition(HumanBodyBones.LeftFoot), you.GetBonePosition(HumanBodyBones.RightFoot), 0.5f);
            Vector3 headBonePos = you.GetBonePosition(HumanBodyBones.Head);

            return Quaternion.LookRotation((headBonePos - avgLegPos).normalized, Vector3.up);
        }
    }
}