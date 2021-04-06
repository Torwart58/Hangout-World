
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Thry.SAO.Button
{
    public class Teleport : UdonSharpBehaviour
    {
        [Header("Type of Teleport")]
        public bool isPlayerTeleport;

        [Header("Point Teleport")]
        public Transform target;

        [Header("Player Teleport")]
        public PlayerManager playerManager;

        [Header("Optional Reference")]
        public Menu menu;
#if THRY_DEV
        public PermissionManager permissionManager;
#endif

        public void clicked()
        {
            Debug.Log("[Thry] Try Teleport \"" + name + "\"");
            if (Networking.LocalPlayer != null)
            {
                if (isPlayerTeleport)
                {
                    VRCPlayerApi target = playerManager.GetPlayerByDisplayName(name);
                    if (target != null)
                    {
                        Debug.Log("[Thry] Execute Teleport to \"" + target.displayName + "\" at " + target.GetPosition());
                        Ray ray = new Ray(target.GetPosition() + target.GetRotation() * Vector3.back + Vector3.up * 2, Vector3.down);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit, 5))
                        {
                            ExecuteTeleport(hit.point, target.GetRotation());
                        }
                        else
                        {
                            Debug.Log("[Thry] Could not find floor to teleport to.");
                        }

                    }
                }
                else if (target != null)
                {
                    ExecuteTeleport(target.position, target.rotation);
                }
                else
                {
                    Debug.Log("[Thry] Teleport Point not specified.");
                }
            }
        }

        private void ExecuteTeleport(Vector3 position, Quaternion rotation)
        {
#if THRY_DEV
            if (permissionManager == null || permissionManager.IsAllowedToTeleport(position))
            {
                Networking.LocalPlayer.TeleportTo(position, rotation);
                if(menu != null) menu.CloseMenu();
            }
            else
                Debug.Log("[Thry][Teleport] Teleport not allowed.");
#else 
            Networking.LocalPlayer.TeleportTo(position, rotation);
            if(menu != null) menu.CloseMenu();
#endif
        }
    }
}
