Shader "Unlit/PastelDots_Wipe_LtoR_Jitter_Palette16"
{
    Properties
    {
        // 0→1 でワイプ進行（0: ほぼ全部見える / 1: ほぼ全部消える）
        _Threshold ("Progress", Range(0,1)) = 0

        // 画面内のドットブロックサイズ（px）
        _BlockPx ("Block Size (px)", Float) = 20

        // シェーダー内での「画面ピクセル換算」用（スクリプトからScreen.width/heightを毎回渡すの推奨）
        _ScreenW ("Screen Width", Float) = 1920
        _ScreenH ("Screen Height", Float) = 1080

        // 色生成（HSV）パラメータ
        _Pastel ("Pastel Amount", Range(0,1)) = 0.4
        _Brightness ("Brightness", Range(0,1)) = 0.95
        _HueShift ("Hue Shift", Range(0,1)) = 0

        // このマテリアル自体の透明度（Open/Closeで最終的に0にするなど）
        _Alpha ("Overlay Alpha", Range(0,1)) = 1

        // ドットごとの「進行ずらし」量（バラバラ度）
        _Jitter ("Random Jitter", Range(0,1)) = 0.15

        // 固定16色パレット化を使うか（1=使う/0=HSVのまま）
        _UsePalette16 ("Use Fixed 16-Color Palette", Range(0,1)) = 1

        // 発色寄せ（パステルの雰囲気を崩さない程度に）
        _SaturationBoost ("Saturation Boost", Range(0,2)) = 1.02
        _Gamma ("Gamma (darken)", Range(0.5,2)) = 1.12

        // GBCっぽい「粗さ」＋「ザラつき」
        _LowResX ("Virtual LowRes X", Float) = 160
        _LowResY ("Virtual LowRes Y", Float) = 144
        _Dither ("Dither Strength", Range(0,1)) = 0.12
    }

    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // ---- Properties ----
            float _Threshold;

            float _BlockPx, _ScreenW, _ScreenH;

            float _Pastel, _Brightness, _Alpha, _HueShift;
            float _Jitter;

            float _UsePalette16;
            float _SaturationBoost;
            float _Gamma;

            float _LowResX, _LowResY;
            float _Dither;

            // ---- Vertex/Fragment structs ----
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                return o;
            }

            // 2D hash -> 0..1（セルごとに固定）
            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 78.233);
                return frac(p.x * p.y);
            }

            // HSV -> RGB
            float3 hsv2rgb(float3 c)
            {
                float4 K = float4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
                float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            // ---- ゆめかわ寄り パステル16色パレット（ピンク/ラベンダー多め） ----
            void GetGbcPalette16(out float3 pal[16])
            {
                // neutrals
                pal[0]  = float3(0.26, 0.22, 0.30); // soft plum
                pal[1]  = float3(0.48, 0.44, 0.54); // lavender gray
                pal[2]  = float3(0.78, 0.76, 0.86); // light lilac gray
                pal[3]  = float3(0.95, 0.94, 0.98); // near white

                // pinks
                pal[4]  = float3(0.99, 0.80, 0.90); // baby pink
                pal[5]  = float3(0.98, 0.68, 0.86); // sakura pink
                pal[6]  = float3(0.99, 0.62, 0.78); // candy pink
                pal[7]  = float3(0.98, 0.72, 0.78); // strawberry milk

                // lavenders / purples
                pal[8]  = float3(0.86, 0.74, 0.98); // lavender
                pal[9]  = float3(0.78, 0.70, 0.98); // periwinkle
                pal[10] = float3(0.92, 0.70, 0.96); // lilac pink
                pal[11] = float3(0.70, 0.62, 0.92); // soft violet

                // creams / peaches
                pal[12] = float3(0.99, 0.94, 0.80); // cream
                pal[13] = float3(0.99, 0.84, 0.66); // peach
                pal[14] = float3(0.99, 0.90, 0.66); // pastel yellow

                // mint accent
                pal[15] = float3(0.74, 0.96, 0.90); // mint
            }

            // 入力色 rgb に最も近いパレット色を返す（RGB空間の二乗距離）
            float3 QuantizeToPalette16(float3 rgb)
            {
                float3 pal[16];
                GetGbcPalette16(pal);

                float bestD = 1e9;
                float3 best = pal[0];

                [unroll] for (int i = 0; i < 16; i++)
                {
                    float3 diff = rgb - pal[i];
                    float dist  = dot(diff, diff);
                    if (dist < bestD)
                    {
                        bestD = dist;
                        best  = pal[i];
                    }
                }
                return best;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // ---- 1) 疑似低解像度（GBCっぽい粗さ）----
                float2 lowRes = max(float2(1.0, 1.0), float2(_LowResX, _LowResY));
                float2 uvLow  = floor(i.uv * lowRes) / lowRes;

                // ---- 2) ドットセル算出（px→cell）----
                // ScreenW/H は「UV→ピクセル換算」に使う（スクリプトから渡すと安定）
                float2 px   = float2(uvLow.x * _ScreenW, uvLow.y * _ScreenH);
                float block = max(1.0, _BlockPx);
                float2 cell = floor(px / block);

                // ---- 3) 左→右の基本進行（0..1）----
                float cellsX = max(1.0, floor(_ScreenW / block));
                float x01    = cell.x / max(1.0, (cellsX - 1.0));

                // ---- 4) ドットごとの進行ズレ（バラバラ度）----
                float rnd    = hash21(cell + 99.0);
                float offset = (rnd - 0.5) * _Jitter;

                // 最終的な進行度
                float prog = saturate(x01 + offset);

                // ---- 5) 可視判定（Progress駆動の本体）----
                // prog > Threshold のセルだけ表示（Threshold=1で残りが出にくい）
                float visible = (prog > _Threshold) ? 1.0 : 0.0;

                // ---- 6) ベース色（HSVで作ってから…）----
                float r    = hash21(cell + 10.0);
                float hue  = frac(r + _HueShift);
                float sat  = lerp(0.08, 0.40, _Pastel);
                float val  = _Brightness;
                float3 rgb = hsv2rgb(float3(hue, sat, val));

                // ---- 7) ディザ（液晶っぽいザラつき）----
                float d = (fmod(cell.x + cell.y, 2.0) * 2.0 - 1.0); // -1 or +1
                rgb += d * (_Dither * 0.03);
                rgb = saturate(rgb);

                // ---- 8) 発色寄せ → パレット量子化 ----
                rgb = saturate(rgb * _SaturationBoost);
                rgb = pow(rgb, _Gamma);

                float3 q = QuantizeToPalette16(rgb);
                rgb = lerp(rgb, q, saturate(_UsePalette16));

                // ---- 9) 出力（最終Alphaは _Alpha * visible）----
                fixed4 col;
                col.rgb = rgb;
                col.a   = _Alpha * visible;
                return col;
            }
            ENDCG
        }
    }
}
