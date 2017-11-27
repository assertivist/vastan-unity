Shader "Custom/SkyShader" {
	Properties {
		_SkyColor ("Sky Color", Color) = (1, 1, 1, 1)
		_HorizColor ("Horizon Color", Color) = (.5, .5, .5, 1)
		_GroundColor ("Ground Color", Color) = (0, 0, 0, 1)
		_GradientHeight ("Gradient Height", Range(0.0, 1.0)) = .7
		_Starfield ("Starfield", Range(0.0, 1.0)) = .8
		
	}
	SubShader {
		Tags{
			"Queue" = "Background"
			"RenderType" = "Opaque"
			"PreviewType" = "Skybox"
		}
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Noise.hlsl"

			float4 _SkyColor;
			float4 _HorizColor;
			float4 _GroundColor;
			float _GradientHeight;
			float _Starfield;

			struct VertexInput {
				float4 vertex : POSITION;
			};

			struct VertexOutput {
				float4 pos : SV_POSITION;
				float2 tex_vector : TEXCOORD0;
			};

			VertexOutput vert(VertexInput v) {
				VertexOutput o = (VertexOutput)0;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.tex_vector = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}
			/*
			float4 star_or_sky(float4 sky, float2 tex) {
				float dist = 6;
				float r = (cnoise(pow(tex, 2)) \
					+ cnoise(float2(tex.x, tex.y + dist))\
					+ cnoise(float2(tex.x + dist, tex.y)) \
					+ cnoise(float2(tex.x + dist, tex.y + dist))\
					+ cnoise(float2(tex.x, tex.y - dist))\
					+ cnoise(float2(tex.x - dist, tex.y - dist))\
					+ cnoise(float2(tex.x - dist, tex.y)))\
					/ 7;
				float thing = pow(r, 2);
				float other = pow(r, 4);
				float star = step(thing, (1 - _Starfield));//pow(r, .3), (1 - _Starfield));
				return lerp(lerp(float4(1, 1, 1, 1), sky, .2), sky, star);
			}
			*/
			float4 frag(VertexOutput i) : COLOR{
				float phi = normalize(i.tex_vector).y;
				//float2 p = i.tex_vector;
				float4 frag_color;
				if (phi <= 0.0) {
					frag_color = _GroundColor;
				}
				if (phi > _GradientHeight) {
					frag_color = _SkyColor; // star_or_sky(_SkyColor, p);
				}
				if (0.0 < phi && phi < _GradientHeight) {
					float grad = phi / _GradientHeight;
					float4 gcolor = mul(_SkyColor, grad) + mul(_HorizColor, (1.0 - grad));
					frag_color = gcolor; // (gcolor, p);
				}
				return frag_color;
			}

			ENDCG
		}
	}
}
