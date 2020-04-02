Shader "Terrain/HeightmapSDF"
{
    Properties
    {
        _InsideColor("Inside Color", Color) = (.5, 0, 0, 1)
        _OutsideColor("Outside Color", Color) = (0, .5, 0, 1)
        
        _LineDistance("Mayor Line Distance", Range(0, 2)) = 1
        _LineThickness("Mayor Line Thiccness", Range(0, 0.1)) = 0.05
        
        [IntRange]_SubLines("Lines between major lines", Range(1, 10)) = 4
        _SubLineThickness("Thickness of inbetween lines", Range(0, 0.05)) = 0.01
    }
    
    SubShader
    {
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags{ "RenderType"="Opaque" "Queue"="Geometry"}

        Pass
        {
            CGPROGRAM

            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float4 worldPos : TEXCOORD0;
            };
            
            float4 _InsideColor;
            float4 _OutsideColor;
            
            float _LineDistance;
            float _LineThickness;
            
            float _SubLines;
            float _SubLineThickness;

            v2f vert(appdata v)
            {
                v2f o;
                //calculate the position in clip space to render the object
                o.position = UnityObjectToClipPos(v.vertex);
                //calculate world position of vertex
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                return o;
            }
            
            float2 translate(float2 samplePosition, float2 offset){
                return samplePosition - offset;
            }
            
            float2 rotate(float2 samplePosition, float rotation){
                const float PI = 3.14159;
                float angle = rotation * PI * 2 * -1;
                float sine, cosine;
                sincos(angle, sine, cosine);
                return float2(cosine * samplePosition.x + sine * samplePosition.y, cosine * samplePosition.y - sine * samplePosition.x);
            }
            
            float merge(float shape1, float shape2){
                return min(shape1, shape2);
            }
            
            float circle(float2 samplePosition, float radius){
                return length(samplePosition) - radius;
            }
            
            float rectangle(float2 samplePosition, float2 halfSize){
                float2 componentWiseEdgeDistance = abs(samplePosition) - halfSize;
                float outsideDistance = length(max(componentWiseEdgeDistance, 0));
                float insideDistance = min(max(componentWiseEdgeDistance.x, componentWiseEdgeDistance.y), 0);
                return outsideDistance + insideDistance;
            }

            float scene(float2 position) {
                const float PI = 3.14159;

                float2 squarePosition = position;
                squarePosition = translate(squarePosition, float2(1, 0));
                squarePosition = rotate(squarePosition, .125);
                float squareShape = rectangle(squarePosition, float2(2, 2));

                float2 circlePosition = position;
                circlePosition = translate(circlePosition, float2(-2.5, 0));
                float circleShape = circle(circlePosition, 2.5);

                float combination = merge(circleShape, squareShape);

                return combination;
            }

            fixed4 frag(v2f i) : SV_TARGET
            {
                float dist = scene(i.worldPos.xz);
                fixed4 col = lerp(_InsideColor, _OutsideColor, step(0, dist));

                float distanceChange = fwidth(dist) * 0.5;
                float majorLineDistance = abs(frac(dist / _LineDistance + 0.5) - 0.5) * _LineDistance;
                float majorLines = smoothstep(_LineThickness - distanceChange, _LineThickness + distanceChange, majorLineDistance);

                float distanceBetweenSubLines = _LineDistance / _SubLines;
                float subLineDistance = abs(frac(dist / distanceBetweenSubLines + 0.5) - 0.5) * distanceBetweenSubLines;
                float subLines = smoothstep(_SubLineThickness - distanceChange, _SubLineThickness + distanceChange, subLineDistance);

                return col * majorLines * subLines;
            }

            ENDCG
        }
    }
    FallBack "Standard" //fallback adds a shadow pass so we get shadows on other objects
}