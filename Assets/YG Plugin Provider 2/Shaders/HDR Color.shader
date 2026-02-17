Shader "YG Plugin Provider/HDR Color"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _Intensity ("Intensity", Range(0, 100)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            float4 _Color;
            float4 _EmissionColor;
            float _Intensity;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float intensityNormalized = _Intensity / 100.0;
                float3 finalColor = _Color.rgb * intensityNormalized; // Цвет объекта умножается на интенсивность
                float3 emission = _EmissionColor.rgb * intensityNormalized; // Эмиссия умножается на интенсивность
                return fixed4(finalColor + emission, _Color.a); // Суммируем цвет объекта и эмиссию
            }
            ENDCG
        }
    }
}