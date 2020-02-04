Shader "Stencils/Materials/StencilEmitMatCurrent"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
		_Emission("Color", Color) = (0,0,0)
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

        struct Input
        {
            float2 uv_MainTex;
			float2 uv_BumpMap;
        };

		fixed4 _Color;
		fixed3 _Emission;

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
			o.Emission = _Emission;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack off
}
