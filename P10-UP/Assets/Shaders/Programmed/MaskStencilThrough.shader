Shader "Stencils/MaskThrough"
{
    Properties
    {
		_StencilValue("Stencil Value", Range(0,255)) = 1
		_StencilReadMask("Stencil ReadMask", Range(0,255)) = 1
    }
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry+400"}
		ColorMask 0
		Stencil
		{
			Ref [_StencilValue]
			ReadMask[_StencilReadMask]
			Comp Less
			Pass Replace
		}

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD1;
			};

			int _StencilValue;
			float4x4 _WorldToPortal;

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
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
				return half4(1,1,0,1);
			}
		ENDCG
		}
	}
}