
Shader "Faster/Common/GaussBlur"
{
    Properties
    {
        [HideInInspector]_MainTex("Texture", 2D) = "white" {}
        _BlurRadius("_BlurRadius",float)=1
    }

    SubShader
    {
          CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _MainTex;
        uniform float4 _MainTexSize;
        half _BlurRadius;

        struct appdata
        {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
            float2 uvs[5] : TEXCOORD1;
        };

        v2f vert_VerticalBlur(appdata v) {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            float2 texSize=v.uv*_MainTexSize.xy;
            o.uvs[0] =v.uv;
            o.uvs[1] =(texSize+float2(0,1.5) * _BlurRadius)/_MainTexSize;
            o.uvs[2] = (texSize+float2(0,-1.5) * _BlurRadius)/_MainTexSize;
            o.uvs[3] = (texSize+float2(0,2.5) * _BlurRadius)/_MainTexSize;
            o.uvs[4] = (texSize+float2(0,-2.5) * _BlurRadius)/_MainTexSize;
            return o;
        }

        v2f vert_HorizontalBlur(appdata v) {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            float2 texSize=v.uv*_MainTexSize.xy;
            o.uvs[0] = v.uv;
            o.uvs[1] =(texSize+float2(1.5,0) * _BlurRadius)/_MainTexSize;
            o.uvs[2] =  (texSize+float2(-1.5,0) * _BlurRadius)/_MainTexSize;
            o.uvs[3] =(texSize+float2(2.5,0) * _BlurRadius)/_MainTexSize;
            o.uvs[4] =(texSize+float2(-2.5,0) * _BlurRadius)/_MainTexSize;
            return o;
        }

        fixed4 fragBlur(v2f i) : SV_Target
        {
            half weight[3] = {0.4026, 0.2442, 0.0545};

            fixed4 col = tex2D(_MainTex, i.uvs[0]) * weight[0];

            for (int j = 1; j < 3; j++)
            {
                col += tex2D(_MainTex, i.uvs[2 * j - 1]) * weight[j];
                col += tex2D(_MainTex, i.uvs[2 * j]) * weight[j];
            }

            return col;
        }
        ENDCG

        ZTest Always
        Cull Off
        ZWrite Off

            //Pass1
            Pass
            {
                NAME "GAUSSIAN_BLUR_VERTICAL"

                CGPROGRAM
                #pragma vertex vert_VerticalBlur
                #pragma fragment fragBlur
                ENDCG
            }

        //Pass2
        Pass
        {
            NAME "GAUSSIAN_BLUR_HORIZONTAL"

            CGPROGRAM
            #pragma vertex vert_HorizontalBlur
            #pragma fragment fragBlur
            ENDCG
        }
    }
}
