Shader "Custom/VHSRewindOverlay"
{
    Properties
    {
        // Requis par Unity UI (ne pas supprimer)
        _MainTex ("Texture", 2D) = "white" {}

        // Scanlines
        _ScanlineIntensity  ("Scanline Intensity",  Range(0, 1))   = 0.0
        _ScanlineCount      ("Scanline Count",      Range(100, 600)) = 300

        // Grain
        _GrainIntensity     ("Grain Intensity",     Range(0, 1))   = 0.0
        _GrainSize          ("Grain Size",          Range(0.5, 5)) = 1.5

        // Glitch
        _GlitchIntensity    ("Glitch Intensity",    Range(0, 1))   = 0.0
        _GlitchSpeed        ("Glitch Speed",        Range(0, 20))  = 8
        _GlitchBlockSize    ("Glitch Block Size",   Range(0.01, 0.2)) = 0.05

        // Global
        _Intensity          ("Global Intensity",    Range(0, 1))   = 0.0
    }

    SubShader
    {
        // UI transparent — s'affiche par-dessus tout sans cacher l'image
        Tags
        {
            "Queue"             = "Overlay"
            "RenderType"        = "Transparent"
            "IgnoreProjector"   = "True"
            "PreviewType"       = "Plane"
            "RenderPipeline"    = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
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
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS);
                OUT.uv          = IN.uv;
                OUT.color       = IN.color;
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
                float  t  = _Intensity;

                // Alpha de base : 0 = complètement invisible quand _Intensity = 0
                float alpha = 0.0;
                float3 col  = float3(0, 0, 0);

                // ── SCANLINES ─────────────────────────────────────────────────
                // Lignes sombres semi-transparentes par-dessus l'image
                float scanline = sin(uv.y * _ScanlineCount * 3.14159) * 0.5 + 0.5;
                float scanAlpha = scanline * _ScanlineIntensity * t;
                // Les scanlines assombrissent → couleur noire, alpha variable
                col   += float3(0, 0, 0);
                alpha  = max(alpha, scanAlpha * (1.0 - scanline)); // zones sombres seulement

                // ── GRAIN ─────────────────────────────────────────────────────
                float2 grainUV = uv * (1.0 / max(0.01, _GrainSize));
                float  noise   = rand(grainUV + frac(_Time.y * float2(13.7, 7.3)));
                // Grain : pixels clairs ou sombres aléatoires
                float  grainAlpha = _GrainIntensity * t * 0.6;
                float3 grainCol   = float3(noise, noise, noise);
                // Blend additif du grain
                col   = lerp(col, grainCol, grainAlpha);
                alpha = max(alpha, grainAlpha * abs(noise - 0.5) * 2.0);

                // ── GLITCH : bandes colorées horizontales ──────────────────────
                float time     = _Time.y * _GlitchSpeed;
                float blockRow = floor(uv.y / _GlitchBlockSize);
                float seed     = floor(time) + blockRow;
                float glitchOn = step(0.82, rand1(seed));  // ~18% des blocs glitchent

                // Couleur de glitch : cyan, magenta ou blanc cassé (typique VHS)
                float  glitchType = rand1(seed + 1.0);
                float3 glitchCol  = glitchType < 0.33
                                    ? float3(0.0, 0.9, 0.9)   // cyan
                                    : glitchType < 0.66
                                        ? float3(0.9, 0.0, 0.9)   // magenta
                                        : float3(0.9, 0.9, 0.7);  // blanc cassé

                float glitchAlpha = glitchOn * _GlitchIntensity * t
                                    * rand1(seed + 2.0) * 0.5;

                col   = lerp(col,      glitchCol,  glitchAlpha);
                alpha = max(alpha, glitchAlpha);

                // ── Ligne de déchirement fort (1-2 lignes fines) ──────────────
                float tearY     = rand1(floor(time * 0.4));       // position verticale
                float tearWidth = 0.003 + rand1(floor(time)) * 0.008;
                float inTear    = 1.0 - smoothstep(0.0, tearWidth, abs(uv.y - tearY));
                float tearAlpha = inTear * _GlitchIntensity * t * 0.8;
                col   = lerp(col,   float3(1, 1, 1), tearAlpha);
                alpha = max(alpha, tearAlpha);

                // ── Alpha global ──────────────────────────────────────────────
                // On s'assure que tout disparaît proprement quand _Intensity = 0
                alpha *= t;

                return float4(col, saturate(alpha));
            }
            ENDHLSL
        }
    }
}
