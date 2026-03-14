Shader "UI/SimpleHealthBar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _FillAmount ("Fill Amount", Range(0,1)) = 1.0
        _BorderWidth ("Border Width", Range(0, 0.1)) = 0.02
        _BorderColor ("Border Color", Color) = (1,1,1,0.5)
        _CornerRadius ("Corner Radius", Range(0, 0.5)) = 0.1

        _ColorFull ("Full Health Color", Color) = (0.2, 1, 0.2, 1)    // зелёный
        _ColorMid ("Mid Health Color", Color) = (1, 1, 0.2, 1)        // жёлтый
        _ColorEmpty ("Empty Health Color", Color) = (1, 0.2, 0.2, 1)  // красный

        // Стандартные UI параметры
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _FillAmount;
            float _BorderWidth;
            fixed4 _BorderColor;
            float _CornerRadius;
            fixed4 _ColorFull;
            fixed4 _ColorMid;
            fixed4 _ColorEmpty;

            float4 _ClipRect;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            // Функция расстояния до скруглённого прямоугольника
            float roundedRectSDF(float2 uv, float2 halfSize, float r)
            {
                float2 d = abs(uv) - halfSize + r;
                return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - r;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 spriteColor = tex2D(_MainTex, IN.texcoord) * IN.color;

                float2 uv = IN.texcoord - 0.5;
                float2 halfSize = float2(0.5, 0.5);
                float dist = roundedRectSDF(uv, halfSize, _CornerRadius);

                // Расчёт альфы для скруглённых углов
                float alpha = 1.0 - smoothstep(0.0, 0.02, max(0, dist)); // внутри alpha = 1, снаружи плавно спадает

                if (dist > 0.0) discard;

                float t = _FillAmount;
                fixed4 healthColor;
                healthColor = lerp(_ColorEmpty, _ColorMid, saturate(t * 2.0));
                healthColor = lerp(healthColor, _ColorFull, saturate((t - 0.5) * 2.0));

                float fillMask = step(uv.x + 0.5, _FillAmount);

                fixed4 finalColor = healthColor;
                finalColor.a = healthColor.a * fillMask;

                float borderDist = abs(dist);
                float borderMask = 1.0 - smoothstep(0.0, _BorderWidth, borderDist);
                float borderAmount = borderMask * _BorderColor.a;

                finalColor.rgb = lerp(finalColor.rgb, _BorderColor.rgb, borderAmount);
                finalColor.a = max(finalColor.a, borderAmount);

                finalColor.a *= alpha;
                finalColor *= spriteColor;

                #ifdef UNITY_UI_CLIP_RECT
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(finalColor.a - 0.001);
                #endif

                return finalColor;
            }
            ENDCG
        }
    }
}