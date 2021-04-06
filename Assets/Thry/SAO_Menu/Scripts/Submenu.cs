
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Thry.SAO
{
    public class Submenu : UdonSharpBehaviour
    {
        public Menu menuManager;
        public GameObject content;
        public bool collapseAfterInstanciate = false;

        [HideInInspector]
        public bool is_sao_submenu_behaviour = true;

        private void Start()
        {
            is_sao_submenu_behaviour = true;
        }

        public void clicked()
        {
            menuManager.ToggleSubMenu(this);
        }
    }
}