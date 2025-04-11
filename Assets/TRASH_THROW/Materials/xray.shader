Shader "Custom/SimpleHighlight" {
    Properties {
        _Color ("Color", Color) = (1,1,1,0.5)
        _OutlineWidth ("Outline Width", Range(0.001, 0.03)) = 0.01
        _EmissionStrength ("Emission Strength", Range(0, 5)) = 1.0
    }
    
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        
        // Make sure it shows through other objects
        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            
            struct v2f {
                float4 pos : SV_POSITION;
            };
            
            float4 _Color;
            float _OutlineWidth;
            float _EmissionStrength;
            
            v2f vert (appdata v) {
                v2f o;
                // Expand vertices along normals
                float3 normal = normalize(v.normal);
                float3 position = v.vertex + normal * _OutlineWidth;
                o.pos = UnityObjectToClipPos(position);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                // Add emission effect
                fixed4 col = _Color;
                col.rgb *= (1.0 + _EmissionStrength);
                return col;
            }
            ENDCG
        }
    }
    FallBack "Transparent/Diffuse"
}