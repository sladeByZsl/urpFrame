Shader "Unlit/ClearFog"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogTex("fog",2D)="white" {}
        [Toggle(RESET)]_Reset("_Reset",float)=1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ RESET

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;

                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _FogTex;
            float4 _MainTex_ST;
            uniform half4 _DrawRect;
            
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 col=fixed4(1,1,1,1);
                #ifdef RESET
                    return  float4(1,1,1,1);
                #endif
                //if (i.uv.x>_DrawRect.z || i.uv.y>_DrawRect.w || i.uv.x<_DrawRect.x || i.uv.y<_DrawRect.y )
                //{
                //    discard;
                //}
                float2 drawSize = _DrawRect.zw - _DrawRect.xy;
                fixed alpha = tex2D(_MainTex,(i.uv -_DrawRect.xy)/drawSize).a;
                fixed fog = tex2D(_FogTex,i.uv).a;
                col.a =alpha * fog;
                
                return col;
            }
            ENDCG
        }
    }
}
