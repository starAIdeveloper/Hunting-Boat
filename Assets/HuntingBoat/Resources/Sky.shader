Shader "HuntingBoat/Sky"
{
    Properties
    {
        _SkyTop ("Zenith",Color)=(0.12,0.36,0.60,1)
        _Horizon ("Horizon",Color)=(0.95,0.53,0.27,1)
        _Exposure ("Exposure",Float)=1
        _SunDirection ("Sun direction",Vector)=(0,0.3,1,0)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            float4 _SkyTop,_Horizon,_SunDirection; float _Exposure;
            struct v2f { float4 pos:SV_POSITION; float3 direction:TEXCOORD0; };
            v2f vert(float4 vertex:POSITION) { v2f o; o.pos=UnityObjectToClipPos(vertex); o.direction=vertex.xyz; return o; }
            fixed4 frag(v2f i):SV_Target
            {
                float3 d=normalize(i.direction); float h=saturate(d.y);
                float3 sky=lerp(_Horizon.rgb,_SkyTop.rgb,pow(h,.5));
                float sun=pow(saturate(dot(d,normalize(_SunDirection.xyz))),1100);
                float glow=pow(saturate(dot(d,normalize(_SunDirection.xyz))),30)*.15;
                return fixed4((sky+float3(1,.8,.5)*(sun*2+glow))*_Exposure,1);
            }
            ENDCG
        }
    }
}
