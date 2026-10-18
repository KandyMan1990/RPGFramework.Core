// A UI Toolkit filter: takes _Subtract from each channel of its source, clamped at zero, and adds _Add, clamped at one.
Shader "Hidden/RPG Framework/Screen Colour"
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
            float4    _Subtract;
            float4    _Add;
            float     _ReadsGamma;
            float     _WritesGamma;

            float3 ToGamma(float3 colour)
            {
                return float3(LinearToGammaSpaceExact(colour.r), LinearToGammaSpaceExact(colour.g), LinearToGammaSpaceExact(colour.b));
            }

            float3 ToLinear(float3 colour)
            {
                return float3(GammaToLinearSpaceExact(colour.r), GammaToLinearSpaceExact(colour.g), GammaToLinearSpaceExact(colour.b));
            }

            Varyings Vertex(FilterVertexInput input)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.uv       = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            // Changed in gamma space, where the amounts are authored, and written opaque, since the screen's alpha
            // beneath can be anything.
            half4 Fragment(Varyings input) : SV_Target
            {
                float3 colour = saturate(tex2D(_MainTex, input.uv).rgb);

                if (_ReadsGamma < 0.5)
                {
                    colour = ToGamma(colour);
                }

                colour = saturate(max(colour - _Subtract.rgb, 0.0) + _Add.rgb);

                if (_WritesGamma < 0.5)
                {
                    colour = ToLinear(colour);
                }

                return half4(colour, 1.0);
            }
            ENDCG
        }
    }
}