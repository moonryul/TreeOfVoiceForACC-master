// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "Projector/Light" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_ShadowTex ("Cookie", 2D) = "" {}
		_FalloffTex ("FallOff", 2D) = "" {}
		_PyramidApex("Pyramid Apex", Vector) = (0.0, 0.0, 0.0, 1.0)
		_PyramidWidth("Pyramid  Width", Float) = 0.0
		_PyramidDepth("Pyramid  Depth", Float) = 0.0
	}
	
	Subshader {
		Tags {"Queue"="Transparent"}
		Pass {
			ZWrite Off
			ColorMask RGB
			//Blend DstColor One
			Blend SrcColor Zero
			Offset -1, -1
	
			CGPROGRAM
			#pragma enable_d3d11_debug_symbols
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			
			struct v2f {
			    //float4 vertex : SV_POSITION;
				float4 uvShadow : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;
				UNITY_FOG_COORDS(2)
				float4 pos : SV_POSITION;
			};
			
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			
			float4 _Color;
			sampler2D _ShadowTex;
			sampler2D _FalloffTex;

			float4 _PyramidApex;
			float 	_PyramidWidth;
			float 	_PyramidDepth;

			float3 _PyramidNormals[4]; // four normals to the pyramid sides; 0 = right side, 1 = back side, 2= left side, 3 = front side


			int findSideOfQInPyramid(float3 Q, float3 _PyramidApex, float _PyramidWidth, float _PyramidDepth)
			{
				int sideNo;
				// TO DO
				return sideNo;
			}

			float3 findPFromQ(float3 Q, float3 _PyramidApex, float3 _PyramidNormal)
			{
				float3 P = float3(0,0,0);
				// TO DO
				return P;
			}


			v2f vert (float4 vertex : POSITION)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(vertex);
				o.uvShadow = mul (unity_Projector, vertex);
				o.uvFalloff = mul (unity_ProjectorClip, vertex);
				UNITY_TRANSFER_FOG(o,o.pos);
				return o;
			}
			
			
			
			float4 frag (v2f i) : SV_Target
			{

				float4 Q = i.pos;

				int j = findSideOfQInPyramid(float3(Q.x, Q.y, Q.z), _PyramidApex,  _PyramidWidth, _PyramidDepth);

				float3 P = findPFromQ(float3(Q.x, Q.y, Q.z), _PyramidApex, _PyramidNormals[j]);

				if (P.x = 0.0f && P.y == 0.0f && P.z == 0.0f) // there is no P corresponding to Q
				{
					return 	float4(0.0f, 0.0f, 0.0f, 1.0f); // return the black color
				}
				else
				{
					i.uvShadow = mul(unity_Projector, float4(P, 1.0f));

					float4 texS = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
					texS.rgb *= _Color.rgb;
					texS.a = 1.0 - texS.a;

					float4 texF = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
					float4 res = texS * texF.a;

					UNITY_APPLY_FOG_COLOR(i.fogCoord, res, float4(0, 0, 0, 0));
					return res;
				}


			}	 //frag(v2f i)
			ENDCG
		}
	}
}
