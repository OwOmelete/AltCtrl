Shader "URP/Unlit/WindowRain"
{
    Properties
    {
        _MainTex     ("Texture (unused tint)", 2D) = "white" {}
        _BaseColor   ("Tint (RGBA -> A = opacity)", Color) = (1,1,1,1)
        _Size        ("Size", Float) = 1
        _T           ("Time", Float) = 1
        _Distortion  ("Distortion", Range(-5, 5)) = 0
        _Blur        ("Blur", Range(0, 1)) = 1
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

        // Transparent blending + don't write to depth
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "Forward"

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target   3.0

            // URP includes
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            // --------- Uniforms
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float4 _BaseColor;
            float  _Size, _T, _Distortion, _Blur;

            // Helper
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
                float4 screenPos  : TEXCOORD1; // for sampling _CameraOpaqueTexture
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS);
                // manual TRANSFORM_TEX for URP
                o.uv = v.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                o.screenPos = ComputeScreenPos(o.positionCS);
                return o;
            }

            // ------- Noise / layers (unchanged)
            float N21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

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
                return float3(offs, fogTrail); // xy = offset, z = mask
            }

            float4 frag (Varyings i) : SV_Target
            {
                // time
                float t = fmod(_Time.y + _T, 7200);

                // accumulate layers
                float3 drops = Layer(i.uv, t);
                drops += Layer(i.uv*1.23 + 7.54, t);
                drops += Layer(i.uv*1.35 + 1.54, t);
                drops += Layer(i.uv*1.57 - 7.54, t);

                // edge fade and blur factor
                float fade = 1 - saturate(fwidth(i.uv) * 60);
                float blur = _Blur * 7 * (1 - drops.z * fade);
                blur *= -.01; // negative = outward ring blur, as in original

                // screen UV from clip coords
                float2 projUv = i.screenPos.xy / i.screenPos.w;
                projUv += drops.xy * _Distortion * fade;

                // radial multi-tap blur on the opaque texture
                const float numSamples = 32;
                float4 col = 0;
                float a = N21(i.uv) * 6.2831;
                [loop]
                for (float k = 0; k < numSamples; k++)
                {
                    float2 offs = float2(sin(a), cos(a)) * blur;
                    float d = frac(sin((k + 1) * 546.) * 5424.);
                    d = sqrt(d);
                    offs *= d;

                    // SAMPLE scene color (requires "Opaque Texture" enabled)
                    col += SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, projUv + offs);
                    a += 1.0;
                }
                col /= numSamples;

                // apply optional tint; put opacity in alpha to control transparency
                col.rgb *= _BaseColor.rgb * 0.9;
                col.a    = _BaseColor.a; // transparency control (0..1)

                return col;
            }
            ENDHLSL
        }
    }
}
