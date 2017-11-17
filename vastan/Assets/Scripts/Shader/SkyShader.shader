Shader "Custom/SkyShader" {
	Properties {
		_SkyColor ("Sky Color", Color) = (1, 1, 1, 1)
		_HorizColor ("Horizon Color", Color) = (.5, .5, .5, 1)
		_GroundColor ("Ground Color", Color) = (0, 0, 0, 1)
		_GradientHeight ("Gradient Height", Range(0.0, 1.0)) = .7
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

			float4 _SkyColor;
			float4 _HorizColor;
			float4 _GroundColor;
			float _GradientHeight;

			struct VertexInput {
				float4 vertex : POSITION;
			};
			struct VertexOutput {
				float4 pos : SV_POSITION;
				float4 tex_vector : TEXCOORD0;
			};

			VertexOutput vert(VertexInput v) {
				VertexOutput o = (VertexOutput)0;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.tex_vector = mul(unity_ObjectToWorld, v.vertex);
				return o;
			}

			float4 frag(VertexOutput i) : COLOR {
				float phi = normalize(i.tex_vector).y;
				float4 frag_color;
				if (phi <= 0.0) {
					frag_color = _GroundColor;
				}
				if (phi > _GradientHeight) {
					frag_color = _SkyColor;
				}
				if (0.0 < phi && phi < _GradientHeight) {
					float grad = phi / _GradientHeight;
					frag_color = mul(_SkyColor, grad) + mul(_HorizColor, (1.0 - grad));
				}
				return frag_color;
			}

			ENDCG
		}
	}
}
