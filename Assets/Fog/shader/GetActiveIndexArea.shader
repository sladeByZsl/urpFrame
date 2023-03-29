// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Fog/GetActiveIndexArea"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LastTex ("LstTex", 2D) = "black" {}
        _Area ("从中心开始的解锁区域(随时间)", float) = 0
        _Index ("Index", int) = 0
        _CenterPoint("中心点", vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
        CGPROGRAM
            #include "UnityCG.cginc"
            #pragma vertex vert
            #pragma fragment frag
            struct a2v{
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f{
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 uv0 :TEXCOORD1;
            };
            float4 _MainTex_ST, _CenterPoint;
            sampler2D _MainTex, _LastTex;
            int _Index;
            float _Area;

            v2f vert(a2v i){
                v2f o;
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                o.uv0 = i.uv;
                return o;
            }
            fixed4 frag(v2f i) : SV_TARGET{
                //激活区域的index
                float offset = length(float2(abs(i.uv0.x - _CenterPoint.x), abs(i.uv0.y - _CenterPoint.y)))/0.3f;
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed last = tex2D(_LastTex, i.uv).r;
                int index = int(col.r * 255);
                fixed4 ret = fixed4(0,0,0,1);
                if(index == _Index && last == 0 && offset <= _Area){
                    ret = fixed4(1,0,0,1);
                }
                return ret;
            }
        ENDCG  
        }

    }
}
