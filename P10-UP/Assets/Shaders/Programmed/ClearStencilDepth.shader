/*
This code is a simple way to clear the depth buffer, taken from: https://en.wikibooks.org/wiki/Cg_Programming/Unity/Portals
It is also combined with Unity's Skybox shader
*/

Shader "Stencils/ClearDepth"
{
	Properties{
		_Cube("Environment Map", Cube) = "white" {}
		_StencilValue("Stencil Value", Range(0,255)) = 1
		//_StencilMask("Stencil Mask", Range(0,255)) = 1
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry+200" }

		Pass
	{
		ZTest Always // always pass depth test (nothing occludes this material) 
		Cull Off // turn off backface culling

	Stencil{
		Ref [_StencilValue]
		//ReadMask [_StencilMask]
		Comp Equal // only pass stencil test if stencil value equals Ref
	}

		CGPROGRAM

#pragma vertex vert  
#pragma fragment frag

		uniform samplerCUBE _Cube;

	struct vertexInput {
		float4 vertex : POSITION;
	};
	struct vertexOutput {
		float4 pos : SV_POSITION;
		float3 texDir : TEXCOORD0;
	};

	vertexOutput vert(vertexInput input)
	{
		vertexOutput output;

		output.texDir = input.vertex;
		output.pos = UnityObjectToClipPos(input.vertex);
		return output;
	}

	float4 frag(vertexOutput input) : COLOR
	{
		return texCUBE(_Cube, input.texDir);
	}

		ENDCG
	}
	}
}