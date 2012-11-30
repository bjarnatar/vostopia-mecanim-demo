Shader "Vostopia/VostopiaBasicGrit"
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

		_Dirt("Dirt Texture (r)", 2D) = "white" {}
		_DirtAmount("Dirt Amount", Range(0, 1)) = 1
		_DirtScale("Dirt Scale", Range(0, 10)) = 1
		_Saturation("Saturation", Range(0,1)) = 0.8
		_ColorScale("Color Scale", Range(0, 2)) = 1
	}
	
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 400

		CGPROGRAM
			#pragma surface surf BlinnPhong 
			#pragma target 3.0

			#define VOSTOPIA_GRIT
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
			#pragma surface surf BlinnPhong 
			#pragma target 3.0

			#define VOSTOPIA_GRIT
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

			#define VOSTOPIA_GRIT
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

			#define VOSTOPIA_GRIT
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

		Pass {
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
			    o.diff = ShadeVertexLights(v.vertex, v.normal);
			    return o;
			}
			
			uniform sampler2D _MainTex : register(s0);
			uniform sampler2D _Dirt : register(s1);
			uniform fixed _Saturation;
			uniform fixed _DirtAmount;
			uniform fixed _DirtScale;
			uniform fixed _ColorScale;
			
			fixed4 frag (v2f i) : COLOR
			{
				half3 color = tex2D(_MainTex, i.uv);	

				//subtract dirt
				half dirt = tex2D(_Dirt, i.uv * _DirtScale).r * _DirtAmount; 
				fixed gray = dot(color, half3(0.22, 0.707, 0.071));
				color = color - gray * dirt;

				//desaturate
				fixed gray2 = dot(color, half3(0.22, 0.707, 0.071));
				color = gray2 * (1-_Saturation) + color * _Saturation;

				//color scale
				color = color * _ColorScale;
				
				fixed4 c;
				c.rgb = (color * i.diff) * 2;
				c.a = 1;
				return c;
			} 

		ENDCG
		}
	}

}
