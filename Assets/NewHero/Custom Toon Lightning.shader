// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Clase07/CustomLightning"
{
	Properties
	{
		_HalfLambert("Half Lambert", Float) = 0
		_Ramp("Ramp", 2D) = "white" {}
		_RampTint("Ramp Tint", Color) = (0,0,0,0)
		_Diffuse("Diffuse", 2D) = "white" {}
		_RimOffset("Rim Offset", Float) = 0
		_RimPower("Rim Power", Float) = 0
		_RimTint("Rim Tint", Color) = (0,0,0,0)
		_NormalMap("Normal Map", 2D) = "bump" {}
		_NormalMapScale("Normal Map Scale", Float) = 0
		_OutlineTint("Outline Tint", Color) = (0,0,0,0)
		_OutlineWidth("Outline Width", Float) = 1
		_SpecularIntensity("Specular Intensity", Float) = 0
		_SpecularTexture("Specular Texture", 2D) = "white" {}
		_SpecularOffset("Specular Offset", Range( 0 , 1)) = 0
		_SpecularTiling("Specular Tiling", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ }
		Cull Front
		CGPROGRAM
		#pragma target 3.0
		#pragma surface outlineSurf Outline nofog  keepalpha noshadow noambient novertexlights nolightmap nodynlightmap nodirlightmap nometa noforwardadd vertex:outlineVertexDataFunc 
		void outlineVertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float outlineVar = _OutlineWidth;
			v.vertex.xyz += ( v.normal * outlineVar );
		}
		inline half4 LightingOutline( SurfaceOutput s, half3 lightDir, half atten ) { return half4 ( 0,0,0, s.Alpha); }
		void outlineSurf( Input i, inout SurfaceOutput o )
		{
			o.Emission = _OutlineTint.rgb;
		}
		ENDCG
		

		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "UnityShaderVariables.cginc"
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
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _Diffuse;
		uniform float4 _Diffuse_ST;
		uniform sampler2D _Ramp;
		uniform float _NormalMapScale;
		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _HalfLambert;
		uniform float4 _RampTint;
		uniform float _RimOffset;
		uniform float _RimPower;
		uniform float4 _RimTint;
		uniform sampler2D _SpecularTexture;
		uniform float _SpecularTiling;
		uniform float _SpecularOffset;
		uniform float _SpecularIntensity;
		uniform float _OutlineWidth;
		uniform float4 _OutlineTint;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			v.vertex.xyz += 0;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float2 uv_Diffuse = i.uv_texcoord * _Diffuse_ST.xy + _Diffuse_ST.zw;
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			float3 tex2DNode41 = UnpackScaleNormal( tex2D( _NormalMap, uv_NormalMap ), _NormalMapScale );
			float3 ase_worldPos = i.worldPos;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult4 = dot( (WorldNormalVector( i , tex2DNode41 )) , ase_worldlightDir );
			float2 temp_cast_0 = (saturate( (dotResult4*_HalfLambert + _HalfLambert) )).xx;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			UnityGI gi17 = gi;
			float3 diffNorm17 = WorldNormalVector( i , tex2DNode41 );
			gi17 = UnityGI_Base( data, 1, diffNorm17 );
			float3 indirectDiffuse17 = gi17.indirect.diffuse + diffNorm17 * 0.0001;
			float temp_output_32_0 = ( ase_lightAtten * dotResult4 );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult24 = dot( (WorldNormalVector( i , tex2DNode41 )) , ase_worldViewDir );
			float temp_output_33_0 = ( temp_output_32_0 * pow( ( 1.0 - saturate( ( dotResult24 + _RimOffset ) ) ) , _RimPower ) );
			float4 lerpResult40 = lerp( ( ( tex2D( _Diffuse, uv_Diffuse ) * ( tex2D( _Ramp, temp_cast_0 ) * _RampTint ) ) * ( ase_lightColor * float4( ( indirectDiffuse17 + ase_lightAtten ) , 0.0 ) ) ) , ( temp_output_33_0 * _RimTint ) , temp_output_33_0);
			float2 temp_cast_2 = (_SpecularTiling).xx;
			float2 uv_TexCoord57 = i.uv_texcoord * temp_cast_2;
			c.rgb = ( lerpResult40 + ( tex2D( _SpecularTexture, uv_TexCoord57 ).r * saturate( ( ( temp_output_32_0 - _SpecularOffset ) * _SpecularIntensity ) ) ) ).rgb;
			c.a = 1;
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows vertex:vertexDataFunc 

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
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
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
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
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
Version=16301
0;402;1358;598;1910.79;115.1348;2.945541;True;False
Node;AmplifyShaderEditor.CommentaryNode;44;-3241.472,654.6353;Float;False;615.0056;280;Normal Map;2;41;42;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;42;-3191.472,752.0671;Float;False;Property;_NormalMapScale;Normal Map Scale;8;0;Create;True;0;0;False;0;0;0.13;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;27;-2419.491,1057.978;Float;False;542.7188;401.3988;World View Direction;3;23;25;24;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;41;-2946.467,704.6353;Float;True;Property;_NormalMap;Normal Map;7;0;Create;True;0;0;False;0;None;4919f878f6d3d714e95dc65627e3b824;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;5;-2239.152,-27.70993;Float;False;621.1866;387.4998;World Space Light;3;2;1;4;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;25;-2369.491,1275.377;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;1;-2189.152,172.971;Float;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;39;-1688.413,1625.72;Float;False;1661.973;614.5522;Rim;9;28;29;31;30;33;34;35;37;38;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;23;-2361.995,1107.978;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;9;-1491.522,16.60774;Float;False;754.8818;303;Half Lambert;3;7;6;8;;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldNormalVector;2;-2144.702,22.29007;Float;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;24;-2111.773,1166.565;Float;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;4;-1852.967,106.7898;Float;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-1638.413,1976.776;Float;False;Property;_RimOffset;Rim Offset;4;0;Create;True;0;0;False;0;0;-0.21;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-1441.522,142.7339;Float;False;Property;_HalfLambert;Half Lambert;0;0;Create;True;0;0;False;0;0;0.62;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;21;-1363.146,462.6507;Float;False;900.3537;440.3503;Attenuation and Ambient Lights;5;16;17;18;19;20;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ScaleAndOffsetNode;6;-1180.133,66.60774;Float;True;3;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;28;-1449.945,1901.685;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;18;-1286.566,737.0007;Float;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;15;-559.9386,-240.1437;Float;False;809.1548;661.9346;Color;6;10;12;11;14;13;22;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;59;-1142.755,939.3579;Float;False;1282.954;569.6932;Specular Highlight;9;56;58;49;57;50;52;53;51;54;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SaturateNode;31;-1235.115,1904.802;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;8;-911.6404,70.23687;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;12;-438.5512,214.7911;Float;False;Property;_RampTint;Ramp Tint;2;0;Create;True;0;0;False;0;0,0,0,0;1,1,1,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;10;-509.9386,23.87909;Float;True;Property;_Ramp;Ramp;1;0;Create;True;0;0;False;0;None;c2d309c54bba0c842bebfe71aaf4e35d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;35;-1049.884,2125.27;Float;False;Property;_RimPower;Rim Power;5;0;Create;True;0;0;False;0;0;0.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;30;-1047.035,1904.679;Float;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;17;-1214.686,649.3787;Float;False;Tangent;1;0;FLOAT3;0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;56;-1092.755,1374.163;Float;False;Property;_SpecularOffset;Specular Offset;13;0;Create;True;0;0;False;0;0;0.6610416;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-1459.435,1057.255;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;58;-970.7313,1045.074;Float;False;Property;_SpecularTiling;Specular Tiling;14;0;Create;True;0;0;False;0;0;2.85;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;49;-765.5447,1178.868;Float;True;2;0;FLOAT;0;False;1;FLOAT;0.83;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-759.7056,1394.052;Float;False;Property;_SpecularIntensity;Specular Intensity;11;0;Create;True;0;0;False;0;0;1.75;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-206.2263,87.92134;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.PowerNode;34;-820.7169,1874.975;Float;True;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;19;-949.3664,650.0009;Float;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LightColorNode;16;-1313.146,512.6505;Float;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;14;-456.7041,-181.5523;Float;True;Property;_Diffuse;Diffuse;3;0;Create;True;0;0;False;0;None;643e3739d9a7f6f459a0d0ef19689d43;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-697.7919,514.1862;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;13;-91.60893,-52.82878;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-580.5789,1796.874;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;57;-705.7313,1028.073;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;-508.6407,1202.372;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;38;-545.5322,2021.867;Float;False;Property;_RimTint;Rim Tint;6;0;Create;True;0;0;False;0;0,0,0,0;0,1,0.2824192,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;52;-273.6779,1216.799;Float;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;54;-420.7813,989.3579;Float;True;Property;_SpecularTexture;Specular Texture;12;0;Create;True;0;0;False;0;None;aa1274b86be1d7c4a9874bf123757bce;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;97.69079,62.28283;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;37;-261.44,1829.229;Float;True;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;48;493.4982,1012.424;Float;False;605.9996;337.0007;Outline;3;46;47;45;;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;46;543.4982,1062.423;Float;False;Property;_OutlineTint;Outline Tint;9;0;Create;True;0;0;False;0;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;40;348.4572,482.7577;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;53;-94.80064,1136.516;Float;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;47;628.4977,1234.424;Float;False;Property;_OutlineWidth;Outline Width;10;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OutlineNode;45;849.4977,1079.423;Float;False;0;True;None;0;0;Front;3;0;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;55;689.6432,593.509;Float;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1227.53,538.382;Float;False;True;2;Float;ASEMaterialInspector;0;0;CustomLighting;Clase07/CustomLightning;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;41;5;42;0
WireConnection;23;0;41;0
WireConnection;2;0;41;0
WireConnection;24;0;23;0
WireConnection;24;1;25;0
WireConnection;4;0;2;0
WireConnection;4;1;1;0
WireConnection;6;0;4;0
WireConnection;6;1;7;0
WireConnection;6;2;7;0
WireConnection;28;0;24;0
WireConnection;28;1;29;0
WireConnection;31;0;28;0
WireConnection;8;0;6;0
WireConnection;10;1;8;0
WireConnection;30;0;31;0
WireConnection;17;0;41;0
WireConnection;32;0;18;0
WireConnection;32;1;4;0
WireConnection;49;0;32;0
WireConnection;49;1;56;0
WireConnection;11;0;10;0
WireConnection;11;1;12;0
WireConnection;34;0;30;0
WireConnection;34;1;35;0
WireConnection;19;0;17;0
WireConnection;19;1;18;0
WireConnection;20;0;16;0
WireConnection;20;1;19;0
WireConnection;13;0;14;0
WireConnection;13;1;11;0
WireConnection;33;0;32;0
WireConnection;33;1;34;0
WireConnection;57;0;58;0
WireConnection;50;0;49;0
WireConnection;50;1;51;0
WireConnection;52;0;50;0
WireConnection;54;1;57;0
WireConnection;22;0;13;0
WireConnection;22;1;20;0
WireConnection;37;0;33;0
WireConnection;37;1;38;0
WireConnection;40;0;22;0
WireConnection;40;1;37;0
WireConnection;40;2;33;0
WireConnection;53;0;54;1
WireConnection;53;1;52;0
WireConnection;45;0;46;0
WireConnection;45;1;47;0
WireConnection;55;0;40;0
WireConnection;55;1;53;0
WireConnection;0;13;55;0
WireConnection;0;11;45;0
ASEEND*/
//CHKSM=DA10FEDB10BDFB5E4CD3E6B33E84066A70CF59C5