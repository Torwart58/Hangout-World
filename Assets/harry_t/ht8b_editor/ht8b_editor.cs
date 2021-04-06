using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UdonSharp;
using VRC.Udon.Common.Interfaces;

[System.Serializable]
public struct ht8b_collision_data_t
{
   public bool bInit;

   public float k_TABLE_WIDTH     ;//= 1.054f;
   public float k_TABLE_HEIGHT    ;//= 0.605f;
   public float k_POCKET_RADIUS   ;//= 0.100f;
   public float k_CUSHION_RADIUS  ;//= 0.043f;
   public float k_INNER_RADIUS    ;//= 0.100f;

   public Vector3 _cornerpocket;
   public Vector3 _sidepocket;

   public void init_default()
   {
      this.k_TABLE_WIDTH = 1.054f;
      this.k_TABLE_HEIGHT = 0.605f;
      this.k_CUSHION_RADIUS = 0.043f;
      this.k_POCKET_RADIUS = 0.100f;
      this.k_INNER_RADIUS = 0.072f;
      this._cornerpocket = new Vector3( 1.087f, 0.0f, 0.627f );
      this._sidepocket = new Vector3( 0.000f, 0.0f, 0.665f );
      this.bInit = true;
   }
}

public class ht8b_editor : MonoBehaviour
{
   public ht8b_config config;
}

#if UNITY_EDITOR 
[CustomEditor(typeof(ht8b_editor))]
public class ht8b_editor_inspector : Editor
{
   bool bShowTable = true;
   bool bShowResource = false;
   bool bResourceInit = false;
   bool bShowCollision = false;

   bool bAllowCompile = false;

   static GUIStyle styleHeader;
   static GUIStyle styleError;
   static GUIStyle styleWarning;
   static bool gui_resource_ready = false;

   table_configurator cdata_displayTarget;
   private static void DrawError( string szError, GUIStyle style )
   {
      GUILayout.BeginVertical( "GroupBox" );
      GUILayout.Label( szError, style );
      GUILayout.EndVertical();
   }

   private static bool Material_ht8b_supports( ref Material mat )
   {
      bool isFullSupport = true;

      if( !mat.HasProperty( "_EmissionColor" ) )
      {
         DrawError( $"[!] Shader '{mat.shader.name}' does not have property: _EmissionColor", styleError );
         isFullSupport = false;
      }

      if( !mat.HasProperty( "_Color" ) )
      {
         DrawError( $"Shader {mat.shader.name} does not have property: _Color", styleWarning );
      }

      return isFullSupport;
   }

   private static bool Prefab_ht8b_supports( ref GameObject pf )
   {
      bool success = true;

      if( !pf.transform.Find( ".4BALL_FILL" ) )
      {
         DrawError( "Prefab does not contain child object named: '.4BALL_FILL' (pocket blockers)", styleError );
         success = false;
      }

      if( !pf.transform.Find( ".RACK" ) )
      {
         DrawError( "Prefab does not contain child object named: '.RACK'", styleError );
         success = false;
      }

      if( !pf.transform.Find( ".TABLE_SURFACE" ) )
      {
         DrawError( "Prefab does not contain child object named: '.TABLE_SURFACE'", styleError );
         success = false;
      }

      return success;
   }

   private static void Ht8bUIGroup( string szHeader )
   {
      GUILayout.BeginVertical( "HelpBox" );
      GUILayout.Label( szHeader, styleHeader );
   }

   private static bool Ht8bUIGroupMitButton( string szHeader, string szButton )
   {
      GUILayout.BeginVertical( "HelpBox" );
      GUILayout.BeginHorizontal();
      GUILayout.Label( szHeader, styleHeader );
      bool b = GUILayout.Button( szButton );
      GUILayout.EndHorizontal();

      return b;
   }
   
   private static void Ht8bUIGroupEnd()
   {
      GUILayout.EndVertical();
   }

   private static void gui_resource_init()
   {
      styleHeader = new GUIStyle()
      {
         fontSize = 14,
         fontStyle = FontStyle.Bold
      };

      styleWarning = new GUIStyle()
      {
         wordWrap = true
      };

      styleError = new GUIStyle()
      {  
         fontStyle = FontStyle.Bold,
         wordWrap = true
      };

      gui_resource_ready = true;
   }

   private static void ui_9x9ColourGrid( Color[] colours )
   {
      GUILayout.BeginHorizontal();
      GUILayout.Label( "Ball Colours" );
      GUILayout.FlexibleSpace();

      GUILayout.BeginVertical();

      for( int y = 0; y < 3; y ++ )
      {
         GUILayout.BeginHorizontal();

         for( int x = 0; x < 3; x ++ )
         {
            colours[ y * 3 + x ] = EditorGUILayout.ColorField( GUIContent.none, colours[ y * 3 + x ], false, false, false, GUILayout.Width( 50.0f ), GUILayout.Height( 50.0f ) );
         }

         GUILayout.EndHorizontal();
      }

      GUILayout.EndVertical();
      GUILayout.EndHorizontal();
   }

   public static Color alpha1( Color src )
   {
      src.a = 1.0f;
      return src;
   }

   public static void balls_showlimited( Transform root, uint mask )
   {
      GameObject[] balls = new GameObject[16]; 
      Transform rootballs = root.Find("intl.balls");
      for( int i = 0; i < 16; i ++ )
      {
         rootballs.GetChild(i).gameObject.SetActive( ((mask >> i) & 0x1u) != 0x0u );
      }
   }

   public override void OnInspectorGUI() 
   {
      if( !gui_resource_ready )
      {
         gui_resource_init();
      }

      ht8b_editor _editor = (ht8b_editor)target;

      base.DrawDefaultInspector();

      EditorGUI.BeginChangeCheck();

      ht8b_config cfg = _editor.config;

      if( cfg != null )
      {
         bAllowCompile = true;

         Ht8bUIGroup( "Table setup" );

            cfg._table_setup_prefab = (GameObject)EditorGUILayout.ObjectField( "Table artwork", cfg._table_setup_prefab, typeof(GameObject), false );

            if( cfg._table_setup_prefab )
            {
               if( !Prefab_ht8b_supports( ref cfg._table_setup_prefab ) )
               {
                  bAllowCompile = false;
               }
            }
            else
            {
               DrawError( "Prefab needs to be set to check structure", styleError );
            }

            cfg._table_surface_mat = (Material)EditorGUILayout.ObjectField( "Table surface material", cfg._table_surface_mat, typeof(Material), false );
            cfg._ball_mat = (Material)EditorGUILayout.ObjectField( "Ball material", cfg._ball_mat, typeof(Material), false );

            if( cfg._table_surface_mat )
            {
               if( !Material_ht8b_supports( ref cfg._table_surface_mat ) )
               {
                  bAllowCompile = false;
               }
            }
            else
            {
               DrawError( "Material needs to be set to check shader properties", styleError );
            }

            Ht8bUIGroup( "Collision info" );

               if( !this.cdata_displayTarget )
               {
                  this.cdata_displayTarget = _editor.gameObject.transform.Find( "intl.balls" ).Find( "__table_refiner__" ).gameObject.GetComponent< table_configurator >();
               }

               this.bShowCollision = EditorGUILayout.Toggle( "Draw collision data", this.cdata_displayTarget.gameObject.activeSelf );
               this.cdata_displayTarget.gameObject.SetActive( this.bShowCollision );

               if( !cfg.cdata.bInit )
               {
                  cfg.cdata.init_default();
               }

               cfg.cdata.k_TABLE_WIDTH = EditorGUILayout.Slider( "Width", cfg.cdata.k_TABLE_WIDTH, 0.4f, 2.4f );
               cfg.cdata.k_TABLE_HEIGHT = EditorGUILayout.Slider( "Height", cfg.cdata.k_TABLE_HEIGHT, 0.4f, 2.4f );
               cfg.cdata.k_POCKET_RADIUS = EditorGUILayout.Slider( "Pocket Radius", cfg.cdata.k_POCKET_RADIUS, 0.06f, 0.4f );
               cfg.cdata.k_CUSHION_RADIUS = EditorGUILayout.Slider( "Cushion Radius", cfg.cdata.k_CUSHION_RADIUS, 0.01f, 0.4f );
               cfg.cdata.k_INNER_RADIUS = EditorGUILayout.Slider( "Pocket Trigger Radius", cfg.cdata.k_INNER_RADIUS, 0.03f, 0.3f );

               cfg.cdata._cornerpocket = EditorGUILayout.Vector3Field( "Corner pocket location", cfg.cdata._cornerpocket );
               cfg.cdata._sidepocket = EditorGUILayout.Vector3Field( "Side pocket location", cfg.cdata._sidepocket );

               this.cdata_displayTarget.cdata = cfg.cdata;

               cfg._collision_data_prefab = (GameObject)EditorGUILayout.ObjectField( "(VFX) Collision model", cfg._collision_data_prefab, typeof(GameObject), false );

               if( !cfg._collision_data_prefab )
               {
                  DrawError( "Without a collision prefab, balls will instantly dissapear when pocketed!", styleWarning );
               }

            Ht8bUIGroupEnd();

         Ht8bUIGroupEnd();

         Ht8bUIGroup( "Global" );

            cfg._colourDefault = alpha1(EditorGUILayout.ColorField( new GUIContent("Default edge light"), cfg._colourDefault, false, false, false ));
            cfg._colourFoul = alpha1(EditorGUILayout.ColorField( new GUIContent("Foul colour"), cfg._colourFoul, false, false, false ));

         Ht8bUIGroupEnd();

         if( Ht8bUIGroupMitButton( "8 Ball", "test it" ) )
         {
            cfg.RenderProcedural_8ball();
            AssetDatabase.Refresh();
            cfg._ball_mat.SetTexture( "_MainTex", (Texture2D)AssetDatabase.LoadAssetAtPath( "Assets/harry_t/ht8b_materials/procedural/tballs_8ball.png", typeof(Texture2D) ) );
            cfg._table_surface_mat.SetColor( "_EmissionColor", cfg._8ball_team_colour_0 * 1.5f );
            cfg._table_surface_mat.SetColor( "_Color", cfg._8ball_fabric_colour );

            balls_showlimited( _editor.gameObject.transform, 0xffffu );

            _editor.transform.Find( "intl.table" ).Find( "table_artwork" ).Find(".4BALL_FILL").gameObject.SetActive( false );
         }

            cfg._8ball_fabric_colour = EditorGUILayout.ColorField( new GUIContent("Surface Colour"), cfg._8ball_fabric_colour, false, true, false );
            cfg._8ball_team_colour_0 = alpha1(EditorGUILayout.ColorField( new GUIContent("Spots Colour"), cfg._8ball_team_colour_0, false, false, false ));
            cfg._8ball_team_colour_1 = alpha1(EditorGUILayout.ColorField( new GUIContent("Stripes Colour"), cfg._8ball_team_colour_1, false, false, false ));

         Ht8bUIGroupEnd();

         if( Ht8bUIGroupMitButton( "9 Ball", "test it" ) )
         {
            cfg.RenderProcedural_9ball_4ball();
            AssetDatabase.Refresh();
            cfg._ball_mat.SetTexture( "_MainTex", (Texture2D)AssetDatabase.LoadAssetAtPath( "Assets/harry_t/ht8b_materials/procedural/tballs_9ball.png", typeof(Texture2D) ) );
            cfg._table_surface_mat.SetColor( "_EmissionColor", cfg._colourDefault * 1.5f );
            cfg._table_surface_mat.SetColor( "_Color", cfg._9ball_fabric_colour );

            balls_showlimited( _editor.gameObject.transform, 0x03ffu );

            _editor.transform.Find( "intl.table" ).Find( "table_artwork" ).Find(".4BALL_FILL").gameObject.SetActive( false );
         }

            cfg._9ball_fabric_colour = EditorGUILayout.ColorField( new GUIContent("Surface Colour"), cfg._9ball_fabric_colour, false, true, false );
            ui_9x9ColourGrid( cfg._9ball_colours );

         Ht8bUIGroupEnd();

         if( Ht8bUIGroupMitButton( "4 Ball", "test it" ) )
         {
            cfg.RenderProcedural_9ball_4ball();
            AssetDatabase.Refresh();
            cfg._ball_mat.SetTexture( "_MainTex", (Texture2D)AssetDatabase.LoadAssetAtPath( "Assets/harry_t/ht8b_materials/procedural/tballs_9ball.png", typeof(Texture2D) ) );
            cfg._table_surface_mat.SetColor( "_EmissionColor", cfg._4ball_team_colour_0 * 1.5f );
            cfg._table_surface_mat.SetColor( "_Color", cfg._4ball_fabric_colour );

            balls_showlimited( _editor.gameObject.transform, 0xf000u );

            _editor.transform.Find( "intl.table" ).Find( "table_artwork" ).Find(".4BALL_FILL").gameObject.SetActive( true );
         }

            cfg._4ball_fabric_colour = EditorGUILayout.ColorField( new GUIContent("Surface Colour"), cfg._4ball_fabric_colour, false, true, false );
            cfg._4ball_team_colour_0 = alpha1(EditorGUILayout.ColorField( new GUIContent("Team A Colour"), cfg._4ball_team_colour_0, false, false, false ));
            cfg._4ball_team_colour_1 = alpha1(EditorGUILayout.ColorField( new GUIContent("Team B Colour"), cfg._4ball_team_colour_1, false, false, false ));
            cfg._4ball_objective_colour = alpha1(EditorGUILayout.ColorField( new GUIContent("Objective colour"), cfg._4ball_objective_colour, false, false, false ));
         
         Ht8bUIGroupEnd();

         Ht8bUIGroup( "Quest / shaders" );

         EQuestStuffUI switchto = quest_stuff.DrawQuestStuffGUI( ref cfg.quest_switch_data );
         if( switchto != EQuestStuffUI.k_EQuestStuffUI_noaction )
         {
            quest_stuff.ApplyReplacement( ref cfg.quest_switch_data, switchto );
            
            // Chain replacements switch into internal materials
            quest_stuff intl_replacements = _editor.gameObject.transform.Find( "__intl_quest_toggle__" ).gameObject.GetComponent< quest_stuff >();
            quest_stuff.ApplyReplacement( ref intl_replacements.data, switchto );
         }

         Ht8bUIGroupEnd();

         bShowResource = EditorGUILayout.Foldout( bShowResource, "Texture sources", true, EditorStyles.foldout );

         if( bShowResource )
         {
            cfg._src_ball_content = (Texture2D)EditorGUILayout.ObjectField( "8/9 Ball layout", cfg._src_ball_content, typeof(Texture2D), false );
            cfg._src_ball_spinmarkers = (Texture2D)EditorGUILayout.ObjectField( "Ball spin marker", cfg._src_ball_spinmarkers, typeof(Texture2D), false );
         }

         if( !bAllowCompile )
         {
            //GUI.enabled = false;
         }

         if( GUILayout.Button("Compile & Apply config") )
         {
            Debug.Log( "Running ht8b config" );

            cfg.RenderProceduralTextures();
            cfg.ApplyConfig( _editor.gameObject.transform );
         }

         GUI.enabled = true;

         if( GUI.changed )
         {
            EditorUtility.SetDirty( cfg );
         }
      }
      else
      {
         GUILayout.Label( "No config set" );
      }

      if (EditorGUI.EndChangeCheck())
      {
         Undo.RegisterCompleteObjectUndo( cfg, "edited ht8b config" );
      }
   }
}
#endif