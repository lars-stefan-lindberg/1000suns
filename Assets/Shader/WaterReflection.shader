Shader "Custom/WaterReflection2D_pixelArt"
{
    Properties
    {
        _MainTex ("Sprite Texture (required)", 2D) = "white" {}
        _ReflectionTex ("Reflection Texture", 2D) = "white" {}
        _NormalTex ("Normal Map", 2D) = "bump" {}
        _NormalTiling ("Normal Tiling", Float) = 0.12
        _NormalOffset1 ("Normal Offset 1", Vector) = (0,0,0,0)
        _NormalOffset2 ("Normal Offset 2", Vector) = (0,0,0,0)
        _NormalStrength ("Normal Strength", Range(0,2)) = 0.9
        _ReflectionOpacity ("Reflection Opacity", Range(0,1)) = 1.0
        _WaterTint ("Water Tint", Color) = (0.6,0.75,0.9,1)
        _FadeHeight ("Fade Height (world units)", Float) = 1.0
        _WaterY ("Waterline Y (world)", Float) = 0.0
        _PixelSize ("Pixel Size (1/RTheight)", Float) = 0.00277778
        _PixelSnap ("Pixel Snap Enabled", Range(0,1)) = 1
        _RippleX ("Ripple Horizontal Strength", Range(0,2)) = 0.4
        _RippleY ("Ripple Vertical Strength", Range(0,2)) = 1.0
    }

    SubShader
    {
        Tags {
            "RenderType"="Transparent"
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _ReflectionTex;
            sampler2D _NormalTex;

            float _NormalTiling;
            float4 _NormalOffset1;
            float4 _NormalOffset2;
            float _NormalStrength;
            float _ReflectionOpacity;
            float4 _WaterTint;
            float _FadeHeight;
            float _WaterY;
            float _PixelSize;
            float _PixelSnap;
            float _RippleX;
            float _RippleY;

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // Helper: sample normal map in a cellified (pixel-art) way
            float3 SampleNormalPixel(sampler2D tex, float2 uv, float cells)
            {
                // uv wrapped and tiled
                float2 cellUV = frac(uv * cells);
                float2 baseUV = (floor(uv * cells) + 0.5) / cells; // center of cell
                float3 n = tex2D(tex, baseUV).rgb;
                return n;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // --- Scroll normal map ---
                float2 nuv1 = frac(i.uv * _NormalTiling + _NormalOffset1.xy);
                float2 nuv2 = frac(i.uv * _NormalTiling + _NormalOffset2.xy);

                float3 n1 = tex2D(_NormalTex, nuv1).rgb;
                float3 n2 = tex2D(_NormalTex, nuv2).rgb;

                // --- Wave from blue channel ---
                float wave1 = n1.b * 2.0 - 1.0;
                float wave2 = n2.b * 2.0 - 1.0;
                float wave  = (wave1 + wave2) * 0.5;

                // --- Distance from shore ---
                float shoreMask = saturate(1.0 - i.uv.y);

                // --- Strength ---
                float ripple = wave * _NormalStrength * shoreMask;

                // --- Reflection UV ---
                float2 reflUV = float2(i.uv.x, 1.0 - i.uv.y);

                // --- Horizontal ripple ONLY (safe) ---
                reflUV.x += ripple * _RippleX;
                reflUV.y += ripple * _RippleY;

                // --- Sample reflection ---
                fixed4 reflectionCol = tex2D(_ReflectionTex, reflUV);

                // --- Tint + Opacity ---
                float3 col = reflectionCol.rgb * _WaterTint.rgb;
                float alpha = _ReflectionOpacity;

                return fixed4(col, alpha);
            }
            ENDCG
        }
    }

    FallBack Off
}
