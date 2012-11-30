Shader "Vostopia/VostopiaBasic"
{
	Properties 
	{
		_MainTex("_MainTex", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
		_MapNR("_MapNR", 2D) = "normal" {}
		_MapTSI("_MapTSI", 2D) = "black" {}
		_Cube("_ReflectionCubemap", CUBE) = "" {}
		_ShininessDropoff("_ShininessDropoff", Range (0.01, 1)) = 0.1
		_Shininess("_Shininess", Range (0.01, 2)) = 1
        _SpecColor ("Specular Color", Color) = (1,1,1,1)
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 400

		CGPROGRAM
			#pragma surface surf BlinnPhong addshadow 
			#pragma target 3.0

			#define VOSTOPIA_SPECULAR
			#define VOSTOPIA_NORMAL
			#define VOSTOPIA_REFLECTION
			#include "vostopia.cginc"

			void surf (Input IN, inout SurfaceOutput o) {
				BasicSurfaceShader(IN, o);	
			}
		ENDCG
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 350

		CGPROGRAM
			#pragma surface surf BlinnPhong addshadow
			#pragma target 3.0

			#define VOSTOPIA_SPECULAR
			#define VOSTOPIA_REFLECTION
			#include "vostopia.cginc"

			void surf (Input IN, inout SurfaceOutput o) {
				BasicSurfaceShader(IN, o);	
			}
		ENDCG
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 300

		CGPROGRAM
			#pragma surface surf BlinnPhong 
			#pragma target 2.0

			#define VOSTOPIA_SPECULAR
			#include "vostopia.cginc"

			void surf (Input IN, inout SurfaceOutput o) {
				BasicSurfaceShader(IN, o);	
			}
		ENDCG
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 250

		CGPROGRAM
			#pragma surface surf Lambert 
			#pragma target 2.0

			#include "vostopia.cginc"

			void surf (Input IN, inout SurfaceOutput o) {
				BasicSurfaceShader(IN, o);	
			}
		ENDCG
	}

	SubShader
	{
		Tags { "LightMode" = "Vertex" }
		LOD 100

		Pass
		{

		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			
			#include "UnityCG.cginc"
			
			struct v2f {
			    float4 pos : SV_POSITION;
			    fixed3 diff : COLOR;
				fixed2 uv : TEXCOORD0;
			};

			v2f vert (appdata_base v) {
			    v2f o;
			    o.pos = mul( UNITY_MATRIX_MVP, v.vertex );
				o.uv = v.texcoord;
			    o.diff.xyz = ShadeVertexLights(v.vertex, v.normal);
			    return o;
			}
			
			uniform sampler2D _MainTex : register(s0);
			float4 _Color;
			
			fixed4 frag (v2f i) : COLOR
			{
				fixed4 color = tex2D(_MainTex, i.uv) * _Color;	
				fixed4 c;
				c.xyz = (color.xyz * i.diff) * 2;
				c.w = 1;
				return c;
			} 
		ENDCG
		}
	}
}
