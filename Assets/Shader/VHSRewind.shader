Shader "Custom/VHSRewind"
{
    Properties
    {
        _MainTex ("Screen Texture", 2D) = "white" {}

        // Scanlines
        _ScanlineIntensity ("Scanline Intensity", Range(0, 1)) = 0.3
        _ScanlineCount     ("Scanline Count",     Range(100, 600)) = 300

        // Grain
        _GrainIntensity ("Grain Intensity", Range(0, 1)) = 0.25
        _GrainSize      ("Grain Size",      Range(0.5, 5)) = 1.5

        // Glitch
        _GlitchIntensity  ("Glitch Intensity",   Range(0, 1)) = 0.4
        _GlitchSpeed      ("Glitch Speed",        Range(0, 20)) = 8
        _GlitchBlockSize  ("Glitch Block Size",   Range(0.01, 0.2)) = 0.05

        // Global
        _Intensity ("Global Intensity", Range(0, 1)) = 0.0  // animé par script
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        ZWrite Off
        Cull Off
        ZTest Always

        Pass
        {
            Name "VHSRewindPass"

            HLSLPROGRAM
            #pragma vertex   Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            float _ScanlineIntensity;
            float _ScanlineCount;
            float _GrainIntensity;
            float _GrainSize;
            float _GlitchIntensity;
            float _GlitchSpeed;
            float _GlitchBlockSize;
            float _Intensity;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv          = IN.uv;
                return OUT;
            }

            // ── Helpers ──────────────────────────────────────────────────────

            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(127.1, 311.7))) * 43758.5453);
            }

            float rand1(float x)
            {
                return frac(sin(x * 127.1) * 43758.5453);
            }

            // ── Fragment ─────────────────────────────────────────────────────

            float4 Frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                // ── GLITCH : décalage horizontal par blocs ────────────────────
                float time       = _Time.y * _GlitchSpeed;
                float blockRow   = floor(uv.y / _GlitchBlockSize);
                float glitchSeed = floor(time) + blockRow;

                // Seuls certains blocs glitchent (seuil aléatoire)
                float glitchOn   = step(0.75, rand1(glitchSeed));
                float offsetX    = (rand1(glitchSeed + 0.3) - 0.5) * 0.08 * _GlitchIntensity * glitchOn;

                // Ligne de déchirement horizontal fort (1-2 lignes)
                float tearRow    = step(0.97, rand1(floor(time * 0.5)));
                float tearOffset = (rand1(floor(time)) - 0.5) * 0.15 * _GlitchIntensity * tearRow;
                float inTear     = step(abs(uv.y - rand1(floor(time * 0.7))), _GlitchBlockSize * 0.5);

                uv.x += offsetX + tearOffset * inTear;
                uv.x  = frac(uv.x); // wrap pour éviter les bords noirs

                // ── SAMPLE ────────────────────────────────────────────────────
                float4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);

                // ── SCANLINES ─────────────────────────────────────────────────
                float scanline  = sin(uv.y * _ScanlineCount * 3.14159);
                scanline        = scanline * 0.5 + 0.5;           // 0..1
                float scanDark  = 1.0 - scanline * _ScanlineIntensity;
                col.rgb        *= scanDark;

                // ── GRAIN ─────────────────────────────────────────────────────
                float2 grainUV  = uv * (1.0 / _GrainSize);
                float  noise    = rand(grainUV + frac(_Time.y * float2(13.7, 7.3)));
                noise           = (noise - 0.5) * _GrainIntensity;
                col.rgb        += noise;

                // ── Aberration chromatique légère sur les bords ───────────────
                float2 center   = uv - 0.5;
                float  dist     = length(center);
                float2 aberrOff = center * dist * 0.015 * _GlitchIntensity;
                float  rChannel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv + aberrOff).r;
                float  bChannel = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv - aberrOff).b;
                col.r           = lerp(col.r, rChannel, _GlitchIntensity * 0.5);
                col.b           = lerp(col.b, bChannel, _GlitchIntensity * 0.5);

                // ── Légère désaturation (ton VHS) ─────────────────────────────
                float luma      = dot(col.rgb, float3(0.299, 0.587, 0.114));
                col.rgb         = lerp(col.rgb, float3(luma, luma, luma), 0.2 * _Intensity);

                // ── Application globale (fade in/out piloté par script) ────────
                // On blend entre l'image originale et l'image avec effets
                float4 original = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                col             = lerp(original, col, _Intensity);

                return col;
            }
            ENDHLSL
        }
    }
}
