Shader "HuntingBoat/Ocean"
{
    Properties
    {
        _DeepColor ("Deep water", Color) = (0.018,0.16,0.24,1)
        _ShallowColor ("Wave crest", Color) = (0.04,0.43,0.48,1)
        _WaveScale ("Wave strength", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            float4 _DeepColor, _ShallowColor;
            float _WaveScale;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 position : SV_POSITION; float3 world : TEXCOORD0; float3 normal : TEXCOORD1; UNITY_FOG_COORDS(2) };
            v2f vert(appdata v)
            {
                v2f o;
                float3 world = mul(unity_ObjectToWorld, v.vertex).xyz;
                float t = _Time.y;
                float a = world.x * .035 + t * 1.1;
                float b = world.z * .055 - t * .85;
                world.y += (sin(a) * .26 + sin(b) * .16) * _WaveScale;
                o.world = world;
                o.normal = normalize(float3(-cos(a) * .0091 * _WaveScale, 1, -cos(b) * .0088 * _WaveScale));
                o.position = mul(UNITY_MATRIX_VP, float4(world, 1));
                UNITY_TRANSFER_FOG(o, o.position);
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float t = _Time.y;
                float3 n = normalize(i.normal + float3(sin(i.world.x * 1.15 + t * 2) * .06, 0, cos(i.world.z * 1.25 - t * 1.6) * .055));
                float3 view = normalize(_WorldSpaceCameraPos - i.world);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float fresnel = pow(1 - saturate(dot(n, view)), 3);
                float crest = sin(i.world.x * .035 + t * 1.1) * .5 + .5;
                float3 baseColor = lerp(_DeepColor.rgb, _ShallowColor.rgb, crest * .3);
                float spec = pow(saturate(dot(n, normalize(lightDir + view))), 120);
                float foam = pow(saturate(sin(i.world.x * .35 + i.world.z * .42 + t * 1.5)), 28) * .065;
                float3 color = baseColor * (.42 + saturate(dot(n, lightDir)) * .6) + fresnel * float3(.17,.30,.38) + _LightColor0.rgb * spec * .8 + foam;
                fixed4 result = fixed4(color, 1); UNITY_APPLY_FOG(i.fogCoord, result); return result;
            }
            ENDCG
        }
    }
}
