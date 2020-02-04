Shader "Stencils/Materials/StencilMatCurrent" {
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
	}
	
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
		Stencil
		{
			Ref 0
			Comp Equal
			Pass keep
			Fail keep
		}
		CGPROGRAM
		// Physically based Standard lighting model, and disable all shadowcasting because of overlapping architecture
		#pragma surface surf Lambert noshadow

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _BumpMap;
		fixed4 _Color;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			o.Alpha = c.a;
		}
		ENDCG
	}
	Fallback Off
}
