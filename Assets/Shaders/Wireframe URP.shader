Shader "Custom/URP/Wireframe"
{
    Properties
    {
        _WireColor ("Wire Color", Color) = (0,0,0,1)
        _FillColor ("Fill Color", Color) = (0.8,0.8,0.8,1)
        _WireWidth ("Wire Width", Range(0, 0.1)) = 0.02
    }
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline"
        }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags
            {
                "LightMode"="UniversalForward"
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma target 4.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct VertexToGeometry
            {
                float4 positionCS : SV_POSITION;
                float3 bary : TEXCOORD0;
            };

            struct GeometryToFragment
            {
                float4 positionCS : SV_POSITION;
                float3 bary : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _WireColor;
                float4 _FillColor;
                float _WireWidth;
            CBUFFER_END

            VertexToGeometry vert(Attributes input)
            {
                VertexToGeometry output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.bary = float3(1, 0, 0);
                return output;
            }

            [maxvertexcount(3)]
            void geom(triangle VertexToGeometry input[3], inout TriangleStream<GeometryToFragment> stream)
            {
                GeometryToFragment output;
                for (int i = 0; i < 3; i++)
                {
                    output.positionCS = input[i].positionCS;

                    output.bary = float3(i == 0 ? 1 : 0, i == 1 ? 1 : 0, i == 2 ? 1 : 0);
                    stream.Append(output);
                }
                stream.RestartStrip();
            }

            half4 frag(GeometryToFragment input) : SV_Target
            {
                float minBary = min(input.bary.x, min(input.bary.y, input.bary.z));
                if (minBary < _WireWidth)
                    return _WireColor;
                else
                    return _FillColor;
            }
            ENDHLSL
        }
    }
}