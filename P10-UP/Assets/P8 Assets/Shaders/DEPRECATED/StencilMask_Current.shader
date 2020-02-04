Shader "Stencils/Masks/StencilMask_Current"
{
	Properties{
		_Cube("Environment Map", Cube) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" "Queue" = "Geometry-100"}
		Stencil
		{
			Ref 0
			Comp Always
			
			ZFail Keep
			Pass replace
		}

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			// User-specified uniforms
			samplerCUBE _Cube;


			struct vertexInput {
				float4 vertex : POSITION;
				float3 texcoord : TEXCOORD0;
			};

			struct vertexOutput {
				float4 vertex : SV_POSITION;
				float3 texcoord : TEXCOORD0;
			};
			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			vertexOutput vert(vertexInput input)
			{
			   vertexOutput output;
			   output.vertex = UnityObjectToClipPos(input.vertex);
			   output.texcoord = input.texcoord;
			   return output;
			}

			//v2f vert(appdata v)
			//{
			//	v2f o;
			//	o.pos = UnityObjectToClipPos(v.vertex);
			//	return o;
			//}
			
			fixed4 frag(vertexOutput input) : COLOR
			{
			   return texCUBE(_Cube, input.texcoord);
			}

			//half4 frag(v2f i) : COLOR
			//{
			//	return half4(1,1,0,1);
			//}
		ENDCG
		}
	}
}