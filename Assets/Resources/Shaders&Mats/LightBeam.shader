Shader "Custom/LightBeam"
{
    Properties
    {
        _Color ("Beam Color", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Range(0, 10)) = 1.0
        _Transparency ("Transparency", Range(0, 1)) = 1.0
        _Width ("Beam Width", Range(0, 10)) = 0.1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            float4 _Color;
            half _Intensity;
            half _Transparency;
            half _Width;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = _Color * _Intensity;
                col.a = _Transparency;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
