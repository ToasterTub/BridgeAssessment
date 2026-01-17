Shader "Custom/Standard-GlossMetalEmissive"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NormalTex ("Normal Map", 2D) = "bump" {}
        _GlossTex ("Gloss", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _MetalTex ("Metal", 2D) = "white" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _EmissionTex ("Emission Tex", 2D) = "white" {}
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex, _GlossTex, _MetalTex, _NormalTex, _EmissionTex;

        struct Input
        {
            float2 uv_MainTex;
        };
        
        half _Glossiness, _Metallic;
        fixed4 _Color, _EmissionColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 albedo = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            fixed3 normal = UnpackNormal(tex2D (_NormalTex, IN.uv_MainTex));
            fixed3 emission = tex2D (_EmissionTex, IN.uv_MainTex) * _EmissionColor;
            fixed3 metal = tex2D (_MetalTex, IN.uv_MainTex) * _Metallic;
            fixed3 smooth = tex2D (_GlossTex, IN.uv_MainTex) * _Glossiness;

            o.Emission = emission;
            o.Albedo = albedo.rgb;
            o.Metallic = metal;
            o.Normal = normal;
            o.Smoothness = smooth;
            o.Alpha = albedo.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
