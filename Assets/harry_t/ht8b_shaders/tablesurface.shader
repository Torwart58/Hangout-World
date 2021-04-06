Shader "harry_t/StandardTintable"
{
   Properties
   {
      _EmissionColor ("Emission Colour", Color) = (1,1,1,1)
      _Color ("Tint Colour", Color) = (1,1,1,1)

      _MainTex ("Albedo (RGB), tintmap(A)", 2D) = "white" {}
      _EmissionMap ("Emission mask", 2D) = "black" {}
      _Metalic ("Metalic/Smoothness(A)", 2D) = "white" {}
   }
   SubShader
   {
      Tags { "RenderType"="Opaque" }
      LOD 200

      CGPROGRAM

      #pragma surface surf Standard fullforwardshadows vertex:vert
      #pragma target 3.0

      sampler2D _MainTex;
      sampler2D _EmissionMap;
      sampler2D _Metalic;

      fixed4 _EmissionColor;
      fixed3 _Color;

      struct Input
      {
         float2 uv_MainTex;
         float3 modelPos;
      };

      void vert ( inout appdata_full v, out Input o ) 
      {
         UNITY_INITIALIZE_OUTPUT(Input,o);
         o.modelPos = v.vertex.xyz;
      }

      void surf (Input IN, inout SurfaceOutputStandard o)
      {
         fixed4 sample_diffuse = tex2D (_MainTex, IN.uv_MainTex);
         fixed4 sample_emission = tex2D( _EmissionMap, IN.uv_MainTex );
         fixed4 sample_metalic = tex2D( _Metalic, IN.uv_MainTex );

         o.Albedo = lerp( sample_diffuse.rgb, _Color * sample_diffuse.rgb * 2.0, pow(sample_diffuse.a,0.1) );
         o.Metallic = sample_metalic.r;
         o.Smoothness = sample_metalic.a;
         o.Alpha = 1.0;
         o.Emission = sample_emission.r * _EmissionColor;
      }

      ENDCG
   }
   FallBack "Diffuse"
}
