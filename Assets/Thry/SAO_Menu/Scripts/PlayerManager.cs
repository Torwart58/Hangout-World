
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Thry
{
    public class PlayerManager : UdonSharpBehaviour
    {
        public VRCPlayerApi[] PLAYER_APIS;
        public GameObject[] NOTIDFY_ON_UPDATE;

        void Start()
        {
            PLAYER_APIS = new VRCPlayerApi[0];
        }

        public override void OnPlayerJoined(VRCPlayerApi joinedPlayerApi)
        {
            VRCPlayerApi[] newplayerapis = new VRCPlayerApi[PLAYER_APIS.Length + 1];
            int oldI = 0;
            bool added = false;
            for (int i = 0; i < newplayerapis.Length; i++)
            {
                if (PLAYER_APIS.Length > 0 && oldI < PLAYER_APIS.Length && (compare(PLAYER_APIS[oldI], joinedPlayerApi) > -1 || added))
                    newplayerapis[i] = PLAYER_APIS[oldI++];
                else
                {
                    newplayerapis[i] = joinedPlayerApi;
                    added = true;
                }
            }
            PLAYER_APIS = newplayerapis;
            Debug.Log("[Thry] Added Player.");
            Log();
            NotifyUpdate();
        }
        public override void OnPlayerLeft(VRCPlayerApi leftPlayerApi)
        {
            VRCPlayerApi[] newplayerapis = new VRCPlayerApi[PLAYER_APIS.Length - 1];
            int newI = 0;
            for (int i = 0; i < PLAYER_APIS.Length; i++)
            {
                if (PLAYER_APIS[i] != leftPlayerApi)
                {
                    newplayerapis[newI++] = PLAYER_APIS[i];
                }
            }
            PLAYER_APIS = newplayerapis;
            NotifyUpdate();
        }


        public VRCPlayerApi GetPlayerByDisplayName(string displayname)
        {
            for (int i = 0; i < PLAYER_APIS.Length; i++)
            {
                if (PLAYER_APIS[i].displayName == displayname)
                {
                    return PLAYER_APIS[i];
                }
            }
            return null;
        }

        public void NotifyUpdate()
        {
            foreach (GameObject o in NOTIDFY_ON_UPDATE)
            {
                UdonBehaviour u = (UdonBehaviour)o.GetComponent(typeof(UdonBehaviour));
                if (u != null)
                    u.SendCustomEvent("OnPlayerListChange");
            }
        }

        int compare(VRCPlayerApi p1, VRCPlayerApi p2)
        {
            return System.String.Compare(p1.displayName, p2.displayName, System.StringComparison.CurrentCultureIgnoreCase);
        }

        private void SortStringArray(string[] arr)
        {
            int i, j, min;
            for (i = 0; i < arr.Length; i++)
            {
                min = i;
                for (j = 0; j < arr.Length; j++)
                {
                    if (System.String.Compare(arr[j], arr[min], System.StringComparison.CurrentCultureIgnoreCase) < 0)
                    {
                        min = j;
                        string temp = arr[i];
                        arr[i] = arr[min];
                        arr[min] = temp;
                    }
                }
            }
        }

        void Log()
        {
            Debug.Log("=================================================");
            Debug.Log("Number of Players:" + PLAYER_APIS.Length);
            for (int i = 0; i < PLAYER_APIS.Length; i++)
            {
                Debug.Log(PLAYER_APIS[i].displayName);
            }
            Debug.Log("=================================================");
        }
    }
}