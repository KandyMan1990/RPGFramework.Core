// A UI Toolkit filter over a greyscale mask: _Colour where the mask is below how covered the screen is, clear above,
// blended over _Softness of the mask's range.
Shader "Hidden/RPG Framework/Mask Wipe"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            #include "UnityCG.cginc"
            #include "UnityUIEFilter.cginc"

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv       : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            float4    _Colour;
            float     _Covered;
            float     _Softness;
            float     _ReadsGamma;
            float     _WritesGamma;

            Varyings Vertex(FilterVertexInput input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv       = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            // The threshold runs from 0 to 1 + softness, so nothing is covered at 0 and everything at 1, the edge
            // included. Thresholded in gamma space, where the mask's greys are authored.
            half4 Fragment(Varyings input) : SV_Target
            {
                float mask = tex2D(_MainTex, input.uv).r;

                if (_ReadsGamma < 0.5)
                {
                    mask = LinearToGammaSpaceExact(mask);
                }

                float softness = max(_Softness, 1.0 / 255.0);
                float covered  = saturate((_Covered * (1.0 + softness) - mask) / softness) * _Colour.a;
                float3 colour  = _Colour.rgb;

                if (_WritesGamma < 0.5)
                {
                    colour = float3(GammaToLinearSpaceExact(colour.r), GammaToLinearSpaceExact(colour.g), GammaToLinearSpaceExact(colour.b));
                }

                return half4(colour * covered, covered);
            }
            ENDCG
        }
    }
}