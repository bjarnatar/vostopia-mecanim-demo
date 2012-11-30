Shader "Vostopia/TextureStamp"
{
    Properties 
	{
        _Color ("Main Color", Color) = (1,1,1,1)
        _MainTex ("_MainTex", 2D) = "white" {}
		_MaskTex ("_MaskTex", 2D) = "white" {}
		_MaskTex2 ("_MaskTex2", 2D) = "white" {}
    }

	SubShader 
	{ 
		ZTest Always 
		Cull Off 
		ZWrite On 
		Fog { Mode Off }
		Blend One OneMinusSrcAlpha
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			float4 _Color;
			sampler2D _MainTex;
			sampler2D _MaskTex;
			sampler2D _MaskTex2;

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
				float4 base = tex2D(_MainTex, i.uv) * _Color;
				float mask = tex2D(_MaskTex, i.uv).a * tex2D(_MaskTex2, i.uv).a * _Color.a;

				float4 col;
				col.rgb = base.rgb * mask;
				col.a = mask;
				return col;
			}

			ENDCG
		}
	}
	Fallback Off 
}
