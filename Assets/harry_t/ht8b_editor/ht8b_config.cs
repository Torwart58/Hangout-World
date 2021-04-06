using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEditor;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[CreateAssetMenu(fileName = "config", menuName = "ht8b/config", order = 1)]
[System.Serializable]
public class ht8b_config : ScriptableObject
{
   public Color _colourDefault;
   public Color _colourFoul;

   public Color _8ball_fabric_colour;
   public Color _8ball_team_colour_0;
   public Color _8ball_team_colour_1;

   public Color _9ball_fabric_colour;
   public Color[] _9ball_colours = new Color[9];

   public Color _4ball_fabric_colour;
   public Color _4ball_team_colour_0;
   public Color _4ball_team_colour_1;
   public Color _4ball_objective_colour;

   public Texture2D _src_ball_content;
   public Texture2D _src_ball_spinmarkers;

   public Material _table_surface_mat;       // Interactive surface material
   public Material _ball_mat;
   public GameObject _table_setup_prefab;    // 'Visual prefab' setup
   public GameObject _collision_data_prefab;

   public float _edge_gap;

   public ht8b_collision_data_t cdata;

   public quest_stuff_data quest_switch_data;

   #if UNITY_EDITOR 
   private static void Image_ApplyTones_Mul( ref Color[] source, ref Color[] dest, Color R, Color G, Color B )
   {
      for( int i = 0; i < source.Length; i ++ )
      {
         Color pSrc = source[i];
         ref Color pOut = ref dest[i];

         pOut = Color.white;

         pOut = pOut * ( 1F-pSrc.r ) + R * pSrc.r;
         pOut = pOut * ( 1F-pSrc.g ) + G * pSrc.g;
         pOut = pOut * ( 1F-pSrc.b ) + B * pSrc.b;

         pOut.a = 1F;
      }
   }

   private static void Image_BlendAlphaOver( ref Color[] dst, ref Color[] src )
   {
      for( int i = 0; i < dst.Length; i ++ )
      {
         // ADD // ONE_MINUS_SRC_ALPHA // SRC_ALPHA
         dst[i] = (src[i]*src[i].a) + (dst[i]*(1F-src[i].a));
      }
   }

   private static void Image_ColourSqr( ref Color[] dst, int imgw, int imgh, int x, int y, int w, int h, Color src )
   {
      Color withoutAlpha = src;
      withoutAlpha.a = 1.0f;

      for( int _y = y; _y < y + h; _y ++ )
      {
         for( int _x = x; _x < x + w; _x ++ )
         {
            ref Color pDst = ref dst[ _y * imgw + _x ];

            float alpha = Mathf.Clamp01( pDst.r + pDst.g + pDst.b );

            pDst = Color.white * ( 1F-alpha ) + src * alpha;
            pDst.a = 1F;
         }
      }
   }

   private static void Image_FillSqr( ref Color[] dst, int imgw, int imgh, int x, int y, int w, int h, Color src )
   {
      for( int _y = y; _y < y + h; _y ++ )
      {
         for( int _x = x; _x < x + w; _x ++ )
         {
            dst[ _y * imgw + _x ] = src;
         }
      }
   }

   // RGB invert
   private static void Image_Invert( ref Color[] dst )
   {
      for( int i = 0; i < dst.Length; i ++ )
      {
         ref Color c = ref dst[i];

         c.r = 1F - c.r;
         c.g = 1F - c.g;
         c.b = 1F - c.b;
      }
   }

   private static void WriteTex2dAsset( ref Color[] src, int x, int y, string path )
   {
      Texture2D writetex = new Texture2D( x,y, TextureFormat.RGBA32, false );
      writetex.SetPixels( src, 0 );

      string output_path = $"{Application.dataPath}/harry_t/ht8b_materials/procedural/{path}.png";

      File.WriteAllBytes( output_path, writetex.EncodeToPNG() );
   }

   public void RenderProcedural_8ball()
   {
      Color[] _spinmarker = this._src_ball_spinmarkers.GetPixels();
      Color[] _8ball_texture = this._src_ball_content.GetPixels();
      Image_ApplyTones_Mul( ref _8ball_texture, ref _8ball_texture, this._8ball_team_colour_0, this._8ball_team_colour_1, Color.black );
      Image_BlendAlphaOver( ref _8ball_texture, ref _spinmarker );

      WriteTex2dAsset( ref _8ball_texture, this._src_ball_content.width, this._src_ball_content.height, "tballs_8ball" );
   }

   public void RenderProcedural_9ball_4ball()
   {
      Color[] _spinmarker = this._src_ball_spinmarkers.GetPixels();
      Color[] _9ball_texture = this._src_ball_content.GetPixels();

      int box_width = this._src_ball_content.width >> 2;
      int box_height = this._src_ball_content.height >> 2;

      int cx = 1;
      int cy = 3;

      for( int i = 0; i < 9; i ++ )
      {
         Image_ColourSqr( ref _9ball_texture, 
         
            this._src_ball_content.width, this._src_ball_content.height,
            box_width*(cx ++), box_height*(cy),
            box_width, box_height,

            this._9ball_colours[ i ]
         );

         if( cx >= 4 )
         {
            cx = 0;
            cy --;
         }
      }

      // 4 ball section
      Image_FillSqr( ref _9ball_texture,
         
         this._src_ball_content.width, this._src_ball_content.height,
         box_width*0,0,
         box_width,box_height,

         this._4ball_team_colour_0
      );

      Image_FillSqr( ref _9ball_texture,
         
         this._src_ball_content.width, this._src_ball_content.height,
         box_width*1,0,
         box_width,box_height,

         this._4ball_team_colour_1
      );

      Image_FillSqr( ref _9ball_texture,
         
         this._src_ball_content.width, this._src_ball_content.height,
         box_width*2,0,
         box_width*2,box_height,

         this._4ball_objective_colour
      );

      Image_BlendAlphaOver( ref _9ball_texture, ref _spinmarker );
      WriteTex2dAsset( ref _9ball_texture, this._src_ball_content.width, this._src_ball_content.height, "tballs_9ball" );
   }

   public void RenderProceduralTextures()
   {
      if( !this._src_ball_content || !this._src_ball_spinmarkers )
      {
         Debug.LogError( "Missing some source content supplied for texture bake!!" );
         return;
      }

      RenderProcedural_8ball();
      RenderProcedural_9ball_4ball();

      AssetDatabase.Refresh();
   }

   private static void store_transform( Transform src, Transform dest )
   {
      dest.position = src.position;
      dest.rotation = src.rotation;
   }

   public void ApplyConfig( Transform root )
   {
      Transform table_folder = root.Find( "intl.table" );

      {
         List<GameObject> destroy_list = new List<GameObject>();
         foreach( Transform t in table_folder )
         {
            destroy_list.Add( t.gameObject );
         }

         for( int i = 0; i < destroy_list.Count; i ++ )
         {
            DestroyImmediate( destroy_list[i] );
         }
      }

      GameObject table_instance = (GameObject)PrefabUtility.InstantiatePrefab( this._table_setup_prefab, table_folder );
      table_instance.name = "table_artwork";

      // Apply transforms to other components
      Transform menu_transform = root.Find( "intl.ui_menu" );
      store_transform( table_instance.transform.Find( ".MENU" ), menu_transform );
      menu_transform.transform.position += menu_transform.right * 0.4f;

      store_transform( table_instance.transform.Find( ".CUE_0" ), root.Find( "intl.cue-0" ) );
      store_transform( table_instance.transform.Find( ".CUE_1" ), root.Find( "intl.cue-1" ) );
      
      // Table surface
      Transform gamespace = root.Find( "intl.balls" );
      gamespace.localPosition = table_instance.transform.Find( ".TABLE_SURFACE" ).localPosition + Vector3.up * 0.03f;
      gamespace.GetChild(0).GetComponent<MeshRenderer>().sharedMaterials[1].SetFloat( "_Floor", gamespace.position.y );

      // Player names
      Transform score_info_root = root.transform.Find( "intl.scorecardinfo" );
      score_info_root.Find( "player0-name" ).gameObject.GetComponent<RectTransform>().localPosition = table_instance.transform.Find( ".NAME_0" ).localPosition;
      score_info_root.Find( "player1-name" ).gameObject.GetComponent<RectTransform>().localPosition = table_instance.transform.Find( ".NAME_1" ).localPosition;

      int ht8b_layerid = LayerMask.NameToLayer( "ht8b" );
      
      // Override layers for desktop rendering
      table_instance.layer = ht8b_layerid;
      foreach( Transform t in table_instance.transform )
      {
         t.gameObject.layer = ht8b_layerid;
      }

      GameObject collision_instance = (GameObject)PrefabUtility.InstantiatePrefab( this._collision_data_prefab, table_instance.transform );
      collision_instance.name = "collision.vfx";
      collision_instance.SetActive( false );

      // Apply script values
      ht8b ht8b_script = root.gameObject.GetComponent<ht8b>();
      
      VRC.Udon.UdonBehaviour behaviour = UdonSharpEditor.UdonSharpEditorUtility.GetBackingUdonBehaviour( ht8b_script );

      // Collision data
      behaviour.publicVariables.TrySetVariableValue( "k_TABLE_WIDTH",      this.cdata.k_TABLE_WIDTH );

      behaviour.publicVariables.TrySetVariableValue( "k_TABLE_HEIGHT",     this.cdata.k_TABLE_HEIGHT );
      behaviour.publicVariables.TrySetVariableValue( "k_POCKET_RADIUS",    this.cdata.k_POCKET_RADIUS );
      behaviour.publicVariables.TrySetVariableValue( "k_CUSHION_RADIUS",   this.cdata.k_CUSHION_RADIUS );
      behaviour.publicVariables.TrySetVariableValue( "k_INNER_RADIUS",     this.cdata.k_INNER_RADIUS );

      // Pockets
      behaviour.publicVariables.TrySetVariableValue( "k_vE", this.cdata._cornerpocket );
      behaviour.publicVariables.TrySetVariableValue( "k_vF", this.cdata._sidepocket );

      // Global colours
      behaviour.publicVariables.TrySetVariableValue( "k_colour_foul",      this._colourFoul * 1.5f );
      behaviour.publicVariables.TrySetVariableValue( "k_colour_default",   this._colourDefault * 1.5f );

      behaviour.publicVariables.TrySetVariableValue( "k_teamColour_spots",   this._8ball_team_colour_0 * 1.5f );
      behaviour.publicVariables.TrySetVariableValue( "k_teamColour_stripes", this._8ball_team_colour_1 * 1.5f );
      
      behaviour.publicVariables.TrySetVariableValue( "k_colour4Ball_team_0", this._4ball_team_colour_0 * 1.5f );
      behaviour.publicVariables.TrySetVariableValue( "k_colour4Ball_team_1", this._4ball_team_colour_1 * 1.5f );

      behaviour.publicVariables.TrySetVariableValue( "k_fabricColour_8ball", this._8ball_fabric_colour );
      behaviour.publicVariables.TrySetVariableValue( "k_fabricColour_9ball", this._9ball_fabric_colour );
      behaviour.publicVariables.TrySetVariableValue( "k_fabricColour_4ball", this._4ball_fabric_colour );

      // Materials
      behaviour.publicVariables.TrySetVariableValue( "tableMaterial", this._table_surface_mat );
      behaviour.publicVariables.TrySetVariableValue( "ballMaterial", this._ball_mat );

      // Textures
      behaviour.publicVariables.TrySetVariableValue( "textureSets", new Texture[] 
      { 
         (Texture)AssetDatabase.LoadAssetAtPath( "Assets/harry_t/ht8b_materials/procedural/tballs_8ball.png", typeof(Texture) ),
         (Texture)AssetDatabase.LoadAssetAtPath( "Assets/harry_t/ht8b_materials/procedural/tballs_9ball.png", typeof(Texture) )
      } 
      );

      Undo.RecordObject( behaviour, "[ht8b] Modify Public Variables" );
      EditorSceneManager.MarkSceneDirty( behaviour.gameObject.scene );
      PrefabUtility.RecordPrefabInstancePropertyModifications( behaviour );
      
   }
   #endif
}