Shader "Vostopia/TextureMergeTSI"
{
    Properties 
	{
        _TransScale ("Transparency Scale", Float) = 1
        _SpecScale ("Specular Scale", Float) = 1
        _IllumScale ("Glow Scale", Float) = 1
        _MainTex ("Transpacency", 2D) = "white" {}
		_SpecTex ("Specular", 2D) = "white" {}
		_IllumTex ("Glob", 2D) = "white" {}
    }

	SubShader 
	{ 
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float _TransScale;
			float _SpecScale;
			float _IllumScale;
			sampler2D _MainTex;
			sampler2D _SpecTex;
			sampler2D _IllumTex;

			//Unity-required vars
			float4 _MainTex_ST;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv: TEXCOORD0;
				float4 color : COLOR;
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
				float tr = tex2D(_MainTex, i.uv).a * _TransScale;
				float sp = tex2D(_SpecTex, i.uv).a * _SpecScale;
				float il = tex2D(_IllumTex, i.uv).a * _IllumScale;
				return float4(tr, sp, il, 1);
			}

			ENDCG
		}
	}
	Fallback Off 
}
