// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see unity_mit_license.txt)

Shader "Hidden/BoxClipBaked/2e15c8851a9a96f429bbfa61eaeef332Inst/Standard" {
CGINCLUDE
    #define BOXCLIP_CONFIGURED 1
    #define BOXCLIP_SCALE 1
    #define BOXCLIP_ALLOW_IN_FRONT 0
    #define BOXCLIP_ALLOW_IN_FRONT 0

    #define boxQuad_ClipShow_Count 4
    #define DECLARE_BOXCLIP_ClipShow_ARRAY static internalBoxQuad boxQuad_ClipShow_Quads[4] = {\
        {float3(6.500001,-1.3,-3.750001),float4(1,0,-1.192093E-07,0.001),float4(0,1,0,0.5),float4(1.192093E-07,0,1,0.7692307)},\
        {float3(5.099999,-1.3,-2.450001),float4(-1,0,1.192093E-07,0.001),float4(0,1,0,0.5),float4(-1.192093E-07,0,-1,0.7692307)},\
        {float3(6.450002,-1.3,-2.400001),float4(-3.330675E-16,0,1,0.001),float4(0,1,0,0.5),float4(-1,0,-3.279435E-15,0.7692301)},\
        {float3(5.199998,-1.35,-3.800001),float4(-2.384186E-07,0,-1,0.001),float4(0,1,0,0.5),float4(1,0,-2.384186E-07,0.7692301)}\
};

    #define boxQuad_ClipHide_Count 0
    #define DECLARE_BOXCLIP_ClipHide_ARRAY static internalBoxQuad boxQuad_ClipHide_Quads[1] = {\
        {float3(0,0,0),float4(0,0,0,0),float4(0,0,0,0),float4(0,0,0,0)},\
};

    #define boxQuad_ShowVolume_Count 0
    #define DECLARE_BOXCLIP_ShowVolume_ARRAY static internalBoxQuad boxQuad_ShowVolume_Quads[1] = {\
        {float3(0,0,0),float4(0,0,0,0),float4(0,0,0,0),float4(0,0,0,0)},\
};

    #define boxQuad_HideVolume_Count 0
    #define DECLARE_BOXCLIP_HideVolume_ARRAY static internalBoxQuad boxQuad_HideVolume_Quads[1] = {\
        {float3(0,0,0),float4(0,0,0,0),float4(0,0,0,0),float4(0,0,0,0)},\
};

    #define boxQuad_ShowCameraWithin_Count 0
    #define DECLARE_BOXCLIP_ShowCameraWithin_ARRAY static internalBoxQuad boxQuad_ShowCameraWithin_Quads[1] = {\
        {float3(0,0,0),float4(0,0,0,0),float4(0,0,0,0),float4(0,0,0,0)},\
};

    #define boxQuad_HideCameraWithin_Count 0
    #define DECLARE_BOXCLIP_HideCameraWithin_ARRAY static internalBoxQuad boxQuad_HideCameraWithin_Quads[1] = {\
        {float3(0,0,0),float4(0,0,0,0),float4(0,0,0,0),float4(0,0,0,0)},\
};

    #define boxQuad_ZCompress_Count 4
    #define DECLARE_BOXCLIP_ZCompress_ARRAY static internalBoxQuad boxQuad_ZCompress_Quads[4] = {\
        {float3(6.500001,-1.3,-3.750001),float4(1,0,-1.192093E-07,0.001),float4(0,1,0,0.5),float4(1.192093E-07,0,1,0.7692307)},\
        {float3(5.099999,-1.3,-2.450001),float4(-1,0,1.192093E-07,0.001),float4(0,1,0,0.5),float4(-1.192093E-07,0,-1,0.7692307)},\
        {float3(6.450002,-1.3,-2.400001),float4(-3.330675E-16,0,1,0.001),float4(0,1,0,0.5),float4(-1,0,-3.279439E-15,0.7692298)},\
        {float3(5.199998,-1.35,-3.800001),float4(-2.384186E-07,0,-1,0.001),float4(0,1,0,0.5),float4(1,0,-2.384186E-07,0.7692298)}\
};


ENDCG

    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo", 2D) = "white" {}

        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        _Glossiness("Smoothness", Range(0.0, 1.0)) = 0.5
        _GlossMapScale("Smoothness Scale", Range(0.0, 1.0)) = 1.0
        [Enum(Metallic Alpha,0,Albedo Alpha,1)] _SmoothnessTextureChannel ("Smoothness texture channel", Float) = 0

        [Gamma] _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _MetallicGlossMap("Metallic", 2D) = "white" {}

        [ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
        [ToggleOff] _GlossyReflections("Glossy Reflections", Float) = 1.0

        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _Parallax ("Height Scale", Range (0.005, 0.08)) = 0.02
        _ParallaxMap ("Height Map", 2D) = "black" {}

        _OcclusionStrength("Strength", Range(0.0, 1.0)) = 1.0
        _OcclusionMap("Occlusion", 2D) = "white" {}

        _EmissionColor("Color", Color) = (0,0,0)
        _EmissionMap("Emission", 2D) = "white" {}

        _DetailMask("Detail Mask", 2D) = "white" {}

        _DetailAlbedoMap("Detail Albedo x2", 2D) = "grey" {}
        _DetailNormalMapScale("Scale", Float) = 1.0
        _DetailNormalMap("Normal Map", 2D) = "bump" {}

        [Enum(UV0,0,UV1,1)] _UVSec ("UV Set for secondary textures", Float) = 0


        // Blending state
        [HideInInspector] _Mode ("__mode", Float) = 0.0
        [HideInInspector] _SrcBlend ("__src", Float) = 1.0
        [HideInInspector] _DstBlend ("__dst", Float) = 0.0
        [HideInInspector] _ZWrite ("__zw", Float) = 1.0
    }

    CGINCLUDE
    #include "UnityCG.cginc"
        #include "BoxClipTemplate.cginc"
    ENDCG

    CGINCLUDE
        #define UNITY_SETUP_BRDF_INPUT MetallicSetup
    ENDCG

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" "DisableBatching"="True" }
        LOD 300

Cull Off
        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            // #pragma vertex vertBase
            // #pragma fragment fragBase
            #include "UnityStandardCoreForward.cginc"

            #pragma require geometry
            #pragma vertex boxVert
            #pragma geometry boxGeom
            #pragma fragment boxFrag
            BOXCLIP_ALL_SHADERS_WORLDPOS(fragBase, VertexOutputForwardBase, vertBase, VertexInput, IN_WORLDPOS(i))

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            // #pragma vertex vertAdd
            // #pragma fragment fragAdd
            #include "UnityStandardCoreForward.cginc"

            #pragma require geometry
            #pragma vertex boxVert
            #pragma geometry boxGeom
            #pragma fragment boxFrag
            BOXCLIP_ALL_SHADERS_WORLDPOS(fragAdd, VertexOutputForwardAdd, vertAdd, VertexInput, i.posWorld)

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------


            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _PARALLAXMAP
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            // #pragma vertex vertShadowCaster
            // #pragma fragment fragShadowCaster

            // #include "UnityStandardShadow.cginc"

            #include "BoxClipStandardShadow.cginc"

            #pragma require geometry
            #pragma vertex boxVert
            #pragma fragment boxFrag
            #define geometry boxGeom

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Deferred pass
        Pass
        {
            Name "DEFERRED"
            Tags { "LightMode" = "Deferred" }

            CGPROGRAM
            #pragma target 3.0
            #pragma exclude_renderers nomrt


            // -------------------------------------

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature _PARALLAXMAP

            #pragma multi_compile_prepassfinal
            #pragma multi_compile_instancing
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertDeferred
            #pragma fragment fragDeferred

            #include "UnityStandardCore.cginc"

            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "PerformanceChecks"="False" "DisableBatching"="True" }
        LOD 150

        // ------------------------------------------------------------------
        //  Base forward pass (directional light, emission, lightmaps, ...)
        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature _ _GLOSSYREFLECTIONS_OFF
            // SM2.0: NOT SUPPORTED shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP

            #pragma skip_variants SHADOWS_SOFT DIRLIGHTMAP_COMBINED

            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog

            // #pragma vertex vertBase
            // #pragma fragment fragBase
            #include "UnityStandardCoreForward.cginc"

            #pragma vertex boxVert
            #pragma fragment boxFrag
            BOXCLIP_ALL_SHADERS_WORLDPOS(fragBase, VertexOutputForwardBase, vertBase, VertexInput, IN_WORLDPOS(i))

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Additive forward pass (one light per pass)
        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend [_SrcBlend] One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature _ _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature ___ _DETAIL_MULX2
            // SM2.0: NOT SUPPORTED shader_feature _PARALLAXMAP
            #pragma skip_variants SHADOWS_SOFT

            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog

            // #pragma vertex vertAdd
            // #pragma fragment fragAdd
            #include "UnityStandardCoreForward.cginc"

            #pragma vertex boxVert
            #pragma fragment boxFrag
            BOXCLIP_ALL_SHADERS_WORLDPOS(fragAdd, VertexOutputForwardAdd, vertAdd, VertexInput, i.posWorld)

            ENDCG
        }
        // ------------------------------------------------------------------
        //  Shadow rendering pass
        Pass {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On ZTest LEqual

            CGPROGRAM
            #pragma target 2.0

            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma skip_variants SHADOWS_SOFT
            #pragma multi_compile_shadowcaster

            // #pragma vertex vertShadowCaster
            // #pragma fragment fragShadowCaster

            // #include "UnityStandardShadow.cginc"

            #include "BoxClipStandardShadow.cginc"

            #pragma vertex boxVert
            #pragma fragment boxFrag

            ENDCG
        }

        // ------------------------------------------------------------------
        // Extracts information for lightmapping, GI (emission, albedo, ...)
        // This pass it not used during regular rendering.
        Pass
        {
            Name "META"
            Tags { "LightMode"="Meta" }

            Cull Off

            CGPROGRAM
            #pragma vertex vert_meta
            #pragma fragment frag_meta

            #pragma shader_feature _EMISSION
            #pragma shader_feature _METALLICGLOSSMAP
            #pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature ___ _DETAIL_MULX2
            #pragma shader_feature EDITOR_VISUALIZATION

            #include "UnityStandardMeta.cginc"
            ENDCG
        }
    }


    FallBack "VertexLit"
    CustomEditor "StandardShaderGUI"
}
