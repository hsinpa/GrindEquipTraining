// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "RRFreelance_Hand/AS_HandSkinShader"
{
	Properties
	{
		_Base_Albedo("Base_Albedo", 2D) = "white" {}
		_Base_Tint("Base_Tint", Color) = (1,1,1,0)
		_MetalSmooth("Metal-Smooth", 2D) = "white" {}
		_SmoothnessAdd("SmoothnessAdd", Range( -1 , 1)) = 0
		_NormalMap("NormalMap", 2D) = "bump" {}
		_OptionalMask("OptionalMask", 2D) = "black" {}
		_sssTint("sssTint", Color) = (0,0,0,0)
		_replaceNailTone("replaceNailTone", Range( 0 , 1)) = 0
		_NailToneSmoothAdd("NailTone-SmoothAdd", Color) = (0,0,0,0)
		_PalmToneSmoothMult("PalmTone-SmoothMult", Color) = (0,0,0,0)
		[Toggle]_Invert_TattooColors("Invert_TattooColors", Float) = 0
		_Tattoo("Tattoo", 2D) = "black" {}
		_TattooTintFadeA("TattooTint-Fade(A)", Color) = (0.07850347,0.07920894,0.08088237,0.909)
		_BloodTexture("BloodTexture", 2D) = "white" {}
		_BloodAmount("BloodAmount", Range( 0 , 1)) = 0
		_AddBloodGloss("AddBloodGloss", Range( 0 , 1)) = 0
		_SSS_FresnelControl("SSS_FresnelControl", Vector) = (3,0.25,5,0)
		_SSS_Bias("SSS_Bias", Range( 0 , 1)) = 0.33
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform sampler2D _Base_Albedo;
		uniform float4 _Base_Albedo_ST;
		uniform float4 _Base_Tint;
		uniform sampler2D _OptionalMask;
		uniform float4 _OptionalMask_ST;
		uniform float4 _PalmToneSmoothMult;
		uniform float4 _NailToneSmoothAdd;
		uniform float _replaceNailTone;
		uniform float _Invert_TattooColors;
		uniform sampler2D _Tattoo;
		uniform float4 _Tattoo_ST;
		uniform float4 _TattooTintFadeA;
		uniform sampler2D _BloodTexture;
		uniform float4 _BloodTexture_ST;
		uniform float _BloodAmount;
		uniform float4 _sssTint;
		uniform float3 _SSS_FresnelControl;
		uniform float _SSS_Bias;
		uniform sampler2D _MetalSmooth;
		uniform float4 _MetalSmooth_ST;
		uniform float _AddBloodGloss;
		uniform float _SmoothnessAdd;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			o.Normal = UnpackNormal( tex2D( _NormalMap, uv_NormalMap ) );
			float2 uv_Base_Albedo = i.uv_texcoord * _Base_Albedo_ST.xy + _Base_Albedo_ST.zw;
			float4 tex2DNode1 = tex2D( _Base_Albedo, uv_Base_Albedo );
			float2 uv_OptionalMask = i.uv_texcoord * _OptionalMask_ST.xy + _OptionalMask_ST.zw;
			float4 tex2DNode7 = tex2D( _OptionalMask, uv_OptionalMask );
			float4 temp_output_20_0 = ( tex2DNode7.b * _PalmToneSmoothMult * tex2DNode1 );
			float4 lerpResult19 = lerp( ( tex2DNode1 * _Base_Tint ) , ( tex2DNode1 + temp_output_20_0 + temp_output_20_0 ) , tex2DNode7.b);
			float4 temp_cast_0 = (0.5).xxxx;
			float4 lerpResult21 = lerp( lerpResult19 , ( tex2DNode7.g * _NailToneSmoothAdd * pow( tex2DNode1 , temp_cast_0 ) ) , ( tex2DNode7.g * _replaceNailTone ));
			float2 uv_Tattoo = i.uv_texcoord * _Tattoo_ST.xy + _Tattoo_ST.zw;
			float4 tex2DNode37 = tex2D( _Tattoo, uv_Tattoo );
			float4 lerpResult36 = lerp( lerpResult21 , ( (( _Invert_TattooColors )?( ( 1.0 - tex2DNode37 ) ):( tex2DNode37 )) * _TattooTintFadeA ) , ( (( _Invert_TattooColors )?( ( 1.0 - tex2DNode37 ) ):( tex2DNode37 )) * _TattooTintFadeA.a ));
			float2 uv_BloodTexture = i.uv_texcoord * _BloodTexture_ST.xy + _BloodTexture_ST.zw;
			float4 tex2DNode43 = tex2D( _BloodTexture, uv_BloodTexture );
			float4 lerpResult45 = lerp( lerpResult36 , tex2DNode43 , ( tex2DNode43.a * _BloodAmount ));
			o.Albedo = lerpResult45.rgb;
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float dotResult55 = dot( ase_worldlightDir , ase_vertexNormal );
			float clampResult58 = clamp( dotResult55 , 0.0 , 1.0 );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float fresnelNdotV59 = dot( ase_worldlightDir, ase_worldViewDir );
			float fresnelNode59 = ( _SSS_FresnelControl.x + _SSS_FresnelControl.y * pow( 1.0 - fresnelNdotV59, _SSS_FresnelControl.z ) );
			float4 lerpResult63 = lerp( ( ( tex2DNode7.r * _sssTint ) * clampResult58 * ase_lightColor * fresnelNode59 ) , ( clampResult58 * _sssTint * ase_lightColor ) , _SSS_Bias);
			o.Emission = lerpResult63.rgb;
			float2 uv_MetalSmooth = i.uv_texcoord * _MetalSmooth_ST.xy + _MetalSmooth_ST.zw;
			float4 tex2DNode4 = tex2D( _MetalSmooth, uv_MetalSmooth );
			o.Metallic = tex2DNode4.r;
			float clampResult24 = clamp( ( ( tex2DNode43.a * _AddBloodGloss * _BloodAmount ) + ( ( tex2DNode7.b * _PalmToneSmoothMult.a * _PalmToneSmoothMult.a ) + ( tex2DNode7.g * _NailToneSmoothAdd.a ) + ( tex2DNode4.a + _SmoothnessAdd ) ) ) , 0.0 , 1.0 );
			o.Smoothness = clampResult24;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows exclude_path:deferred 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18800
0;207.3333;2560;1180;188.3683;-452.4802;1.04772;True;False
Node;AmplifyShaderEditor.ColorNode;15;-493.4711,611.4734;Float;False;Property;_PalmToneSmoothMult;PalmTone-SmoothMult;9;0;Create;True;0;0;0;False;0;False;0,0,0,0;1,1,1,0.5490196;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-559.9107,403.2025;Inherit;True;Property;_OptionalMask;OptionalMask;5;0;Create;True;0;0;0;False;0;False;-1;None;6410416828848c049982bb2ec4f15701;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-664.7747,-336.2899;Inherit;True;Property;_Base_Albedo;Base_Albedo;0;0;Create;True;0;0;0;False;0;False;-1;None;7542e019f371d3545a6b672eacec98ab;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;93.06075,339.6524;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;37;384.8344,-588.5651;Inherit;True;Property;_Tattoo;Tattoo;11;0;Create;True;0;0;0;False;0;False;-1;None;0fa61d2432f6a874ab41877694cda37c;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;2;-468.2296,-104.9784;Float;False;Property;_Base_Tint;Base_Tint;1;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.9150943,0.8748071,0.8287647,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;34;-2.75795,960.9005;Float;False;Constant;_Float0;Float 0;10;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;60;719.4197,901.0499;Inherit;False;684.9242;538.3793;SimpleSSS;7;54;53;55;59;58;57;61;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;320.7151,238.4878;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;3;-56.37962,-212.5566;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;4;-554.7065,70.14758;Inherit;True;Property;_MetalSmooth;Metal-Smooth;2;0;Create;True;0;0;0;False;0;False;-1;None;ac17406779b36fc4d8237c1422765256;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;41;459.954,162.0128;Float;False;Property;_replaceNailTone;replaceNailTone;7;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;52;827.3218,-438.8076;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;53;769.4197,951.0499;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalVertexDataNode;54;815.7297,1087.087;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;5;-533.8904,280.0416;Float;False;Property;_SmoothnessAdd;SmoothnessAdd;3;0;Create;True;0;0;0;False;0;False;0;-0.15;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;10;-493.712,986.5991;Float;False;Property;_NailToneSmoothAdd;NailTone-SmoothAdd;8;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.235849,0.235849,0.235849,0.6196079;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;33;172.8594,868.3171;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.ToggleSwitchNode;50;881.0707,-576.7609;Float;False;Property;_Invert_TattooColors;Invert_TattooColors;10;0;Create;True;0;0;0;False;0;False;0;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;181.1305,1057.461;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;-109.0783,201.5849;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;44;1540.368,233.4861;Float;False;Property;_AddBloodGloss;AddBloodGloss;15;0;Create;True;0;0;0;False;0;False;0;0.517;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;42;707.15,89.86047;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;43;1509.569,-81.39529;Inherit;True;Property;_BloodTexture;BloodTexture;13;0;Create;True;0;0;0;False;0;False;-1;None;c346fbba5f4b05742b647b03a6968d6c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;450.8839,442.6782;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;61;754.3005,1252.579;Inherit;False;Property;_SSS_FresnelControl;SSS_FresnelControl;16;0;Create;True;0;0;0;False;0;False;3,0.25,5;3,0.83,5;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;8;-487.671,794.7775;Float;False;Property;_sssTint;sssTint;6;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.5943396,0.06167669,0.06167669,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;397.7692,661.102;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;19;403.0866,-68.24194;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DotProductOpNode;55;1055.965,1001.702;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;46;1547.239,131.3274;Float;False;Property;_BloodAmount;BloodAmount;14;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;38;456.0923,-300.4926;Float;False;Property;_TattooTintFadeA;TattooTint-Fade(A);12;0;Create;True;0;0;0;False;0;False;0.07850347,0.07920894,0.08088237,0.909;0.01427337,0.0146701,0.0147059,0.716;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;1023.808,656.2542;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;21;842.415,-88.24806;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;40;1270.687,-486.65;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LightColorNode;57;1154.344,1109.507;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;39;1273.873,-377.6393;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;27;841.7479,408.3146;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;58;1193.756,966.0856;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;59;1076.253,1233.429;Inherit;False;Standard;WorldNormal;ViewDir;False;False;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;48;1866.14,269.6754;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;1498.621,870.1412;Inherit;False;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;49;2090.994,382.0895;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;64;1687.145,961.2446;Inherit;False;Property;_SSS_Bias;SSS_Bias;17;0;Create;True;0;0;0;False;0;False;0.33;0.371;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;36;1605.792,-366.6876;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;56;1454.853,726.3665;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;2207.34,96.19354;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;24;2311.307,406.9689;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;63;1684.145,814.2446;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;18;893.6025,138.5529;Inherit;True;Property;_NormalMap;NormalMap;4;0;Create;True;0;0;0;False;0;False;-1;None;83bfc4b9c3b3fe24aa974bb2be6dbb36;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;45;2612.929,-34.09534;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2800.837,327.2232;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;RRFreelance_Hand/AS_HandSkinShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;20;0;7;3
WireConnection;20;1;15;0
WireConnection;20;2;1;0
WireConnection;28;0;1;0
WireConnection;28;1;20;0
WireConnection;28;2;20;0
WireConnection;3;0;1;0
WireConnection;3;1;2;0
WireConnection;52;0;37;0
WireConnection;33;0;1;0
WireConnection;33;1;34;0
WireConnection;50;0;37;0
WireConnection;50;1;52;0
WireConnection;26;0;7;2
WireConnection;26;1;10;4
WireConnection;23;0;4;4
WireConnection;23;1;5;0
WireConnection;42;0;7;2
WireConnection;42;1;41;0
WireConnection;25;0;7;3
WireConnection;25;1;15;4
WireConnection;25;2;15;4
WireConnection;22;0;7;2
WireConnection;22;1;10;0
WireConnection;22;2;33;0
WireConnection;19;0;3;0
WireConnection;19;1;28;0
WireConnection;19;2;7;3
WireConnection;55;0;53;0
WireConnection;55;1;54;0
WireConnection;11;0;7;1
WireConnection;11;1;8;0
WireConnection;21;0;19;0
WireConnection;21;1;22;0
WireConnection;21;2;42;0
WireConnection;40;0;50;0
WireConnection;40;1;38;4
WireConnection;39;0;50;0
WireConnection;39;1;38;0
WireConnection;27;0;25;0
WireConnection;27;1;26;0
WireConnection;27;2;23;0
WireConnection;58;0;55;0
WireConnection;59;0;53;0
WireConnection;59;1;61;1
WireConnection;59;2;61;2
WireConnection;59;3;61;3
WireConnection;48;0;43;4
WireConnection;48;1;44;0
WireConnection;48;2;46;0
WireConnection;62;0;58;0
WireConnection;62;1;8;0
WireConnection;62;2;57;0
WireConnection;49;0;48;0
WireConnection;49;1;27;0
WireConnection;36;0;21;0
WireConnection;36;1;39;0
WireConnection;36;2;40;0
WireConnection;56;0;11;0
WireConnection;56;1;58;0
WireConnection;56;2;57;0
WireConnection;56;3;59;0
WireConnection;47;0;43;4
WireConnection;47;1;46;0
WireConnection;24;0;49;0
WireConnection;63;0;56;0
WireConnection;63;1;62;0
WireConnection;63;2;64;0
WireConnection;45;0;36;0
WireConnection;45;1;43;0
WireConnection;45;2;47;0
WireConnection;0;0;45;0
WireConnection;0;1;18;0
WireConnection;0;2;63;0
WireConnection;0;3;4;0
WireConnection;0;4;24;0
ASEEND*/
//CHKSM=29EB582BB0FF6F3BC210049625C37C7B215C0EB9