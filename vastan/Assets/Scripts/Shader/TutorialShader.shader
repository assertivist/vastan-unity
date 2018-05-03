// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/TutorialShader" {
	Properties {
		_AmbientColor ("Ambient Color", Color) = (1, 1, 1, 1)
		_AmbientIntensity ("Ambient Intensity", Range(0.0, 1.0)) = 1
	}
	SubShader {
		Pass {
			CGPROGRAM
			#pragma target 2.0
			#pragma vertex vertexShader
			#pragma fragment fragmentShader

			fixed4 _AmbientColor;
			fixed4 _AmbientIntensity;


			float4 vertexShader(float4 v:POSITION) : SV_POSITION
			{
				return UnityObjectToClipPos(v);
			}

			fixed4 fragmentShader() : SV_Target
			{
				return _AmbientColor * _AmbientIntensity;
			}
			ENDCG
		}
	}
}
