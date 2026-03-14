Shader "UI/StylizedHealthBar"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _FillAmount ("Fill Amount", Range(0,1)) = 1.0
        _FillColor ("Fill Color", Color) = (0.2, 0.8, 0.2, 1)    // зелёный
        _EmptyColor ("Empty Color", Color) = (0.4, 0.1, 0.1, 1)  // тёмно-красный
        _EdgeColor ("Edge Color", Color) = (1,1,1,0.5)            // цвет обводки
        _EdgeWidth ("Edge Width", Range(0, 0.1)) = 0.02
        
        _GradientStart ("Gradient Start Color", Color) = (0.2,0.8,0.2,1)
        _GradientEnd ("Gradient End Color", Color) = (1,0.5,0,1)

        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _NoiseIntensity ("Noise Intensity", Range(0,1)) = 0.15

        _GlowIntensity ("Glow Intensity", Range(0,1)) = 0.3
        _GlowColor ("Glow Color", Color) = (1,1,0.5,1)

        _PulseThreshold ("Pulse Threshold", Range(0,0.5)) = 0.2    // при каком уровне здоровья начинается пульсация
        _PulseSpeed ("Pulse Speed", Range(0.5, 5)) = 2.0
        _PulseMinAlpha ("Pulse Min Alpha", Range(0,1)) = 0.3      // минимальная прозрачность при пульсации
        _PulseColor ("Pulse Color", Color) = (1,0,0,1)            // цвет пульсации (красный)

        // стандартные UI параметры
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
            fixed4 _FillColor;
            fixed4 _EmptyColor;
            fixed4 _EdgeColor;
            float _EdgeWidth;
            float _FillAmount;
            fixed4 _GradientStart;
            fixed4 _GradientEnd;
            sampler2D _NoiseTex;
            float _NoiseIntensity;
            float _GlowIntensity;
            fixed4 _GlowColor;
            
            // Параметры пульсации
            float _PulseThreshold;
            float _PulseSpeed;
            float _PulseMinAlpha;
            fixed4 _PulseColor;

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

            fixed4 frag(v2f IN) : SV_Target
            {
                // Основной цвет из текстуры (если есть)
                half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;

                // Координаты внутри спрайта (0-1)
                float2 uv = IN.texcoord;

                // Определяем, заполнена ли текущая точка
                float isFilled = step(uv.x, _FillAmount);

                // Градиент
                float t = uv.x;
                fixed4 gradientColor = lerp(_GradientStart, _GradientEnd, t);

                // Цвета заполненной и пустой частей
                fixed4 fillAreaColor = gradientColor * _FillColor;
                fixed4 emptyAreaColor = _EmptyColor;
                fixed4 healthColor = lerp(emptyAreaColor, fillAreaColor, isFilled);

                // Шум
                float noise = tex2D(_NoiseTex, uv * 2).r;
                healthColor.rgb += (noise - 0.5) * _NoiseIntensity;

                // Обводка по периметру
                float2 borderDist = float2(
                    min(uv.x, 1 - uv.x),        // расстояние до левой/правой границы
                    min(uv.y, 1 - uv.y)         // расстояние до верхней/нижней границы
                );
    
                float minBorderDist = min(borderDist.x, borderDist.y);
                float borderMask = 1 - smoothstep(0, _EdgeWidth, minBorderDist);
    
                fixed4 borderColor = _EdgeColor * borderMask;

                // Свечение у границы заполнения
                float glowWidth = 0.05;
                float glowDist = 1 - saturate(abs(uv.x - _FillAmount) / glowWidth);
                float glowMask = glowDist * glowDist;
                fixed4 glow = _GlowColor * glowMask * _GlowIntensity;

                // Пульсация при низком здоровье
                float isLowHealth = step(_FillAmount, _PulseThreshold);
                float lowHealthFactor = saturate(1 - (_FillAmount / max(_PulseThreshold, 0.001)));
                lowHealthFactor *= isLowHealth;
    
                float pulse = (sin(_Time.y * _PulseSpeed * 2 * 3.14159) + 1) * 0.5;
                float pulseAlpha = lerp(_PulseMinAlpha, 1, pulse);
    
                fixed4 pulseColor = _PulseColor * isFilled * lowHealthFactor * (1 - pulseAlpha);
                healthColor.rgb *= lerp(1.0, (0.8 + 0.2 * pulse), isLowHealth * isFilled);

                // Итоговый цвет с обводкой
                color.rgb = healthColor.rgb + borderColor.rgb + glow.rgb + pulseColor.rgb;
                color.a = healthColor.a;

                // Поддержка UI-маски
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}