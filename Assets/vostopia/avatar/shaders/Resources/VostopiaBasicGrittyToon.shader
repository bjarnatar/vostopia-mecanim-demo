Shader "Vostopia/VostopiaBasicGrittyToon"
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

		_Ramp("Toon Ramp", 2D) = "gray" {}
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)
		_Outline ("Outline width", Range (.002, 0.03)) = .005
	}
	
	SubShader 
	{
		LOD 250
		UsePass "Toon/Basic Outline/OUTLINE"

		CGPROGRAM
			#pragma surface surf Toon
			#pragma target 2.0

			#define VOSTOPIA_TOON
			#define VOSTOPIA_GRIT
			#include "vostopia.cginc"

			void surf (Input IN, inout SurfaceOutput o) 
			{
				BasicSurfaceShader(IN, o);
			}
		ENDCG
	}

	SubShader
	{
		LOD 100
		Tags
		{
			"LightMode" = "Vertex"
		}
	
		UsePass "Toon/Basic Outline/OUTLINE"
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma fragmentoption ARB_precision_hint_fastest
				
				#include "UnityCG.cginc"
		
				float4 ToonShadeVertexLights (float4 vertex, float3 normal)
				{
					float3 viewpos = mul (UNITY_MATRIX_MV, vertex).xyz;
					float3 viewN = mul ((float3x3)UNITY_MATRIX_IT_MV, normal);
					float4 light;
					light.xyz = UNITY_LIGHTMODEL_AMBIENT.xyz;
					light.w = (light.x + light.y + light.z) / 3;
					for (int i = 0; i < 2; i++) {
						float3 toLight = unity_LightPosition[i].xyz - viewpos.xyz * unity_LightPosition[i].w;
						float lengthSq = dot(toLight, toLight);
						float atten = 1.0 / (1.0 + lengthSq * unity_LightAtten[i].z);
						float diff = max (0, dot (viewN, normalize(toLight)));
						light.xyz += unity_LightColor[i].rgb * (diff * atten);
						light.w += diff;
					}
					return light;
				}

		
				struct v2f {
				    float4 pos : SV_POSITION;
				    fixed4 light : COLOR;
					fixed2 uv : TEXCOORD0;
				};
	
				v2f vert (appdata_base v) {
				    v2f o;
				    o.pos = mul( UNITY_MATRIX_MVP, v.vertex );
					o.uv = v.texcoord;
				    o.light = ToonShadeVertexLights(v.vertex, v.normal);
				    return o;
				}
				
				uniform sampler2D _MainTex : register(s0);
				uniform sampler2D _Dirt : register(s1);
				uniform fixed _Saturation;
				uniform fixed _DirtAmount;
				uniform fixed _DirtScale;
				uniform fixed _ColorScale;
				uniform sampler2D _Ramp : register(s2);
				
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
					
					fixed3 ramp = tex2D(_Ramp, fixed2(i.light.w, i.light.w));
					fixed4 c;
					c.rgb = (color * ramp * i.light.xyz) * 2;
					c.a = 1;
					return c;
				} 
			ENDCG
		}
	}
}
