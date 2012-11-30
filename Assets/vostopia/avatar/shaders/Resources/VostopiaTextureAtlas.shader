Shader "Vostopia/TextureAtlas"
{
    Properties 
	{
        _MainTex ("_MainTex", 2D) = "white" {}
		_Offset  ("Offset", Vector) = (0, 0, 0, 0)
		_Scale   ("Scale", Vector) = (1, 1, 0, 0)
    }

	SubShader 
	{ 
		ZTest Always Cull Off ZWrite Off Fog { Mode Off }
		Blend One OneMinusSrcAlpha
		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			sampler2D _MainTex;
			float4 _Offset;
			float4 _Scale;

			//Unity-required vars
			float4 _MainTex_ST;

			struct v2f {
				float4 pos : SV_POSITION;
				float2 uv: TEXCOORD0;
			};

			v2f vert (appdata_base v)
			{
				v2f o;
				o.pos = v.vertex;
				o.pos.xy = 2 * o.pos.xy * _Scale.xy + 2 * _Offset.xy - 1;
				#if SHADER_API_D3D9
				o.pos.y = - o.pos.y;
				#endif
			
				float2 texcoord = v.texcoord;
				texcoord.y = 1 - texcoord.y;

				o.uv = TRANSFORM_TEX(texcoord, _MainTex);
				return o;
			}

			float4 frag(v2f i) : COLOR
			{
				return tex2D(_MainTex, i.uv);
			}

			ENDCG
		}
	}
	Fallback Off 
}
