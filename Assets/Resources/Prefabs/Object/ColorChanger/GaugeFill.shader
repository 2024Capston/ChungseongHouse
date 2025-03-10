Shader "Custom/GaugeFill"
{
    Properties
    {
        _BackgroundColor ("Background Color", Color) = (0, 0, 0, 1)
        _FillColor ("Fill Color", Color) = (1, 0, 0, 1)
        _FillAmount ("Fill Amount", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _BackgroundColor;
            fixed4 _FillColor;
            float _FillAmount;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float fill = step(i.uv.x, _FillAmount);
                fixed4 color = lerp(_BackgroundColor, _FillColor, fill);
                return color;
            }
            ENDCG
        }
    }
}