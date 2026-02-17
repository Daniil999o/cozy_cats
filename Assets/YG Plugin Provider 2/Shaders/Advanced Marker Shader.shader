Shader "Mobile/Advanced Marker Shader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Scale ("Base Scale (Overall)", Float) = 1.0
        
        [Header(Size Limits)]
        _MinSize ("Min Screen Size", Float) = 0.5
        _MaxSize ("Max Screen Size", Float) = 10.0
        
        [Header(Distance Scaling)]
        _DistSensitivity ("Distance Sensitivity", Float) = 0.1
        
        [Header(Visibility)]
        _MaxDist ("Max Visible Distance", Float) = 100.0
        _FadeRange ("Fade Out Range", Float) = 10.0

        [Header(Toggles)]
        [Toggle(BILLBOARD_ON)] _UseBillboard ("Always Face Camera", Float) = 1
        [Toggle(FIXED_SIZE_ON)] _UseFixedSize ("Keep Screen Size", Float) = 1
    }

    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent+100" 
            "IgnoreProjector"="True" 
            "RenderType"="Transparent" 
            "DisableBatching"="True" 
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always 
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local __ BILLBOARD_ON
            #pragma multi_compile_local __ FIXED_SIZE_ON
            
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color    : COLOR;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color    : COLOR;
                fixed alphaFade : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Scale;
            float _MinSize;
            float _MaxSize;
            float _DistSensitivity;
            float _MaxDist;
            float _FadeRange;

            v2f vert(appdata_t v)
            {
                v2f o;
                
                float3 worldOrigin = mul(unity_ObjectToWorld, float4(0,0,0,1)).xyz;
                float dist = distance(worldOrigin, _WorldSpaceCameraPos);
                
                // --- ЛОГИКА ВИДИМОСТИ ---
                float visibility = 1.0 - smoothstep(_MaxDist - _FadeRange, _MaxDist, dist);
                
                // --- ЛОГИКА РАЗМЕРА ---
                float currentScale = _Scale;
                
                #if FIXED_SIZE_ON
                    // Используем _DistSensitivity вместо жесткого 0.1
                    // Это определяет, какой "вес" имеет дистанция в формуле масштаба
                    currentScale *= (dist * _DistSensitivity);
                #endif

                // Ограничиваем размер (включая влияние Scale и Sensitivity)
                currentScale = clamp(currentScale, _MinSize, _MaxSize);
                
                // Умножаем на видимость (если дальше MaxDist, размер станет 0)
                currentScale *= visibility;

                #if BILLBOARD_ON
                    float3 viewPos = mul(UNITY_MATRIX_V, float4(worldOrigin, 1.0)).xyz;
                    
                    // Берем масштаб из Transform
                    float2 objScale = float2(
                        length(unity_ObjectToWorld[0].xyz),
                        length(unity_ObjectToWorld[1].xyz)
                    );
                    
                    viewPos.xy += v.vertex.xy * objScale * currentScale;
                    o.vertex = mul(UNITY_MATRIX_P, float4(viewPos, 1.0));
                #else
                    float3 localPos = v.vertex.xyz * currentScale;
                    float3 worldPos = worldOrigin + mul((float3x3)unity_ObjectToWorld, localPos);
                    o.vertex = mul(UNITY_MATRIX_VP, float4(worldPos, 1.0));
                #endif

                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                o.alphaFade = visibility;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, i.texcoord) * i.color;
                c.a *= i.alphaFade;
                clip(c.a - 0.01); 
                return c;
            }
            ENDCG
        }
    }
}