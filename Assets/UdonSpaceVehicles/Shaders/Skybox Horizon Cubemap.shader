// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Skybox/Horizon Cubemap"
{
	Properties
	{
		_Tex("Tex", CUBE) = "black" {}
		_GroundTex("Ground Tex", CUBE) = "black" {}
		_PlanetRadius("Planet Radius", Float) = 6321000
		_ViewHeight("View Height", Float) = 2
		_Rotation("Rotation", Vector) = (0,0,0,0)
		_RotationSpeed("Rotation Speed", Vector) = (0,0,0,0)
		[Gamma]_Boundary("Boundary", Range( 0 , 1)) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Background"  "Queue" = "Background+0" "IsEmissive" = "true"  "PreviewType"="Skybox" }
		Cull Back
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float3 worldPos;
			float3 viewDir;
		};

		uniform samplerCUBE _Tex;
		uniform samplerCUBE _GroundTex;
		uniform float3 _RotationSpeed;
		uniform float3 _Rotation;
		uniform float _ViewHeight;
		uniform float _PlanetRadius;
		uniform float _Boundary;


		float3 RotateAroundAxis( float3 center, float3 original, float3 u, float angle )
		{
			original -= center;
			float C = cos( angle );
			float S = sin( angle );
			float t = 1 - C;
			float m00 = t * u.x * u.x + C;
			float m01 = t * u.x * u.y - S * u.z;
			float m02 = t * u.x * u.z + S * u.y;
			float m10 = t * u.x * u.y + S * u.z;
			float m11 = t * u.y * u.y + C;
			float m12 = t * u.y * u.z - S * u.x;
			float m20 = t * u.x * u.z - S * u.y;
			float m21 = t * u.y * u.z + S * u.x;
			float m22 = t * u.z * u.z + C;
			float3x3 finalMatrix = float3x3( m00, m01, m02, m10, m11, m12, m20, m21, m22 );
			return mul( finalMatrix, original ) + center;
		}


		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 _Vector5 = float3(0,1,0);
			float3 viewToWorld65 = mul( UNITY_MATRIX_I_V, float4( float3( 0,0,0 ), 1 ) ).xyz;
			float temp_output_141_0 = ( _ViewHeight + viewToWorld65.y );
			float temp_output_66_0 = ( temp_output_141_0 + _PlanetRadius );
			float3 temp_output_168_0 = ( ( ( ( _RotationSpeed + _Rotation ) / 180.0 ) * UNITY_PI ) + ( ( viewToWorld65 * float3(1,0,1) ) / temp_output_66_0 ) );
			float dotResult126 = dot( temp_output_168_0 , _Vector5 );
			float3 _Vector4 = float3(1,0,0);
			float dotResult124 = dot( temp_output_168_0 , _Vector4 );
			float3 _Vector3 = float3(0,0,1);
			float dotResult123 = dot( temp_output_168_0 , _Vector3 );
			float3 temp_output_68_0 = ( float3(0,1,0) * temp_output_66_0 );
			float3 normalizeResult135 = normalize( ase_vertex3Pos );
			float3 normalizeResult134 = normalize( ase_vertex3Pos );
			float dotResult70 = dot( temp_output_68_0 , normalizeResult134 );
			float temp_output_87_0 = ( pow( ( dotResult70 * -2.0 ) , 2.0 ) - ( 4.0 * 1.0 * ( pow( temp_output_66_0 , 2.0 ) - pow( _PlanetRadius , 2.0 ) ) ) );
			float temp_output_84_0 = ( 1.0 * 2.0 );
			float3 normalizeResult104 = normalize( ( temp_output_68_0 - ( normalizeResult135 * ( ( -( dotResult70 * -2.0 ) + temp_output_87_0 ) / temp_output_84_0 ) ) ) );
			float3 rotatedValue111 = RotateAroundAxis( float3( 0,0,0 ), normalizeResult104, _Vector3, dotResult123 );
			float3 rotatedValue115 = RotateAroundAxis( float3( 0,0,0 ), rotatedValue111, _Vector4, dotResult124 );
			float3 rotatedValue116 = RotateAroundAxis( float3( 0,0,0 ), rotatedValue115, _Vector5, dotResult126 );
			float3 lerpResult107 = lerp( (texCUBE( _Tex, ase_vertex3Pos )).rgb , (texCUBE( _GroundTex, rotatedValue116 )).rgb , ( saturate( ( ( ( -( dotResult70 * -2.0 ) + temp_output_87_0 ) / temp_output_84_0 ) / ( pow( temp_output_66_0 , 2.0 ) * _Boundary ) ) ) * step( 0.0 , i.viewDir.y ) ));
			o.Emission = lerpResult107;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows 

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
				float3 worldPos : TEXCOORD1;
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
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.worldPos = worldPos;
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
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.viewDir = worldViewDir;
				surfIN.worldPos = worldPos;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
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
Version=18900
0;1122;2326;958;-634.4155;803.7301;1.695953;True;True
Node;AmplifyShaderEditor.RangedFloatNode;14;-640,512;Inherit;False;Property;_ViewHeight;View Height;4;0;Create;True;0;0;0;False;0;False;2;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TransformPositionNode;65;-946.1511,483.8603;Inherit;False;View;World;False;Fast;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;16;-656,784;Inherit;False;Property;_PlanetRadius;Planet Radius;3;0;Create;True;0;0;0;False;0;False;6321000;6321000;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;141;-375.1447,570.4609;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PosVertexDataNode;133;-515.5715,915.5588;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;66;-63.97327,618.4064;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;67;-128,464;Inherit;False;Constant;_Vector1;Vector 1;4;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;128,512;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;74;94.1907,945.7172;Inherit;False;Constant;_Float2;Float 2;4;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalizeNode;134;-102.5427,783.5236;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;73;290.1907,851.7172;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;70;288,576;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;75;296.1907,975.717;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;332.5522,745.9596;Inherit;False;Constant;_Float1;Float 1;4;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;77;640,416;Inherit;False;Constant;_Float3;Float 3;4;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;76;608.1907,831.7172;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;94;912.9434,397.5036;Inherit;False;1597.552;634;Quadratic Formula;16;78;80;79;84;89;82;83;88;81;90;91;87;86;85;93;92;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;621.5522,620.9596;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;80;978.9434,746.5441;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;79;962.9434,458.544;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;88;1235.496,916.5037;Inherit;False;Constant;_Float5;Float 5;4;0;Create;True;0;0;0;False;0;False;4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;78;972.4956,599.5037;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;83;1066.496,876.5037;Inherit;False;Constant;_Float4;Float 4;4;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;89;1442.496,850.5037;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;82;1441.943,734.5441;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;87;1653.496,772.5037;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;81;1432.943,467.544;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;84;1577.496,580.5037;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;90;1781.496,447.5036;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;162;3255.824,-217.7964;Inherit;False;Property;_RotationSpeed;Rotation Speed;6;0;Create;True;0;0;0;False;0;False;0,0,0;0,0.0041781,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.Vector3Node;113;3245.722,-57.38862;Inherit;False;Property;_Rotation;Rotation;5;0;Create;True;0;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RelayNode;166;2566.424,-116.7355;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;171;2583.065,118.8694;Inherit;False;Constant;_Vector0;Vector 0;8;0;Create;True;0;0;0;False;0;False;1,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;163;3527.804,-123.5459;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;132;2571.374,817.3137;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;85;2064.943,465.544;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;120;3456,144;Inherit;False;Constant;_Float6;Float 6;5;0;Create;True;0;0;0;False;0;False;180;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;92;2322.495,462.5036;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;101;940.5685,253.0385;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;135;2944,816;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;121;3655.869,-41.27028;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PiNode;122;3485.149,236.6632;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;170;2851.027,37.46362;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RelayNode;169;2841.528,200.1225;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;102;2387.103,262.1282;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;172;3064.717,132.437;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;3232,736;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;119;3705.855,94.43002;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;168;3908.885,171.1723;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;114;4059.673,175.3108;Inherit;False;Constant;_Vector3;Vector 3;4;0;Create;True;0;0;0;False;0;False;0,0,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleSubtractOpNode;103;3427.733,400.2705;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;104;3602.882,422.6489;Inherit;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;117;4133.424,-142.6735;Inherit;False;Constant;_Vector4;Vector 4;4;0;Create;True;0;0;0;False;0;False;1,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;165;4441.848,1578.307;Inherit;False;Constant;_Float9;Float 9;7;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;123;4348.755,349.3412;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;111;4616.449,-137.3107;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;160;4305.804,1371.226;Inherit;False;Property;_Boundary;Boundary;7;1;[Gamma];Create;True;0;0;0;False;0;False;0;0.05;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;118;4604.209,297.9892;Inherit;False;Constant;_Vector5;Vector 5;4;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;124;4348.577,-168.5844;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;164;4703.848,1320.307;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;159;4904.954,1312.08;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;126;4890.755,225.3412;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;152;4806.161,1220.95;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RotateAboutAxisNode;115;4444.424,74.32654;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PosVertexDataNode;131;-649.4322,139.8556;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleDivideOpNode;161;5024.915,1196.731;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;130;4681.775,1454.333;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TexturePropertyNode;1;-568,-320.5;Inherit;True;Property;_Tex;Tex;1;0;Create;True;0;0;0;False;0;False;None;None;False;black;LockedToCube;Cube;-1;0;2;SAMPLERCUBE;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RotateAboutAxisNode;116;4909.507,13.29839;Inherit;False;False;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TexturePropertyNode;7;4544,496;Inherit;True;Property;_GroundTex;Ground Tex;2;0;Create;True;0;0;0;False;0;False;None;None;False;black;LockedToCube;Cube;-1;0;2;SAMPLERCUBE;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SaturateNode;158;5260.907,1275.815;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;8;4992,512;Inherit;True;Property;_TextureSample1;Texture Sample 1;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StepOpNode;153;5190.684,1389.01;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-112,-320;Inherit;True;Property;_TextureSample0;Texture Sample 0;1;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ComponentMaskNode;106;5296,288;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;154;5503.001,1279.003;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;105;5407.537,-293.7915;Inherit;False;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;150;5360.383,898.0924;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StepOpNode;151;5181.317,1155.783;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;108;5405.155,487.5305;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;144;2646.496,1160.229;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;137;3542.997,827.9922;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;147;914.9595,1286.208;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NegateNode;129;-337.7281,-119.5592;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;167;2746.129,-119.4218;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;5876.914,1351.385;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;138;3307.499,868.4529;Inherit;False;Constant;_Float0;Float 0;6;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;91;1839.496,691.5037;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;155;5527.757,540.4259;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;143;2642.774,1264.463;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;110;4640,1024;Inherit;False;Constant;_Vector2;Vector 2;4;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RelayNode;146;921.0877,1143.478;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;140;3570.864,1081.87;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;149;5644.492,1007.951;Inherit;False;Constant;_Float7;Float 7;6;0;Create;True;0;0;0;False;0;False;1E-13;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;139;4144.905,788.7938;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;142;3572.662,961.1011;Inherit;False;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;145;3826.548,888.4829;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;107;5927.719,-55.9964;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;86;2068.943,599.5441;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RelayNode;93;2320.646,614.4156;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;4;-610,-81.5;Inherit;False;World;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;6283.13,192.3561;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;Skybox/Horizon Cubemap;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;False;Background;;Background;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;1;PreviewType=Skybox;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;141;0;14;0
WireConnection;141;1;65;2
WireConnection;66;0;141;0
WireConnection;66;1;16;0
WireConnection;68;0;67;0
WireConnection;68;1;66;0
WireConnection;134;0;133;0
WireConnection;73;0;66;0
WireConnection;73;1;74;0
WireConnection;70;0;68;0
WireConnection;70;1;134;0
WireConnection;75;0;16;0
WireConnection;75;1;74;0
WireConnection;76;0;73;0
WireConnection;76;1;75;0
WireConnection;71;0;70;0
WireConnection;71;1;72;0
WireConnection;80;0;76;0
WireConnection;79;0;77;0
WireConnection;78;0;71;0
WireConnection;89;0;88;0
WireConnection;89;1;79;0
WireConnection;89;2;80;0
WireConnection;82;0;78;0
WireConnection;82;1;83;0
WireConnection;87;0;82;0
WireConnection;87;1;89;0
WireConnection;81;0;78;0
WireConnection;84;0;79;0
WireConnection;84;1;83;0
WireConnection;90;0;81;0
WireConnection;90;1;87;0
WireConnection;166;0;65;0
WireConnection;163;0;162;0
WireConnection;163;1;113;0
WireConnection;85;0;90;0
WireConnection;85;1;84;0
WireConnection;92;0;85;0
WireConnection;101;0;68;0
WireConnection;135;0;132;0
WireConnection;121;0;163;0
WireConnection;121;1;120;0
WireConnection;170;0;166;0
WireConnection;170;1;171;0
WireConnection;169;0;66;0
WireConnection;102;0;101;0
WireConnection;172;0;170;0
WireConnection;172;1;169;0
WireConnection;100;0;135;0
WireConnection;100;1;92;0
WireConnection;119;0;121;0
WireConnection;119;1;122;0
WireConnection;168;0;119;0
WireConnection;168;1;172;0
WireConnection;103;0;102;0
WireConnection;103;1;100;0
WireConnection;104;0;103;0
WireConnection;123;0;168;0
WireConnection;123;1;114;0
WireConnection;111;0;114;0
WireConnection;111;1;123;0
WireConnection;111;3;104;0
WireConnection;124;0;168;0
WireConnection;124;1;117;0
WireConnection;164;0;66;0
WireConnection;164;1;165;0
WireConnection;159;0;164;0
WireConnection;159;1;160;0
WireConnection;126;0;168;0
WireConnection;126;1;118;0
WireConnection;152;0;92;0
WireConnection;115;0;117;0
WireConnection;115;1;124;0
WireConnection;115;3;111;0
WireConnection;161;0;152;0
WireConnection;161;1;159;0
WireConnection;116;0;118;0
WireConnection;116;1;126;0
WireConnection;116;3;115;0
WireConnection;158;0;161;0
WireConnection;8;0;7;0
WireConnection;8;1;116;0
WireConnection;8;7;7;1
WireConnection;153;1;130;2
WireConnection;2;0;1;0
WireConnection;2;1;131;0
WireConnection;2;7;1;1
WireConnection;106;0;8;0
WireConnection;154;0;158;0
WireConnection;154;1;153;0
WireConnection;105;0;2;0
WireConnection;150;0;104;0
WireConnection;150;1;151;0
WireConnection;151;1;152;0
WireConnection;108;1;110;0
WireConnection;144;0;146;0
WireConnection;137;1;138;0
WireConnection;147;0;141;0
WireConnection;129;0;4;0
WireConnection;167;0;166;0
WireConnection;148;0;151;0
WireConnection;148;1;149;0
WireConnection;91;0;81;0
WireConnection;91;1;87;0
WireConnection;155;0;108;0
WireConnection;143;0;147;0
WireConnection;146;0;16;0
WireConnection;140;0;143;0
WireConnection;140;1;138;0
WireConnection;139;0;145;0
WireConnection;139;1;137;0
WireConnection;142;0;138;0
WireConnection;142;1;144;0
WireConnection;142;2;143;0
WireConnection;145;0;142;0
WireConnection;145;1;140;0
WireConnection;107;0;105;0
WireConnection;107;1;106;0
WireConnection;107;2;154;0
WireConnection;86;0;91;0
WireConnection;86;1;84;0
WireConnection;93;0;86;0
WireConnection;0;2;107;0
ASEEND*/
//CHKSM=9BBCBB5E1B2E3F0E715524FAE6C234E1FEA76EF4