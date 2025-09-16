Shader "URP/Unlit/WindowRainFixed"
{
    Properties
    {
        _MainTex        ("Texture (unused tint)", 2D) = "white" {}
        _BaseColor      ("Tint (RGBA)", Color) = (1,1,1,1)
        _Size           ("Size", Float) = 1
        _T              ("Time", Float) = 1
        _Distortion     ("Distortion", Range(-5, 5)) = 0
        _Blur           ("Blur", Range(0, 1)) = 0
        [Toggle]_ReplaceBackground ("Replace Background (no mixing)", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        // Prémultiplié : pas d’halo, mélange propre
        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "Forward"

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            // --------- Uniforms
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float4 _BaseColor;
            float  _Size, _T, _Distortion, _Blur, _ReplaceBackground;

            #define S(a,b,t) smoothstep(a,b,t)

            struct Attributes
            {
                float3 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 screenPos  : TEXCOORD1; // for _CameraOpaqueTexture
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv         = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.screenPos  = ComputeScreenPos(o.positionCS);
                return o;
            }

            float N21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            // xy = offset ; z = mask
            float3 Layer(float2 UV, float t)
            {
                float2 aspect = float2(2,1);
                float2 uv  = UV * _Size * aspect;
                uv.y      += t * .25;
                float2 gv  = frac(uv) - .5;
                float2 id  = floor(uv);

                float n = N21(id);
                t += n * 6.2831;

                float w = UV.y * 10;
                float x = (n - .5) * .8;
                x += (.4 - abs(x)) * sin(3*w) * pow(sin(w), 6) * .45;

                float y = -sin(t + sin(t + sin(t) * .5)) * .45;
                y -= (gv.x - x) * (gv.x - x);

                float2 dropPos = (gv - float2(x, y)) / aspect;
                float  drop    = S(.05, .03, length(dropPos));

                float2 trailPos = (gv - float2(x, t*.25)) / aspect;
                trailPos.y = (frac(trailPos.y * 8) -.5) / 8;

                float trail    = S(.03, .01, length(trailPos));
                float fogTrail = S(-.05, .05, dropPos.y);
                fogTrail *= S(.5, y, gv.y);
                trail    *= fogTrail;
                fogTrail *= S(.05, .04, abs(dropPos.x));

                float2 offs = drop * dropPos + trail * trailPos;
                return float3(offs, fogTrail);
            }

            float4 frag (Varyings i) : SV_Target
            {
                float t = fmod(_Time.y + _T, 7200);

                // Accumulate rainy layers
                float3 drops = Layer(i.uv, t);
                drops += Layer(i.uv*1.23 + 7.54, t);
                drops += Layer(i.uv*1.35 + 1.54, t);
                drops += Layer(i.uv*1.57 - 7.54, t);

                // Edge fade (for distortion intensity only)
                float fade = 1 - saturate(fwidth(i.uv) * 60);

                // Strength of effect (drives alpha logic)
                float effectStrength = max(abs(_Distortion), _Blur);
                bool  active = effectStrength > 1e-5;

                // Screen UV
                float2 projUv = i.screenPos.xy / i.screenPos.w;
                projUv += drops.xy * _Distortion * fade;

                // Sample scene color with optional blur
                float4 col;

                if (_Blur <= 1e-5f)
                {
                    // No blur → single crisp sample (no pseudo-flou)
                    col = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, projUv);
                }
                else
                {
                    // Radial multi-tap blur
                    float blur = _Blur * 7 * (1 - drops.z * fade);
                    blur *= -.01;

                    const float numSamples = 32;
                    float a = N21(i.uv) * 6.2831;
                    col = 0;
                    [loop]
                    for (float k = 0; k < numSamples; k++)
                    {
                        float2 offs = float2(sin(a), cos(a)) * blur;
                        float d = frac(sin((k + 1) * 546.) * 5424.);
                        d = sqrt(d);
                        offs *= d;

                        col += SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, projUv + offs);
                        a += 1.0;
                    }
                    col /= numSamples;
                }

                // Optional tint (no extra darkening)
                col.rgb *= _BaseColor.rgb;

                // Alpha policy:
                // - If no effect → fully transparent (really invisible)
                // - If effect active:
                //     * _ReplaceBackground != 0 → force alpha 1 (remplace le fond -> pas de mélange flou)
                //     * sinon → utilise _BaseColor.a (tu acceptes un mélange, donc "film" possible)
                float alpha = active ? ((_ReplaceBackground > 0.5) ? 1.0 : saturate(_BaseColor.a)) : 0.0;

                // Premultiply for clean edges
                col.rgb *= alpha;
                col.a    = alpha;

                return col;
            }
            ENDHLSL
        }
    }
}
