Shader "Vostopia/VostopiaAnimatedTextureShader"
{
	Properties 
	{
		_MainTex("Main Texture", 2D) = "white" {}
        _Color("Main Color", Color) = (1,1,1,1)
		_FrameOffset("Frame Offset", Vector) = (0, 0, 0, 0)
		_FrameSize("Frame Size", Vector) = (1, 1, 0, 0)
	}
	
	SubShader 
	{
		LOD 100
		Cull Back 
		ZTest LEqual 
		Tags
		{
			"Queue"="Geometry+2"
			"IgnoreProjector"="False"
			"RenderType"="Transparent"
		}

		CGPROGRAM
			#pragma surface surf Lambert alpha

			struct Input {
				float2 uv_MainTex;
			};

			float4 _Color;
			sampler2D _MainTex;
			float2 _FrameOffset;
			float2 _FrameSize;

			void surf (Input IN, inout SurfaceOutput o) {
				//Use frame offset to show the desired frame
				float4 col = tex2D(_MainTex, IN.uv_MainTex + _FrameOffset);
			
				//only show what's inside the frame
				float alpha = col.a * _Color.a;
				alpha *= all(IN.uv_MainTex >= 0);
				alpha *= all(IN.uv_MainTex <= _FrameSize);
	
				o.Albedo = col.rgb * _Color.rgb;
				o.Alpha = alpha;
			}
		ENDCG

	}
}
