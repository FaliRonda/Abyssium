Shader "Custom/CannonLight"
{
    Properties
    {
        _Color ("Light Color", Color) = (0,0,1,1)
        _Intensity ("Light Intensity", Range(0, 10)) = 1.0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" }
        
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

            half4 _Color;
            half _Intensity;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half4 col = _Color * _Intensity;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
