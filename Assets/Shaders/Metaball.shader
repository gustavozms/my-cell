Shader "Custom/Metaball"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Threshold ("Threshold", Range(0, 1)) = 0.5
        _Smoothness ("Smoothness", Range(0, 0.2)) = 0.1
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                float3 worldPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Threshold;
            float _Smoothness;
            
            int _Count;
            float4 _Positions[100]; // Maximum 100 metaballs
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float sum = 0;
                
                // Calculate metaball field
                for (int j = 0; j < _Count; j++)
                {
                    float2 pos = _Positions[j].xy;
                    float dist = distance(pos, i.worldPos.xy);
                    sum += 1.0 / (1.0 + dist * 20.0); // Inverse square falloff
                }
                
                // Smooth thresholding
                float alpha = smoothstep(_Threshold - _Smoothness, 
                                       _Threshold + _Smoothness, 
                                       sum);
                
                fixed4 col = _Color;
                col.a = alpha;
                return col;
            }
            ENDCG
        }
    }
}