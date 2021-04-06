
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Thry.SAO
{
    public class Menu : UdonSharpBehaviour
    {
        const string DEBUG_PREFIX = "[Thry][SAO]";

        [Header("Animation")]
        public Vector3 startPosition = new Vector3(0, 450, 0);
        public float ANIMATION_DURATION = 0.5f;
        public float SUBMENU_ANIMATION_DURATION = 0.2f;
        public float ANIMATION_Y_MODIFIER = 0.001f;
        public float ANIMATION_Y_OFFSET = 0.15f;

        public float MENU_HEIGHT = 0.2f;

        public float KEEP_SAME_SUBMENU_DURATION = 10;

        public float BUTTON_CLICK_ANIMATION_DURATION = 0.2f;

        public float CLOSE_VELOCITY = 0.1f;

        private float animation_start_time;
        private int animation_direction = 1;

        private bool finished = false;
        private bool init = false;
        private Vector3[] targetPositions;
        private Vector3[] targetScale;

        private Transform canvas;

        [Header("Test")]
        public bool TEST_VR;

        [Header("Needed References")]
        public UnityEngine.UI.Button emptyUIButton;

        [Header("Optional References")]
        [Tooltip("Used to display mirror sprites when menu is open.")]
        public Thry.Mirror.Opener mirrorOpener;
        [Tooltip("Enables these gameobjects while menu is open. Suggested use indicators.")]
        public GameObject[] inWorldIndicators;

        private float lastClosed = 0;
        private float lastButtonClick = 0;

        public void Start()
        {
            Init();
            OnDisable();
            this.gameObject.SetActive(false);
            finished = true;
        }

        public void OnEnable()
        {
            Init();
            animation_direction = 1;
            animation_start_time = Time.time;
            finished = false;
        }

        public void OnDisable()
        {
            if (init)
            {
                for (int i = 0; i < canvas.childCount; i++)
                {
                    Transform child = canvas.GetChild(i);
                    child.localScale = Vector3.zero;
                    child.localPosition = targetPositions[i];
                }
            }
        }

        private bool prevVRValue = false;
        private void Init()
        {
            if (!init)
            {
                Debug.Log(DEBUG_PREFIX + " INITILITING...");
                canvas = transform.GetChild(0);
                targetPositions = new Vector3[canvas.childCount];
                targetScale = new Vector3[canvas.childCount];
                for (int i = 0; i < canvas.childCount; i++)
                {
                    Transform child = canvas.GetChild(i);
                    targetPositions[i] = child.localPosition;
                    targetScale[i] = child.localScale;
                    child.localScale = Vector3.zero;
                    child.localPosition = startPosition;
                }
                init = true;
            }
            //Checking for changed vr value every time the menu opens, because vrchat only sets the value to true for vr users awhile after joining and Init() was called the first time
            bool vr = TEST_VR || (VRC.SDKBase.Networking.LocalPlayer != null && VRC.SDKBase.Networking.LocalPlayer.IsUserInVR());
            if (prevVRValue != vr)
            {
                Debug.Log(DEBUG_PREFIX + " Is user in vr: " + vr);
                ToggleBoxColliders(transform, vr);
                prevVRValue = vr;
            }
        }

        private void ToggleBoxColliders(Transform parent, bool on)
        {
            //Debug.Log(DEBUG_PREFIX + " Turn " + (on ? "on" : "off") + " box colliders for: " + parent.name);
            foreach (Component collider in parent.GetComponentsInChildren(typeof(BoxCollider), true))
            {
                ((BoxCollider)collider).enabled = on;
            }

        }

        public void ButtonClicked(UnityEngine.UI.Button clickedUIButton)
        {
            lastButtonClick = Time.time;
            clickedUIButton.Select();
        }
        public void ButtonClickedDestop()
        {
            Debug.Log("[Thry][SAO] Button clicked");
            lastButtonClick = Time.time;
        }

        private object[] subMenuObjects = new object[0];
        private object[] subMenuAnimationDirections = new object[0];
        private object[] subMenuAnimationStarttime = new object[0];

        public void ToggleSubMenu(Submenu submenu)
        {
            //Debug.Log(DEBUG_PREFIX + " Toggle Submenu: " + submenu.name);
            if (submenu == null)
                return;
            if (submenu.transform.parent == null)
                return;
            if (submenu.content.activeSelf)
                CloseSubMenu(submenu.content.transform, submenu.name);
            else
                OpenSubMenu(submenu.content.transform, submenu.name);

            for (int i = 0; i < submenu.transform.parent.childCount; i++)
            {
                Transform child = submenu.transform.parent.GetChild(i);
                if (child == submenu.transform)
                    continue;
                UdonBehaviour otherSubMenu = GetSAOSubmenu(child.gameObject);
                if (otherSubMenu != null)
                {
                    GameObject content = (GameObject)otherSubMenu.GetProgramVariable("content");
                    if (content.activeSelf)
                    {
                        //Debug.Log(DEBUG_PREFIX + " Close open menu: " + otherSubMenu.name);
                        CloseSubMenu(content.transform, otherSubMenu.name);
                    }
                }
            }
        }

        private UdonBehaviour GetSAOSubmenu(GameObject o)
        {
            Component c = o.GetComponent(typeof(UdonBehaviour));
            if (c == null)
                return null;
            UdonBehaviour u = (UdonBehaviour)c;
            object isSubmenu = u.GetProgramVariable("is_sao_submenu_behaviour");
            //Debug.Log(o.name + " isSubmenu:" + (isSubmenu==null? "null":isSubmenu.ToString()));
            if (isSubmenu != null && (bool)isSubmenu == true)
            {
                return u;
            }
            return null;
        }

        private void OpenSubMenu(Transform content, string name)
        {
            //Debug.Log(DEBUG_PREFIX + " Open Submenu: " + name);
            if (ListContains(subMenuObjects, content) == false)
            {
                ToggleBoxColliders(content, false);
                content.localScale = Vector3.zero;
                content.gameObject.SetActive(true);
                subMenuObjects = ListAdd(subMenuObjects, content);
                subMenuAnimationDirections = ListAdd(subMenuAnimationDirections, 1);
                subMenuAnimationStarttime = ListAdd(subMenuAnimationStarttime, Time.time);
            }
        }

        private void CloseSubMenu(Transform content, string name)
        {
            //Debug.Log(DEBUG_PREFIX + " Close Submenu: " + name);
            ToggleBoxColliders(content, false);
            if (ListContains(subMenuObjects, content) == false)
            {
                subMenuObjects = ListAdd(subMenuObjects, content);
                subMenuAnimationDirections = ListAdd(subMenuAnimationDirections, -1);
                subMenuAnimationStarttime = ListAdd(subMenuAnimationStarttime, Time.time);
            }
            else
            {
                int index = ListGetIndex(subMenuObjects, content);
                if ((int)subMenuAnimationDirections[index] == 1)
                {
                    subMenuAnimationDirections[index] = -1;
                    subMenuAnimationStarttime[index] = Time.time - (SUBMENU_ANIMATION_DURATION - (Time.time - (float)subMenuAnimationStarttime[index]));
                }
            }
        }

        private void AnimateSubMenus()
        {
            for (int i_menu = 0; i_menu < subMenuObjects.Length; i_menu++)
            {
                float scale = (Time.time - (float)subMenuAnimationStarttime[i_menu]) / SUBMENU_ANIMATION_DURATION;
                int direction = (int)subMenuAnimationDirections[i_menu];
                if (direction == -1)
                    scale = 1 - scale;
                scale = Mathf.Min(1, Mathf.Max(scale, 0));
                Transform t = (Transform)subMenuObjects[i_menu];
                t.localScale = new Vector3(scale, scale, scale);
                if ((scale == 1 && direction == 1) || (scale == 0 && direction == -1))
                {
                    if (scale == 0)
                        t.gameObject.SetActive(false);
                    if ((TEST_VR) || (VRC.SDKBase.Networking.LocalPlayer != null && VRC.SDKBase.Networking.LocalPlayer.IsUserInVR()))
                        ToggleBoxColliders(t, true);
                    subMenuObjects = ListRemoveIndex(subMenuObjects, i_menu);
                    subMenuAnimationDirections = ListRemoveIndex(subMenuAnimationDirections, i_menu);
                    subMenuAnimationStarttime = ListRemoveIndex(subMenuAnimationStarttime, i_menu);
                }
            }
        }

        float RELATIVE_MENU_POSITION = 0.07f;
        float RELATIVE_MENU_POSITION_DESKTOP = 0.3f;

        private void ResetSubMenu()
        {
            foreach (Component transform in transform.GetComponentsInChildren(typeof(UdonBehaviour), true))
            {
                UdonBehaviour u = GetSAOSubmenu(transform.gameObject);
                if (u != null)
                {
                    GameObject submenu = (GameObject)u.GetProgramVariable("content");
                    submenu.SetActive(false);
                }
            }
        }


        //open menu
        //only if menu is completly closed
        public void OpenMenu(bool isRightHand)
        {
            if (this.gameObject.activeSelf == false)
            {
                if (Time.time - lastClosed > KEEP_SAME_SUBMENU_DURATION)
                {
                    ResetSubMenu();
                }
                SetMenuPosition(isRightHand);
                animation_direction = 1;
                animation_start_time = Time.time;
                this.gameObject.SetActive(true);
                //enable inworld indicators
                foreach (GameObject o in inWorldIndicators) o.SetActive(true);
                if (mirrorOpener != null)
                    mirrorOpener.ToggleSprites(true);
            }
        }

        private void SetMenuPosition(bool isRightHand)
        {
            float size = GetLocalAvatarHeight();
            this.gameObject.transform.localScale = new Vector3(size, size, size);
            VRCPlayerApi.TrackingData head = VRC.SDKBase.Networking.LocalPlayer.GetTrackingData(VRC.SDKBase.VRCPlayerApi.TrackingDataType.Head);
            VRCPlayerApi.TrackingData hand;
            if (isRightHand == true)
                hand = VRC.SDKBase.Networking.LocalPlayer.GetTrackingData(VRC.SDKBase.VRCPlayerApi.TrackingDataType.RightHand);
            else
                hand = VRC.SDKBase.Networking.LocalPlayer.GetTrackingData(VRC.SDKBase.VRCPlayerApi.TrackingDataType.LeftHand);
            if (TEST_VR || VRC.SDKBase.Networking.LocalPlayer.IsUserInVR())
            {
                Vector3 relativePosition = (head.rotation * Vector3.forward).normalized * RELATIVE_MENU_POSITION * size;
                relativePosition.y += MENU_HEIGHT / 2 * size;
                this.transform.position = (hand.position) + relativePosition;
            }
            else
            {
                Vector3 relativePosition = (head.rotation * Vector3.forward).normalized * RELATIVE_MENU_POSITION_DESKTOP * size;
                this.transform.position = head.position + relativePosition;
            }
            this.transform.rotation = Quaternion.Euler(0, head.rotation.eulerAngles.y, 0);
        }

        public void CloseMenu()
        {
            if (finished)
            {
                animation_direction = -1;
                animation_start_time = Time.time;
                lastClosed = Time.time;
                finished = false;
                //disable inworld indicators
                foreach (GameObject o in inWorldIndicators) o.SetActive(false);
                if (mirrorOpener != null)
                    mirrorOpener.ToggleSprites(false);
            }
        }

        //startPosition - targetPos.y
        //min startPositon
        //max targetPos

        private void Update()
        {
            if (!finished)
            {
                Animate();
            }
            if (Networking.LocalPlayer != null)
            {
                if (VRC.SDKBase.Networking.LocalPlayer.GetVelocity().magnitude > CLOSE_VELOCITY)
                {
                    CloseMenu();
                }
            }
            if (lastButtonClick != 0 && Time.time - lastButtonClick > BUTTON_CLICK_ANIMATION_DURATION)
            {
                lastButtonClick = 0;
                emptyUIButton.Select();
            }
            AnimateSubMenus();

        }

        private void Animate()
        {
            if (RescaleObjects())
                finished = true;
            if (finished && animation_direction == -1)
            {
                this.gameObject.SetActive(false);
            }
        }

        private bool RescaleObjects()
        {
            float scale = (Time.time - animation_start_time) / ANIMATION_DURATION;
            if (animation_direction == -1)
                scale = 1 - scale;
            bool allDone = true;
            for (int i = 0; i < canvas.childCount; i++)
            {
                float localScale = (scale - (targetPositions[i].y) * ANIMATION_Y_MODIFIER - ANIMATION_Y_OFFSET);
                localScale = Mathf.Min(1, Mathf.Max(localScale, 0));
                if ((localScale < 1 && animation_direction == 1) || (localScale > 0 && animation_direction == -1))
                    allDone = false;
                Transform child = canvas.GetChild(i);
                child.localScale = targetScale[i] * localScale;
                child.localPosition = (startPosition + (targetPositions[i] - startPosition) * localScale);
            }
            return allDone;
        }

        //--------------List Helpers--------------

        private object[] ListAdd(object[] array, object o)
        {
            object[] newArray = new object[array.Length + 1];
            for (int i = 0; i < array.Length; i++)
                newArray[i] = array[i];
            newArray[array.Length] = o;
            return newArray;
        }

        private object[] ListRemoveObject(object[] array, object o)
        {
            object[] newArray = new object[array.Length - 1];
            int newArrayIndex = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] != o)
                    newArray[newArrayIndex++] = array[i];
            }
            return newArray;
        }

        private object[] ListRemoveIndex(object[] array, int removeIndex)
        {
            object[] newArray = new object[array.Length - 1];
            int newArrayIndex = 0;
            for (int i = 0; i < array.Length; i++)
            {
                if (i != removeIndex)
                    newArray[newArrayIndex++] = array[i];
            }
            return newArray;
        }

        private bool ListContains(object[] array, object o)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == o)
                    return true;
            }
            return false;
        }

        private int ListGetIndex(object[] array, object o)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == o)
                    return i;
            }
            return -1;
        }

        //--------------PLAYER HEIGHT----------------

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
    }
}