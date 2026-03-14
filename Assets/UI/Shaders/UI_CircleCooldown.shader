Shader "UI/CircleCooldown"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _Progress ("Progress", Range(0,1)) = 0
        _FillColor ("Fill Color", Color) = (0.5,0.5,0.5,0.5)
        _StartAngle ("Start Angle (degrees)", Range(0,360)) = 270

        // Стандартные параметры для UI-маскирования
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
            float4 _Color;
            float4 _FillColor;
            float _Progress;
            float _StartAngle;

            // Параметры для UI-маскирования
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
                // Основной цвет из текстуры (с учётом тонирования)
                half4 color = tex2D(_MainTex, IN.texcoord) * IN.color;

                // Координаты внутри спрайта (0–1)
                float2 uv = IN.texcoord;
                float2 center = float2(0.5, 0.5);
                float2 dir = uv - center;
                float dist = length(dir);

                // Отсекаем пиксели вне круга (радиус 0.5)
                if (dist > 0.5)
                    discard;

                // Вычисляем угол по часовой стрелке от правого направления (0)
                // atan2(-dir.y, dir.x) даёт: 0 - право, 90 - низ, 180 - лево, 270 - верх
                float angleCW = atan2(-dir.y, dir.x);          // результат в диапазоне [-pi, pi]
                angleCW = angleCW < 0 ? angleCW + 2 * UNITY_PI : angleCW; // приводим к [0, 2pi]

                // Начальный угол в радианах
                float startRad = radians(_StartAngle);

                // Угловое расстояние от startAngle по часовой стрелке
                float delta = fmod(angleCW - startRad + 2 * UNITY_PI, 2 * UNITY_PI);

                // Пороговое значение, соответствующее прогрессу
                float threshold = _Progress * 2 * UNITY_PI;

                // Если угол меньше порога — пиксель прозрачный (фон)
                // Иначе — закрашиваем цветом перезарядки
                if (delta < threshold)
                {
                    color.a = 0;
                }
                else
                {
                    color = _FillColor;
                }

                // Поддержка UI-маски (прямоугольная маска)
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                // Отсечение по альфе (если включено)
                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}