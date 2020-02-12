Shader "Stencils/Unlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_StencilValue("Stencil Value", Range(0,255)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"  "Queue"="Geometry+300" }
        LOD 100
		Stencil
		{
			Ref [_StencilValue]
			Comp Equal
			Pass keep
			Fail keep
		}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct vertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
			int _StencilValue;
            float4 _MainTex_ST;
			float4x4 _WorldToPortal;

            v2f vert (vertexInput v)
            {
                v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
				
                return o;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, IN.uv);
                // apply fog
                UNITY_APPLY_FOG(IN.fogCoord, col);

				// Discard geometry based on z axis proximity, but not when camera is close enough to the portal
				if (_StencilValue > 0) {
					if (mul(_WorldToPortal, float4(_WorldSpaceCameraPos, 1.0)).z > 0.2)
					{
						if (mul(_WorldToPortal, float4(IN.worldPos, 1.0)).z > 0.21)
							discard;
					}
					else if (mul(_WorldToPortal, float4(_WorldSpaceCameraPos, 1.0)).z < -0.2)
					{
						if (mul(_WorldToPortal, float4(IN.worldPos, 1.0)).z < -0.21)
							discard;
					}
				}

                return col;
            }
            ENDCG
        }
    }
}
