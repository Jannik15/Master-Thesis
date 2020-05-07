Shader "Stencils/StencilEmission"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MainColor("Color", Color) = (0,0,0,1)
        [Toggle(USE_EMISSION)] _UseEmission("Use Emission", Float) = 0
        _Emission("Emission", Color) = (0,0,0,1)
        _StencilValue("Stencil Value", Range(0,255)) = 0
    }
    SubShader
    {
        Tags {"RenderType"="Opaque" "Queue" = "Geometry"}
        LOD 100
        Stencil
        {
            Ref[_StencilValue]
            Comp Equal
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature USE_EMISSION
            #include "UnityCG.cginc"        
            #include "UnityStandardBRDF.cginc" // for shader lighting info and some utils
            #include "UnityStandardUtils.cginc" // for energy conservation

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL; 
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float3 normal : TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _MainColor;
            fixed4 _Emission;
            int _StencilValue;
            float4x4 _WorldToPortal;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.normal = v.normal; //get normal for frag shader from vert info
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float3 lightDir = _WorldSpaceLightPos0.xyz;
                float3 viewDir = normalize(_WorldSpaceCameraPos - i.worldPos);
                float3 lightColor = _LightColor0.rgb;
                col.rgb *= lightColor * DotClamped(lightDir, i.normal); // apply diffuse light by angle of incidence
#ifdef USE_EMISSION
                col.rgb += _Emission.rgb;
#endif
                // Discard geometry based on z axis proximity, but not when camera is close enough to the portal
                if (_StencilValue > 0) {
                    if (mul(_WorldToPortal, float4(_WorldSpaceCameraPos, 1.0)).z > 0.1)
                    {
                        if (mul(_WorldToPortal, float4(i.worldPos, 1.0)).z > 0.11)
                            discard;
                    }
                    else if (mul(_WorldToPortal, float4(_WorldSpaceCameraPos, 1.0)).z < -0.1)
                    {
                        if (mul(_WorldToPortal, float4(i.worldPos, 1.0)).z < -0.11)
                            discard;
                    }
                }
                return col;
            }
            ENDCG
        }
    }
}
