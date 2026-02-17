Shader "YG Plugin Provider/Mobile Water"
{
    Properties
    {
        _MainTex ("Water Texture", 2D) = "white" {}
        _UseTexture ("Use Texture (0=Color, 1=Texture)", Range(0, 1)) = 0

        _ShallowColor ("Shallow Water Color", Color) = (0.0, 0.8, 1.0, 0.8)
        _DeepColor ("Deep Water Color", Color) = (0.0, 0.3, 0.6, 1.0)
        _GradientPower ("Gradient Power", Range(0.1, 5.0)) = 1.0

        _WaveSpeed ("Wave Speed", Range(0.0, 2.0)) = 0.5
        _WaveScale ("Wave Scale", Range(0.0, 0.1)) = 0.02
        _WaveFrequency ("Wave Frequency", Range(1.0, 10.0)) = 3.0
        _WaveDirection ("Wave Direction", Vector) = (1,1,0)

        _Transparency ("Transparency", Range(0.0, 1.0)) = 0.8
        _Glossiness ("Glossiness", Range(0.0, 1.0)) = 0.5
        _NormalStrength ("Normal Strength", Range(0.0, 1.0)) = 0.5
        _NormalMap ("Normal Map", 2D) = "bump" {} 

    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "IgnoreProjector"="True" 
        }

        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off 

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 tangent : TANGENT;
                float3 normal : NORMAL; // Added normal
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD2;
                float3 worldNormal : TEXCOORD3;
                float3 worldTangent : TEXCOORD4;
                float3 worldBitangent : TEXCOORD5;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NormalMap;
            float _NormalStrength;
            float3 _WaveDirection;
            fixed4 _ShallowColor;
            fixed4 _DeepColor;
            float _GradientPower;
            float _UseTexture;

            float _WaveSpeed;
            float _WaveScale;
            float _WaveFrequency;
            float _Transparency;
            float _Glossiness;

            v2f vert(appdata v)
            {
                v2f o;

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldNormal = UnityObjectToWorldNormal(v.normal); // Now v.normal is defined
                o.worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.worldBitangent = cross(o.worldNormal, o.worldTangent) * v.tangent.w;

                float wave = sin(dot(_WaveDirection.xz, v.vertex.xz) * _WaveFrequency + _Time.y * _WaveSpeed) * _WaveScale;
                v.vertex.y += wave;
                o.vertex = UnityObjectToClipPos(v.vertex);

                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col;
                if (_UseTexture > 0.5)
                {
                    col = tex2D(_MainTex, i.uv);

                    float2 animatedUV = i.uv + float2(_Time.x * 0.02, _Time.x * 0.01);
                    fixed4 animatedTex = tex2D(_MainTex, animatedUV);
                    col = lerp(col, animatedTex, 0.3);
                }
                else
                {
                    float gradient = pow(i.uv.y, _GradientPower);
                    col = lerp(_ShallowColor, _DeepColor, gradient);
                }

                fixed3 tangentNormal = UnpackNormal(tex2D(_NormalMap, i.uv));
                fixed3 worldNormal = normalize(tangentNormal.x * i.worldTangent + tangentNormal.y * i.worldBitangent + tangentNormal.z * i.worldNormal);
                worldNormal = lerp(i.worldNormal, worldNormal, _NormalStrength);

                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float gloss = dot(lightDir, worldNormal) * _Glossiness;
                col.rgb += gloss;

                col.a *= _Transparency;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }

    FallBack "Mobile/Diffuse"
}