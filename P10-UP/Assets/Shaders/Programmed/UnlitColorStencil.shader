Shader "Stencils/UnlitColor"
{
    Properties
    {
		_MainColor("Color", Color) = (0,0,0,1)
		_StencilValue("Stencil Value", Range(0,255)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque"  "Queue"="Geometry" }
        //LOD 100
		Stencil
		{
			Ref [_StencilValue]
			Comp Equal
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
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
				float3 worldPos : TEXCOORD1;
            };

			fixed4 _MainColor;
			int _StencilValue;
			float4x4 _WorldToPortal;

            v2f vert (vertexInput v)
            {
                v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                UNITY_TRANSFER_FOG(o,o.vertex);
				
                return o;
            }

            fixed4 frag (v2f IN) : SV_Target
            {
                // apply fog
                UNITY_APPLY_FOG(IN.fogCoord, _MainColor);

				// Discard geometry based on z axis proximity, but not when camera is close enough to the portal
				if (_StencilValue > 0) {
					if (mul(_WorldToPortal, float4(_WorldSpaceCameraPos, 1.0)).z > 0.1)
					{
						if (mul(_WorldToPortal, float4(IN.worldPos, 1.0)).z > 0.11)
							discard;
					}
					else if (mul(_WorldToPortal, float4(_WorldSpaceCameraPos, 1.0)).z < -0.1)
					{
						if (mul(_WorldToPortal, float4(IN.worldPos, 1.0)).z < -0.11)
							discard;
					}
				}

                return _MainColor;
            }
            ENDCG
        }
    }
}
