Shader "Custom/EmissionShader"
{
    Properties
    {
        _Color ("Main Color", Color) = (.5,.5,.5,1)
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionIntensity ("Emission Intensity", Range (0, 10)) = 1.0
        _MainTex ("Base (RGB)", 2D) = "white" { }
    }
 
    SubShader
    {
        Tags {"Queue"="Overlay" }
        LOD 100
 
        CGPROGRAM
        #pragma surface surf Lambert
 
        sampler2D _MainTex;
 
        struct Input
        {
            float2 uv_MainTex;
        };
 
        fixed4 _Color;
        fixed4 _EmissionColor;
        float _EmissionIntensity;
 
        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
 
            // Emission
            o.Emission = _EmissionColor.rgb * _EmissionIntensity;
        }
        ENDCG
    }
 
    FallBack "Diffuse"
}
