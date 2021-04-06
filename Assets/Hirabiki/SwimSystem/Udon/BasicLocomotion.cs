namespace Hirabiki.Udon.Works
{
    using UdonSharp;
    using UnityEngine;
    using VRC.SDKBase;
    using VRC.Udon;

    //public class BasicLocomotion : UdonSharpBehaviour, ILocomotion
    public class BasicLocomotion : UdonSharpBehaviour
    {
        // A reference locomotion interface (for use in your world. "Let's Udon Swimming!" version is customized)
        // To make it an interface, this script is meant to be referenced as a generic UdonBehaviour.
        // You can implement your own if you have other use cases (such as when player is in a Station)

        // Traditional udon can't call function and get return value
        [Tooltip("Disable if player is in a Station or systems that override default locomotion")]
        public bool useVRCLocomotion = true;
        private bool immobile = false;

        void Start()
        {
            //if(you == null) gameObject.SetActive(false);

            if(useVRCLocomotion)
            {
                EnableVRCLocomotion();
            } else
            {
                DisableVRCLocomotion();
            }
        }

        void OnDisable()
        {
            Immobilize(false);
            SetGravityStrength(1f);
        }

        public void EnableVRCLocomotion()
        {
            useVRCLocomotion = true;
            // Implement needed changes here
        }
        public void DisableVRCLocomotion()
        {
            useVRCLocomotion = false;
            // Implement needed changes here
        }

        public Vector3 GetMoveVector()
        {
            // 1.0.2 Update: All controls can now be used at the same time in case only the right VR controller is alive
            Vector3 outputAxis = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            outputAxis += new Vector3(Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryThumbstickHorizontal"),
                0f, Input.GetAxisRaw("Oculus_CrossPlatform_PrimaryThumbstickVertical"));
            outputAxis += new Vector3(Input.GetAxisRaw("Joy1 Axis 1"),
                0f, -Input.GetAxisRaw("Joy1 Axis 2")); //XBox left stick

            // In "Let's Udon Swimming" world, there is a special logic that detect vive wand 
            // which allows for proper traditional VRChat locomotion (eliminates touchy locomotion when gesturing)
            // However it requires a special "headset detector" that can detect Vive Cosmos head and assume they have Cosmos controller
            // which require a bit of extra code and a referencing to "Main Camera" in the scene
            // Alternatively, it would add another toggle for player to your world's settings panel and is only appropriate for vive wands
            // Because otherwise, they can't move at all, which is worse than touchy locomotion for vive wands players
            // Because Vive wands and Vive cosmos likely have the same VRCInputMethod but one has the touchpad, the other thumbstick
            // A thumbstick click == A touchpad click
            // A Vive's touchpad click is required to move
            // But Touch/Knuckle/WMR/Cosmos's thumbstick click is not used to move
            return Vector3.ClampMagnitude(outputAxis, 1f);
        }

        //---- Interface implementation ----//
        public Vector3 GetVelocity()
        {
            // if(useVRCLocomotion)
            return Networking.LocalPlayer.GetVelocity();
        }
        public void SetVelocity(Vector3 v)
        {
            // if(useVRCLocomotion)
            Networking.LocalPlayer.SetVelocity(v);
        }
        public void SetGravityStrength(float f)
        {
            // if(useVRCLocomotion)
            Networking.LocalPlayer.SetGravityStrength(f);
        }
        public void Immobilize(bool b)
        {
            // if(useVRCLocomotion)
            if(b != immobile)
            {
                Networking.LocalPlayer.Immobilize(b);
                immobile = b;
            }
        }
        public bool IsImmobile()
        {
            return immobile;
        }
        public bool IsOnGround()
        {
            // if(useVRCLocomotion)
            return Networking.LocalPlayer.IsPlayerGrounded();
        }
        public bool IsUsingVRCLocomotion()
        {
            // return useVRCLocomotion;
            return true;
        }
    }
}