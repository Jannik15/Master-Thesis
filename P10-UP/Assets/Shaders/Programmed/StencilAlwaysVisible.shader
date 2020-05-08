Shader "Stencils/AlwaysVisible"
{
    Properties
    {
        _MainColor("Color", Color) = (0,0,0,1)
        _MainTex("Texture", 2D) = "black" {}
    }
        SubShader
    {
        Tags { "RenderType" = "Opaque"  "Queue" = "Transparent+2000" }
        Stencil
        {
            Comp Always
            Pass Keep
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct vertexInput
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _MainColor;
            float4 _MainTex_ST;

            v2f vert(vertexInput v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, IN.uv);
                col.rgb += _MainColor.rgb;

                return col;
            }
            ENDCG
        }
    }
}
