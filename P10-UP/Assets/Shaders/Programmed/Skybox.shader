// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

/*
This code is a simple way to clear the depth buffer, taken from: https://en.wikibooks.org/wiki/Cg_Programming/Unity/Portals
It is also combined with Unity's Skybox shader
*/

Shader "Stencils/Skybox"
{
	Properties{
		_Cube("Environment Map", Cube) = "white" {}
	}
		SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }

		Pass
	{
		Cull off // turn off backface culling

		Stencil{
		Ref 0
		Comp Equal // only pass stencil test if stencil value equals 1
		Fail Keep // do not change stencil value if stencil test fails
		ZFail Keep // do not change stencil value if stencil test passes but depth test fails
		Pass Keep // keep stencil value if stencil test passes
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