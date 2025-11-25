Shader "Custom/SimpleShowReflection"
{
    Properties{
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ReflectionTex("ReflectionTex", 2D) = "white" {}
    }
    SubShader{
        Tags{ "RenderType"="Transparent" "Queue"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Pass{
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _ReflectionTex;
            sampler2D _MainTex;
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; };
            v2f vert(appdata v) { v2f o; o.vertex = UnityObjectToClipPos(v.vertex); o.uv = v.uv; return o; }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 c = tex2D(_ReflectionTex, i.uv);
                // show alpha as fully opaque for diagnosis
                c.a = 1.0;
                return c;
            }
            ENDCG
        }
    }
    FallBack Off
}