Shader "harry_t/StandardTintable_Quest"
{
	Properties
	{
      _EmissionColor ("Emission Colour", Color) = (1,1,1,1)
      _Color ("Tint Colour", Color) = (1,1,1,1)

      _MainTex ("Albedo (RGB)", 2D) = "white" {}
      _EmissionMap ("Emission mask", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM

		#pragma surface surf Lambert noforwardadd

      sampler2D _MainTex;
      sampler2D _EmissionMap;

      fixed4 _EmissionColor;
      fixed3 _Color;

		struct Input
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
         fixed4 sample_diffuse = tex2D (_MainTex, IN.uv_MainTex);
         fixed4 sample_emission = tex2D( _EmissionMap, IN.uv_MainTex );

			o.Albedo = lerp( sample_diffuse.rgb, _Color * sample_diffuse.rgb * 2.0, pow(sample_diffuse.a,0.1) );
			o.Emission = sample_emission.r * _EmissionColor;
		}

		ENDCG
	}
	FallBack "Diffuse"
}