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
			#define fadeout(f, fAverage, fFeatureSize, fWidth)\
				lerp(f, fAverage, smoothstep(0.2, 0.6, fWidth / fFeatureSize))
			#define filterednoise(x, w) fadeout(cnoise(x), 0.5, 1, w)

			float fBm(float3 vInputCoords, float nNumOctaves, float fLacunarity, float fInGain, float fFilterWidth) { 
				float fNoiseSum = 0; 
				float fAmplitude = 1; 
				float fAmplitudeSum = 0; 
				float fFilterWidthPerBand = fFilterWidth; 
				float3 vSampleCoords = vInputCoords; 
				for (int i = 0; i < nNumOctaves; i += 1) {
					fNoiseSum += fAmplitude * filterednoise(vSampleCoords, fFilterWidthPerBand);
					fAmplitudeSum += fAmplitude; 
					fFilterWidthPerBand *= fLacunarity;      
					fAmplitude *= fInGain; 
					vSampleCoords *= fLacunarity;
				}
				fNoiseSum /= fAmplitudeSum; 
				return fNoiseSum; 
			}

			float4 _SkyColor;
			float4 _HorizColor;
			float4 _GroundColor;
			float _GradientHeight;
			float _Starfield;
			float _Seed;

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

			float4 star_or_sky(float4 sky, float2 tex) {
				//float r = cnoise(tex);
				float r = fBm(float3(tex, 1), 4, .1, 1, .1);
				bool star = r > (1 - _Starfield);
				if (star) {
					float term = cnoise(tex);
					return lerp(float4(1, 1, 1, 1), sky, term);
				}
				else return sky;
			}

			float4 frag(VertexOutput i) : COLOR{
				float phi = normalize(i.tex_vector).y;
				float2 p = i.tex_vector;
				float4 frag_color;
				if (phi <= 0.0) {
					frag_color = _GroundColor;
				}
				if (phi > _GradientHeight) {
					frag_color = star_or_sky(_SkyColor, p);
				}
				if (0.0 < phi && phi < _GradientHeight) {
					float grad = phi / _GradientHeight;
					float4 gcolor = mul(_SkyColor, grad) + mul(_HorizColor, (1.0 - grad));
					frag_color = star_or_sky(gcolor, p);
				}
				return frag_color;
			}

			ENDCG
		}
	}
}
