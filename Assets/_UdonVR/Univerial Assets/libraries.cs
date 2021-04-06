
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonVR
{
    public class UdonVR
    {
        static public void Haptics(string Hand, float Duration, float Amp, float Frequency)
        {
            if (Hand.ToLower() == "left")
            {
                Networking.LocalPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Left, Duration, Amp, Frequency);
            }
            else if (Hand.ToLower() == "right")
            {
                Networking.LocalPlayer.PlayHapticEventInHand(VRC_Pickup.PickupHand.Right, Duration, Amp, Frequency);
            }
        }

        static public void ToggleList(GameObject[] _List, bool _State)
        {
            foreach (GameObject _Object in _List)
            {
                _Object.SetActive(_State);
            }
        }
    }
}

