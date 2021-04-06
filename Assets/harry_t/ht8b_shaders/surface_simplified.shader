Shader "harry_t/surface_simplified"
{
	Properties
	{
		_EmissionColor ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_EmissionMap ("Emission mask", 2D) = "black" {}
		_Color ("Cloth Colour", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM

		#pragma surface surf Lambert noforwardadd

		sampler2D _MainTex;
		sampler2D _EmissionMap;

		struct Input
		{
			float2 uv_MainTex;
		};

		fixed4 _EmissionColor;
		fixed3 _Color;

		void surf (Input IN, inout SurfaceOutput o)
		{
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 stuff = tex2D( _EmissionMap, IN.uv_MainTex );
			o.Albedo = c.rgb * _Color;
			o.Emission = stuff.r * _EmissionColor;
		}

		ENDCG
	}
	FallBack "Diffuse"
}