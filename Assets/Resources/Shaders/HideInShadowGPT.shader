Shader "Custom/HideOnSpotlight"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _LightPosition ("Light Position", Vector) = (0, 0, 0, 0)
        _LightDirection ("Light Direction", Vector) = (0, 0, -1, 0)
        _LightConeAngle ("Light Cone Angle", Range(1, 179)) = 45
    }

    SubShader
    {
        Tags { "Queue"="Transparent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _LightPosition;
            float4 _LightDirection;
            half _LightConeAngle;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color;

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                half3 toLight = normalize(_LightPosition - i.pos);
                half3 lightDir = normalize(_LightDirection.xyz);
                half spotFactor = dot(toLight, -lightDir);

                if (spotFactor < cos(radians(_LightConeAngle * 0.5)))
                {
                    discard;
                }

                return tex2D(_MainTex, i.texcoord) * i.color;
            }
            ENDCG
        }
    }
}
