Shader "Custom/MetaballMembrane"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _Radius ("Influence Radius", Range(0.1, 2)) = 0.5
        _Smoothness ("Blend Smoothness", Range(0.01, 1)) = 0.2
        _Threshold ("Surface Threshold", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            StructuredBuffer<float3> _MetaballPositions;
            int _MetaballCount;
            float _Radius;
            float _Smoothness;
            float _Threshold;
            fixed4 _Color;
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            
            float metaball(float3 pos, float3 center, float radius)
            {
                float d = distance(pos, center);
                return radius / (d * d);
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                float sum = 0;
                for (int j = 0; j < _MetaballCount; j++)
                {
                    sum += metaball(i.worldPos, _MetaballPositions[j], _Radius);
                }
                
                float alpha = smoothstep(_Threshold - _Smoothness, _Threshold + _Smoothness, sum);
                return fixed4(_Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}