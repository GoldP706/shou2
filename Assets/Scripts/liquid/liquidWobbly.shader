Shader "Custom/LiquidWobble"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _WobbleStrength ("晃动强度", Range(0, 0.1)) = 0.03
        _WobbleSpeed ("晃动速度", Range(0, 10)) = 3
        _OffsetX ("水平偏移", Float) = 0
        _OffsetY ("垂直偏移", Float) = 0
        
        // 涟漪参数
        _SplashCenter ("涟漪中心X", Range(0, 1)) = 0.5
        _SplashAmount ("涟漪强度", Range(0, 0.2)) = 0
        _SplashFrequency ("涟漪密度", Float) = 20
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
        }
        LOD 100

        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _WobbleStrength;
            float _WobbleSpeed;
            float _OffsetX;
            float _OffsetY;
            float _SplashCenter;
            float _SplashAmount;
            float _SplashFrequency;

            v2f vert (appdata v)
            {
                v2f o;
                
                // 顶点波动：杯子顶部的顶点上下晃动
                float topFactor = v.uv.y; // 越靠上晃动越大
                float wobble = sin(v.vertex.x * _WobbleSpeed + _Time.y * _WobbleSpeed) 
                             * _WobbleStrength * topFactor;
                
                // 加上液体惯性偏移带来的倾斜
                float tilt = _OffsetX * 0.5 * topFactor;
                
                v.vertex.y += wobble + tilt;
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // 涟漪：在UV上做径向波动
                float dist = distance(i.uv.x, _SplashCenter);
                float ripple = sin(dist * _SplashFrequency - _Time.y * 10) 
                             * _SplashAmount 
                             * (1 - dist * 2) // 中心强，边缘弱
                             * i.uv.y;        // 只在液面有
                
                float2 uvOffset = float2(0, ripple);
                
                fixed4 col = tex2D(_MainTex, i.uv + uvOffset);
                return col;
            }
            ENDCG
        }
    }
}