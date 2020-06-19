// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Procedural Worlds/Horizon"
{
	Properties
	{
		_Scattering("Scattering", Range( 0 , 1)) = 1
		_FogDensity("Fog Density", Range( 0 , 1)) = 0.7
		_HorizonFalloff("Horizon Falloff", Range( 0 , 1)) = 0.65
		_HorizonBlend("Horizon Blend", Range( 0 , 1)) = 0.7
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent-350" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma surface surf StandardSpecular alpha:fade keepalpha noshadow 
		struct Input
		{
			float3 worldNormal;
			float4 screenPos;
		};

		uniform float _Scattering;
		uniform float _FogDensity;
		uniform float _HorizonBlend;
		uniform float _HorizonFalloff;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float4 temp_cast_0 = (1.1).xxxx;
			float4 temp_cast_1 = (0.1).xxxx;
			o.Albedo = ( ( unity_AmbientGround - temp_cast_0 ) + ( temp_cast_1 - unity_FogColor ) ).rgb;
			float temp_output_19_0_g61 = _Scattering;
			float3 ase_worldNormal = i.worldNormal;
			float3 ase_normWorldNormal = normalize( ase_worldNormal );
			float temp_output_10_0_g61 = (1.0 + (ase_normWorldNormal.y - 0.0) * (2.0 - 1.0) / (0.21 - 0.0));
			float clampResult16_g61 = clamp( ( temp_output_19_0_g61 * ( 1.0 - temp_output_10_0_g61 ) ) , 0.0 , 1.0 );
			float4 temp_cast_3 = (clampResult16_g61).xxxx;
			float4 lerpResult36_g61 = lerp( unity_AmbientSky , temp_cast_3 , temp_output_19_0_g61);
			float temp_output_30_0_g61 = _FogDensity;
			float temp_output_49_0_g61 = ( temp_output_30_0_g61 * 2.0 );
			o.Emission = ( ( ( 1.0 - ( lerpResult36_g61 * temp_output_49_0_g61 * unity_FogColor ) ) + ( 1.0 - saturate( ( ( unity_AmbientSky + unity_FogColor ) * _HorizonBlend ) ) ) ) + temp_output_30_0_g61 ).rgb;
			float4 temp_output_168_26 = ( unity_AmbientSky * 0.3 );
			o.Specular = temp_output_168_26.rgb;
			o.Smoothness = temp_output_168_26.r;
			float clampResult14_g61 = clamp( ( _HorizonFalloff * temp_output_10_0_g61 ) , 0.0 , 21.0 );
			float clampResult29_g61 = clamp( ( ( temp_output_10_0_g61 - clampResult14_g61 ) + 0.05 ) , 0.0 , temp_output_49_0_g61 );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth126_g61 = LinearEyeDepth(UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture,UNITY_PROJ_COORD( ase_screenPos ))));
			float distanceDepth126_g61 = saturate( abs( ( screenDepth126_g61 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( 150.0 ) ) );
			o.Alpha = ( clampResult29_g61 * distanceDepth126_g61 );
		}

		ENDCG
	}
	//CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16600
439;34;824;926;771.106;203.4198;1.189937;True;False
Node;AmplifyShaderEditor.CommentaryNode;49;-969.1581,9.478146;Float;False;895.7869;508.5767;Main Setup;4;60;40;36;78;Main Setup;0,0.5450981,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;60;-917.0861,62.66229;Float;False;Property;_FogDensity;Fog Density;1;0;Create;True;0;0;False;0;0.7;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-913.0358,363.0549;Float;False;Property;_Scattering;Scattering;0;0;Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-911.1581,150.4781;Float;False;Property;_HorizonFalloff;Horizon Falloff;2;0;Create;True;0;0;False;0;0.65;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;78;-914.0861,254.6623;Float;False;Property;_HorizonBlend;Horizon Blend;3;0;Create;True;0;0;False;0;0.7;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;168;-597.2195,152.8434;Float;False;Horizon Falloff;-1;;61;fe878580c8681da4b8b8ba7902200d1b;0;4;30;FLOAT;0.1;False;18;FLOAT;0.7;False;71;FLOAT;0;False;19;FLOAT;1;False;4;COLOR;131;COLOR;4;COLOR;26;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;-26,1.9;Float;False;True;2;Float;ASEMaterialInspector;0;0;StandardSpecular;Procedural Worlds/Horizon;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;-0.16;True;False;-350;False;Transparent;;Transparent;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;168;30;60;0
WireConnection;168;18;36;0
WireConnection;168;71;78;0
WireConnection;168;19;40;0
WireConnection;0;0;168;131
WireConnection;0;2;168;4
WireConnection;0;3;168;26
WireConnection;0;4;168;26
WireConnection;0;9;168;0
ASEEND*/
//CHKSM=819482C063D0365C78ACBF7E6ED44A900B150EAB