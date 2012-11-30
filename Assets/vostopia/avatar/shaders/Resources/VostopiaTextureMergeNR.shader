Shader "Vostopia/TextureMergeNR"
{
    Properties 
	{
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Normal", 2D) = "normal" {}
		_ReflTex ("Reflection", 2D) = "white" {}
    }

	SubShader 
	{ 
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _Color;
			sampler2D _MainTex;
			sampler2D _ReflTex;

			//Unity-required vars
			float4 _MainTex_ST;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv: TEXCOORD0;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				float2 nm = tex2D(_MainTex, i.uv).rg;
				float rf = tex2D(_ReflTex, i.uv).a * _Color.b;
				return float4(nm.r, nm.g, rf, 1);
			}

			ENDCG
		}
	}
	Fallback Off 
}
