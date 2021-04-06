
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class newvendingbutton : UdonSharpBehaviour
{
    public Material[] Canmats;
    public GameObject sodaCounter;
    public GameObject spawnlocation;

    private void Interact()
    {
        SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "spawnsoda");
        

    }

   public void spawnsoda()
    {
        Canmats[1] = GetComponent<Renderer>().material;
        SodaManager sodamanager = sodaCounter.GetComponent<SodaManager>();
        int i = sodamanager.sodacount;
        sodamanager.sodacans[i].SetActive(true);
        sodamanager.sodacans[i].GetComponent<Renderer>().materials = Canmats;
        sodamanager.sodacans[i].transform.position = spawnlocation.transform.position;
        if (i >= sodamanager.sodacans.Length - 1)
        {
            sodamanager.sodacount = 0;
        }
        else
        {
            sodamanager.sodacount++;
        }
    }


}
