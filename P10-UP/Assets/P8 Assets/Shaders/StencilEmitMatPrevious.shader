Shader "Stencils/Materials/StencilEmitMatPrevious" {
	Properties
	{
		_Color("Main Color", Color) = (1,1,1,1)
		_MainTex("Base (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		_Emission("Color", Color) = (0,0,0)
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry+300" }
		Stencil
		{
			Ref 1
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
		float4x4 _WorldToPortal;
		fixed4 _Color;
		fixed3 _Emission;

		struct Input {
			float2 uv_MainTex;
			float2 uv_BumpMap;
			float3 worldPos;
		};

		void surf(Input IN, inout SurfaceOutput o) {
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
			o.Emission = _Emission;
			o.Alpha = c.a;

			// Discard geometry based on z axis proximity, but not when camera is close enough to the portal
			if (mul(_WorldToPortal, float4(_WorldSpaceCameraPos, 1.0)).z > 0.2) {
				if (mul(_WorldToPortal, float4(IN.worldPos, 1.0)).z > 0.21)
					discard;
			}
			else if (mul(_WorldToPortal, float4(_WorldSpaceCameraPos, 1.0)).z < -0.2) {
				if (mul(_WorldToPortal, float4(IN.worldPos, 1.0)).z < -0.21)
					discard;
			}
		}
		ENDCG
	}
	Fallback Off
}
