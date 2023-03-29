Shader "Fog/Blend"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CurTex ("Current RT", 2D) = "white"{}
    }
    SubShader
    {
        
        Pass{
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
            };
            float4 _MainTex_ST;
            sampler2D _MainTex, _CurTex;

            v2f vert(a2v i){
                v2f o;
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.uv = TRANSFORM_TEX(i.uv, _MainTex);
                return o;
            }
            fixed4 frag(v2f i): SV_TARGET{
                fixed4 col1 = tex2D(_MainTex, i.uv);
                fixed4 col2 = tex2D(_CurTex, i.uv);
                fixed4 final = saturate(col1 + col2);
                return final;
            }
            ENDCG
        }
    }
}
