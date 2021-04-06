
#if !COMPILER_UDONSHARP && UNITY_EDITOR // These using statements must be wrapped in this check to prevent issues on builds
using UnityEditor;
using UdonSharpEditor;
using System.Linq;
using VRC.SDK3.Video.Components.AVPro;
using UnityEngine;
#endif

namespace UdonVR.Takato
{

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(UdonSyncVideoPlayer))]
    public class UdonSyncVideoPlayerEditor : Editor
    {
        string[] notProps = new string[5] { "Base", "m_Script", "size", "data", "url" };

        public override void OnInspectorGUI()
        {
            // Draws the default convert to UdonBehaviour button, program asset field, sync settings, etc.
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target, false, false)) return;
            UdonSyncVideoPlayer inspectorBehaviour = (UdonSyncVideoPlayer)target;
            serializedObject.Update();
            var p = serializedObject.GetIterator();
            do
            {
                if (!notProps.Contains(p.name))
                {
                    if (p.name != "videoURL")
                        EditorGUILayout.PropertyField(p, p.hasChildren);
                    else if (inspectorBehaviour.autoPlay)
                    {
                        EditorGUILayout.PropertyField(p, p.hasChildren);
                    }
                }
                //if (p.name == "someProperty")
                //{
                // Add extra GUI after "someProperty"
                //}
            }
            while (p.NextVisible(true));


            if (inspectorBehaviour.videoPlayer != null)
            {
                if (inspectorBehaviour.videoPlayer.GetType() == typeof(VRCAVProVideoPlayer))
                {
                    if (((VRCAVProVideoPlayer)inspectorBehaviour.videoPlayer).AutoPlay)
                    {
                        EditorGUILayout.HelpBox("This player works best when not usuing the AutoPlay options on the player, Please use the AutoPlay options on this script", MessageType.Info);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("This player works best when not usuing the AutoPlay options on the player, Please use the AutoPlay options on this script", MessageType.Info);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
        
    }
#endif
}
