#ifndef VOSTOPIA_INCLUDE
#define VOSTOPIA_INCLUDE

#ifdef VOSTOPIA_GRIT
#define VOSTOPIA_DIRT
#define VOSTOPIA_DESATURATE
#define VOSTOPIA_COLOR_SCALE
#endif

//Variables for all shader parameters
half4 _Color;
sampler2D _MainTex;
sampler2D _MapTSI;

#if defined(VOSTOPIA_REFLECTION)
	samplerCUBE _Cube;
	sampler2D _MapNR;
#elif defined(VOSTOPIA_NORMAL)
	sampler2D _MapNR;
#endif

#ifdef VOSTOPIA_TOON
	sampler2D _Ramp;
	half4 _OutlineColor;
	half _Outline;
#endif

#ifdef VOSTOPIA_SPECULAR
	float _ShininessDropoff;
	float _Shininess;
#endif

#ifdef VOSTOPIA_DESATURATE
	float _Saturation;
#endif

#ifdef VOSTOPIA_DIRT
	sampler2D _Dirt;
	float _DirtAmount;
	float _DirtScale;
#endif

#ifdef VOSTOPIA_COLOR_SCALE
	float _ColorScale;
#endif

#ifdef VOSTOPIA_LAYER
	sampler2D _StampMask;
#endif

struct Input {
	float2 uv_MainTex;
	#ifdef VOSTOPIA_REFLECTION
		float3 worldRefl;
		INTERNAL_DATA
	#endif
};

inline fixed3 ExtractNormal(fixed4 packednormal)
{
	fixed3 normal;
	normal.yx = packednormal.xy * 2 - 1;
	normal.z = sqrt(1 - normal.x*normal.x - normal.y * normal.y);
	return normal;
}


//Toon Shading
#ifdef VOSTOPIA_TOON
inline half4 LightingToon (SurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
{
	#ifndef USING_DIRECTIONAL_LIGHT
	lightDir = normalize(lightDir);
	#endif

	half d = dot (s.Normal, lightDir)*0.5 + 0.5;
	half3 ramp = tex2D (_Ramp, float2(d,d)).rgb;
	
	half4 c;
	c.rgb = s.Albedo * _LightColor0.rgb * ramp * (atten * 2);
	c.a = 0;
	return c;
}
#endif

inline void BasicSurfaceShader(Input IN, inout SurfaceOutput o)
{
	half4 color = tex2D(_MainTex, IN.uv_MainTex) * _Color;

	#ifdef VOSTOPIA_NORMAL
		half3 normal = ExtractNormal(tex2D(_MapNR, IN.uv_MainTex));
	#else
		half3 normal = half3(0,0,1);
	#endif
	
	//transparency, specular, self illumination
	half3 tsi = tex2D(_MapTSI, IN.uv_MainTex);
	half transparency = color.a;
	half illum = tsi.z;

	//setup output
	o.Albedo = color;
	o.Normal = normal;
	o.Emission = (color * illum);

	#ifdef VOSTOPIA_SPECULAR
		half spec = tsi.y;
		o.Specular = _ShininessDropoff;
		o.Gloss = spec * _Shininess;
	#endif

	#ifdef VOSTOPIA_REFLECTION
		half reflection = tex2D(_MapNR, IN.uv_MainTex).z;
		half4 reflCol = texCUBE (_Cube, WorldReflectionVector(IN, o.Normal)) * reflection;
		o.Emission += reflCol;
	#endif

	#if defined(VOSTOPIA_TRANSPARENT)
		o.Alpha = transparency;
	#elif defined(VOSTOPIA_LAYER)
		o.Alpha = color.a * tex2D(_StampMask, IN.uv_MainTex).a;
	#else
		o.Alpha = 1;
	#endif

	#ifdef VOSTOPIA_DIRT
		half dirt = tex2D(_Dirt, IN.uv_MainTex * _DirtScale).r * _DirtAmount; 
		half gray = dot(o.Albedo.rgb, half3(0.22, 0.707, 0.071));
		o.Albedo = o.Albedo - gray * dirt;
	#endif

	#ifdef VOSTOPIA_DESATURATE
		gray = dot(o.Albedo.rgb, half3(0.22, 0.707, 0.071));
		o.Albedo = gray * (1-_Saturation) + o.Albedo * _Saturation;
	#endif

	#ifdef VOSTOPIA_COLOR_SCALE
		o.Albedo = _ColorScale * o.Albedo;
	#endif
}

#endif
