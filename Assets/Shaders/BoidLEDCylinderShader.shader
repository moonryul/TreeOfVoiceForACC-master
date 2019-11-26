Shader "Custom/BoidLEDCylinderShader" {
	//The Properties block contains shader variables (textures, colors etc.) 
	//that will be saved as part of the Material, and displayed in the material inspector.
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
	    _SpecColor("Specular Material Color", Color) = (1,1,1,1)
		_Shininess("Shininess", Float) = 10
    }

	//https://answers.unity.com/questions/38924/unity-is-a-left-handed-coordinate-system-why.html
	//https://en.wikibooks.org/wiki/Cg_Programming/Unity/Minimal_Shader
	// Cg in Unity: https://en.wikibooks.org/wiki/Cg_Programming/Unity
	// CG versus GLSL:
	//A properly written Cg application can be written once and then work with either OpenGL or Direct3D.
	//http://developer.download.nvidia.com/CgTutorial/cg_tutorial_chapter01.html
	// http://forums.cgsociety.org/archive/index.php?t-310564.html
			//https://forum.unity.com/threads/cg-order-of-vectors-in-matrix.120583/
			//http://steve.hollasch.net/cgindex/math/matrix/column-vec.html
			// good: http://duckmaestro.com/2013/08/17/matrices-are-not-transforms/
	//float3x3 m = float3x3(
	//1.1, 1.2, 1.3, // first row (not column as in GLSL = IRIS GL)
			//Shader language (row-major for CG, col-major for GLSL).
			//https://stackoverflow.com/questions/17717600/confusion-between-c-and-opengl-matrix-order-row-major-vs-column-major/17718692
			//https://en.wikibooks.org/wiki/Cg_Programming/Applying_Matrix_Transformations
			//https://gamedev.stackexchange.com/questions/102748/what-is-the-convention-for-column-major-order-matrix-transformations
			//http://antongerdelan.net/teaching/3dprog1/maths_cheat_sheet.pdf
			//https://www.khronos.org/bugzilla/show_bug.cgi?id=49
	//	2.1, 2.2, 2.3, // second row
	//	3.1, 3.2, 3.3  // third row
	//	);

    SubShader {
		
        Pass {
			
            Tags {"LightMode" = "ForwardBase"}
            
			Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
//#pragma exclude_renderers gles

            #pragma vertex vert
            #pragma fragment frag
            
//https://docs.unity3d.com/kr/530/Manual/SL-UnityShaderVariables.html
// shader variables: https://www.sysnet.pe.kr/2/0/11608
// Understanding the view matrix (camera matrix): https://www.3dgep.com/understanding-the-view-matrix/
// Model matrix. In scripts: Transform.localToWorldMatrix. In vertex shaders: _Object2World.
//View matrix.In scripts : Camera.worldToCameraMatrix.In vertex shaders : UNITY_MATRIX_V.
//Projection matrix.In scripts : Camera.projectionMatrix.In vertex shaders : UNITY_MATRIX_P.


// The basis vectors of the view transform:
//vector rorX = vector(UNITY_MATRIX_V._m00_m01_m02, 0);
//vector rorY = vector(UNITY_MATRIX_V._m10_m11_m12, 0);
//vector rorZ = vector(UNITY_MATRIX_V._m20_m21_m22, 0);

//UNITY_MATRIX_V[3].xyz == UNITY_MATRIX_V._m30_m31_m32, not UNITY_MATRIX_V._m03_m13_m23
//
//Though those still won't match quite right as the z axis of UNITY_MATRIX_V is inverted compared to _WorldSpaceCameraPos.z and Unity's world space coordinates in general.That also means that _WorldSpaceCameraPos and even float3(UNITY_MATRIX_V._m03_m13, -UNITY_MATRIX_V._m23) will only match if there's no rotation.
//
//The inverse view matrix's third column should match _WorldSpaceCameraPos.xyz though:
//UNITY_MATRIX_I_V._m03_m13_m23 == _WorldSpaceCameraPos.xyz

//C:\Program Files\Unity\Editor\Data\CGIncludes
            #include "UnityCG.cginc"

            uniform float4 _LightColor0;
		// color of light source (from "Lighting.cginc")

		// User-specified properties
		    uniform float4 _Color;
		    uniform float4 _SpecColor;
		    uniform float _Shininess;

           // #include "Lighting.cginc"
           // #include "AutoLight.cginc"
            
		//https://forum.unity.com/threads/regarding-unity_matrix_mvp-and-unityobjecttoclippos.460940/
		// CGIncludes\UnityCG.cginc: Tranforms position from object to homogenous space
		//inline float4 UnityObjectToClipPos(in float3 pos)
	    // {
        // #if defined(UNITY_SINGLE_PASS_STEREO) || defined(UNITY_USE_CONCATENATED_MATRICES)
		// More efficient than computing (M*V)P matrix product
		// return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
        // #else
		// return mul(UNITY_MATRIX_MVP, float4(pos, 1.0));
         //#endif
	     // }
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma target 4.5

		// shader side buffer struct:

            #define M_PI 3.1415926535897932384626433832795

           


			struct BoidLEDData
			{
				float4x4 boidFrame; // boid affine frame: also includes position
				float3  position; //
				float3  headDir; // heading direction of the boid on the local plane
				float4  color;         // RGBA color
				float3  scale;
				int    wallNo;      // the number of the wall whose boids defined the light sources of the branch cylinder
										// 0=> the inner  circular wall. 
										// 1 => the outer circular wall;
				int    nearestBoidID;
				int  neighborCount;
			};


            sampler2D _MainTex;
            //float2 _Scale; // scale factor on x-z plane
            //fixed4 _Color;

	        float3  GroundMinCorner;
			float3  GroundMaxCorner;

			float3  CeilingMinCorner;
			float3  CeilingMaxCorner;

			float3 _Scale;

			float _CeilingHeight;

			int _BoidOrLED;

			int   _BloidsNum;
			float _CohesionRadius;


			//float   Use3DBoids;

        #if SHADER_TARGET >= 45
           // StructuredBuffer<Boid> _BoidBuffer;	
			StructuredBuffer<BoidLEDData> _BoidLEDBuffer;
			/*RWStructuredBuffer<float4x4> _MatrixBuffer : register(u2);
			RWStructuredBuffer<float4> _LEDBoidPosBuffer : register(u3);*/

			//The register(u1)represents which internal gpu registrar 
			//	to bind the data structure to.
			//	You need to specify the same in C#, and keep in mind 
			//	this is global on the GPU.

			//int numOfShaderMatrices = 3; // 

		    //_MatrixBuffer[0] = UNITY_MATRIX_M;
		    //_MatrixBuffer[1] = UNITY_MATRIX_V;
		    //_MatrixBuffer[2] = UNITY_MATRIX_P;
        #endif

            struct v2f
            {
                float4 pos : SV_POSITION; // the clip space position
				float4 posWorld: TEXCOORD1; // world space pos of each vertex
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
    //            SHADOW_COORDS(2) // create a struct member
				//UNITY_FOG_COORDS(1)
    //            fixed3 diff : COLOR0;
    //            fixed3 ambient : COLOR1;
				fixed4 color : COLOR2;
                
            };

			float3x3 inverse3x3(float3x3 A)
			{
				float3 r0 = A[0]; // the first row
				float3 r1 = A[1];
				float3 r2 = A[2];

				float3 c0 = A._m00_m10_m20;  // first column
				float3 c1 = A._m01_m11_m21; // the second column
				float3 c2 = A._m02_m12_m22; // the third;
				
				float detA = dot(c0, -cross(c1, c2)); // the cross product is reversed
				// to change from the right hand system of CG to the left hand system of 
				// UNITY; All the vectors and matrices in this shader are assumed to 
				// follow the Unity convention. 

				float3x3 invA = (1 / detA)
					            * float3x3( -cross(c1, c2), // first row
						                    -cross(c2, c0), // second row
						                    -cross(c0, c1)); // third row

				return invA;

			}

			// matrix swizle operator
			//float4x4 myMatrix;
			//float    myFloatScalar;
			//float4   myFloatVec4;

			//// Set myFloatScalar to myMatrix[3][2]
			//myFloatScalar = myMatrix._m32;

			//// Assign the main diagonal of myMatrix to myFloatVec4
			//myFloatVec4 = myMatrix._m00_m11_m22_m33;
			//row vector a[3] is equivalent to a._m30_m31_m32_m33

			//The right-handed coordinate system use right hand rule to determine the direction of 
			//the cross product, while the left-handed coordinate system use left hand rule,
			//and hence the result is the same. This means cross product can not be used to determine 
			//the handedness. 
			//Suppose x = (1,0,0), and y = (0, 1, 0). I think we can all agree that x cross y = z 
			//= (0,0,1). Now, is this right or left handed coordinates?
			//It actually doesn't matter. Cross product still works the same

			// shaders with unity3D: http://xboxoneindiedevelopment.blogspot.kr/2015/02/coming-from-shaders-in-xna-to-shaders.html

            v2f vert (appdata_full v, uint instanceID : SV_InstanceID )
            {

				////////////////////////////////
           // #if SHADER_TARGET >= 45
				
				BoidLEDData b = _BoidLEDBuffer[instanceID];


				//_MatrixBuffer[0] = UNITY_MATRIX_M;
				//_MatrixBuffer[1] = UNITY_MATRIX_V;
				//_MatrixBuffer[2] = UNITY_MATRIX_P;

				//// store the global position of v.vertex: each vertex of the boid
				//// will be refered to, and the value of the final one will be actually
				//// stored in the buffer, but for our debuging purose, it is OK.
				//_LEDBoidPosBuffer[instanceID] = mul( b.boidFrame, v.vertex);


				//float3 scale = _Scale * b.scale; // component wise multiplication
				float scale0 = _Scale[0] * b.scale[0]; // component wise multiplication

				// The boid's head is assumed to be aligned with the z axis. 
				// There is no roll motion about the z axis, only the pitch about the x axis
				// and the yaw about the y axis is allowed
				// rotX is the angle between y axis and the head direction b.headDir; this angle
				// is the same as the angle between the original y axis and the new y aixs
				// obtained when the pitch about the x axis is performed
				// 


				// R = Ry * Rx * Rz 
				//float4x4 object2world = transform(
				//	b.scale,
				//	float3(rotX, rotY, 0), // pitch => yaw => no roll
				//	boidLoc
				//);

				

				/*float4x4 scaleMat = float4x4(
					scale[0],    0,        0,           0,
					0,           scale[1],  0,          0,
					0,              0,       scale[2],  0,
					0,             0,            0,        1
					);
*/

				//float4x4 object2world = scale0 * b.boidFrame;

			
				float3x3 R = (float3x3) b.boidFrame; // this is TR, without S (scale)
				
				float3x3 RS = scale0 * R;
				float4 T = b.boidFrame._m03_m13_m23_m33;

				float4x4 object2world = float4x4(
					float4(RS._m00_m01_m02, T[0]),
					float4(RS._m10_m11_m12, T[1]),
					float4(RS._m20_m21_m22, T[2]),
					float4(0, 0, 0, T[3]));


				float3x3 Rinv = transpose(R);

				// transform position to the clip space
                v2f o;
				
				// Note float4 pos : SV_POSITION; // the clip space position
				// mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
                //o.pos = UnityObjectToClipPos( mul( object2world, v.vertex)  ); // v: the current vertex in the current mesh instance
				o.posWorld = mul(object2world, v.vertex);
				o.pos = mul( UNITY_MATRIX_VP, o.posWorld);

				//NormalMatrix: http://www.lighthouse3d.com/tutorials/glsl-12-tutorial/the-normal-matrix/
				// get the 3x3 part of 4x4 transform object2world
				/*float3x3 L = float3x3( object2world[0].xyz,
					                   object2world[1].xyz,
					                   object2world[2].xyz );*/

               // o.normal = normalize( mul( R, v.normal) ) ; // for debuging
			    o.normal = normalize( mul( v.normal, (1/scale0) * Rinv ) ) ;

				// // just pass the texture coordinate
                o.uv = v.texcoord;

				// This is a per-vertex lighting
    //            half nl = max(0, dot( o.normal, _WorldSpaceLightPos0.xyz ));
    //            o.diff = nl * _LightColor0.rgb; // this is the diffuse color of the surface point
    //            o.ambient = ShadeSH9( half4(o.normal.xyz, 1) );
				//
				
				o.color = b.color;


				//               // A fish mesh will have the same color on the whole of its surface.
				//               // This color is interpreted as the reflection cofficient in the shading process
				//               // in the fragment shader frag()
				//TRANSFER_SHADOW(o);
                return o; // the return o (of type v2f ) is given as the input to frag(v2f i) after being interpolated

            } // vert()

           // fixed4 frag (v2f i) : SV_TARGET
			//// pixel shader; returns low precision ("fixed4" type)
            // color ("SV_Target" semantic)
			//SV_TargetN: Multiple render targets
			//SV_Target1, SV_Target2, etc.: These are additional colors written by the shader.
			//This is used when rendering into more than one render target at once(known
			//as the Multiple Render Targets rendering technique, or MRT).SV_Target0 is the same as SV_Target.
			//SV_Depth: Pixel shader depth output
			//Usually the fragment shader does not override the Z buffer value, and a default value is used from the regular triangle rasterization.
			//However, for some effects it is useful to output custom Z buffer depth values per pixel.

			fixed4 frag(v2f i) : SV_Target
			{
				// use emissive color
				//return i.color;
				//int unity_InstanceID;
				//UNITY_SETUP_INSTANCE_ID(i); //  necessary only if any instanced properties are going to be accessed in the fragment Shader

				float3 normalDirection = i.normal;

				float3 viewDirection = normalize(
					_WorldSpaceCameraPos - i.posWorld.xyz);
				float3 lightDirection;
				float attenuation;

				// Forward Base pass is for the directional light

				//attenuation = 1.0; // no attenuation
				//lightDirection =
				//		normalize(_WorldSpaceLightPos0.xyz);

				if (0.0 == _WorldSpaceLightPos0.w) // directional light?
				{
					attenuation = 1.0; // no attenuation
					lightDirection =
						normalize(_WorldSpaceLightPos0.xyz);

					// for debugging 
					//return float4(1, 0, 0, 1);
				}
				else // point or spot light
				{
					//return float4(0, 0, 1, 1);
					float3 vertexToLightSource =
						_WorldSpaceLightPos0.xyz - i.posWorld.xyz;
					float distance = length(vertexToLightSource);
					attenuation = 1.0 / distance; // linear attenuation 
					lightDirection = normalize(vertexToLightSource);
				}

				/*float3 ambientLighting =
					UNITY_LIGHTMODEL_AMBIENT.rgb * i.color.rgb;*/

				float3 ambientLighting = float3(0, 0, 0);

				float3 diffuseReflection =
					attenuation * _LightColor0.rgb * i.color.rgb
					            * max(0.0, dot(normalDirection, lightDirection));

         		/*float3 diffuseReflection =
					attenuation * _LightColor0.rgb * i.color.rgb
					* abs( dot(normalDirection, lightDirection) );*/


				//// for debugging
				//float lightToNormal = dot(normalDirection, lightDirection);

				//if (lightToNormal < 0) {
				//	return float4(1, 0, 0, 1);

				//}

				//// for debugging
				float lightToNormal = dot(normalDirection, lightDirection);
				if (lightToNormal < 0) {
					return float4(1, 0, 0, 1);

				}

				else return float4(0, 1, 0, 1); // for debugging

				float3 specularReflection;
				if (dot(normalDirection, lightDirection) < 0.0)
					// light source on the wrong side?
				{
					specularReflection = float3(0.0, 0.0, 0.0);
					// no specular reflection
					/*specularReflection = attenuation * _LightColor0.rgb
						* _SpecColor.rgb * pow(max(0.0, dot(
							reflect(-lightDirection, normalDirection),
							viewDirection)), _Shininess);*/

				}
				else // light source on the right side
				{
					specularReflection = attenuation * _LightColor0.rgb
						* _SpecColor.rgb * pow(max(0.0, dot(
							reflect(-lightDirection, normalDirection),
							viewDirection)), _Shininess);
				}


                //fixed shadow = SHADOW_ATTENUATION(i);
				//
				//fixed4 col = tex2D(_MainTex, i.uv) * i.col; // get the interpolated color of the current pixel, which is interpolated from the vertex colors of the triangle
				//fixed4 col = i.col;

                //fixed3 lighting = i.diff * shadow + i.ambient;

                //col.rgb *= lighting; // col.rgb is intepreted as the reflection cofficient with respect to the lighting
               // apply the fog
			   // UNITY_APPLY_FOG(i.fogCoord, col); // we do not use FOG
               // return col; // the returned col is of type SV_Target

				return float4( ambientLighting +  diffuseReflection + specularReflection, 1.0);

            } // frag()

            ENDCG
        } // PASS forwardBase pass

  //    

		//Pass{

		//	Tags {"LightMode" = "ForwardAdd"}

		//	//Blend SrcAlpha OneMinusSrcAlpha
		//	Blend One One

		//	CGPROGRAM
		//	// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
		//	//#pragma exclude_renderers gles

		//				#pragma vertex vert
		//				#pragma fragment frag

		//	//C:\Program Files\Unity\Editor\Data\CGIncludes
		//				#include "UnityCG.cginc"

		//				uniform float4 _LightColor0;
		//// color of light source (from "Lighting.cginc")

		//// User-specified properties
		//	uniform float4 _Color;
		//	uniform float4 _SpecColor;
		//	uniform float _Shininess;

		//	// #include "Lighting.cginc"
		//	// #include "AutoLight.cginc"

		// //https://forum.unity.com/threads/regarding-unity_matrix_mvp-and-unityobjecttoclippos.460940/
		// // CGIncludes\UnityCG.cginc: Tranforms position from object to homogenous space
		// //inline float4 UnityObjectToClipPos(in float3 pos)
		// // {
		// // #if defined(UNITY_SINGLE_PASS_STEREO) || defined(UNITY_USE_CONCATENATED_MATRICES)
		// // More efficient than computing (M*V)P matrix product
		// // return mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
		// // #else
		// // return mul(UNITY_MATRIX_MVP, float4(pos, 1.0));
		//  //#endif
		//  // }
		//	 #pragma multi_compile_fwdadd nolightmap nodirlightmap nodynlightmap novertexlight
		//	 #pragma target 4.5

		// // shader side buffer struct:

		//	 #define M_PI 3.1415926535897932384626433832795

	 ////      

		//	 struct BoidLEDData
		//	 {
		//		 float4x4 boidFrame; // boid affine frame: also includes position
		//		 float3  position; //
		//		 float3  headDir; // heading direction of the boid on the local plane
		//		 float4  color;         // RGBA color
		//		 float3  scale;
		//		 int    wallNo;      // the number of the wall whose boids defined the light sources of the branch cylinder
		//								 // 0=> the inner  circular wall. 
		//								 // 1 => the outer circular wall;
		//		 int    nearestBoidID;
		//		 int  neighborCount;
		//	 };


		//	 sampler2D _MainTex;
		//	 //float2 _Scale; // scale factor on x-z plane
		//	 //fixed4 _Color;

		//	 //float3  GroundMinCorner;
		//	 //float3  GroundMaxCorner;

		//	 //float3  CeilingMinCorner;
		//	 //float3  CeilingMaxCorner;

		//	 float3 _Scale;

		//	// float _LEDCeilingHeight;

		//	 //int _BoidOrLED;

		//	 int   _BloidsNum;
		//	 //float _CohesionRadius;


		//	 //float   Use3DBoids;

		// #if SHADER_TARGET >= 45
		//	// StructuredBuffer<Boid> _BoidBuffer;	
		//	 StructuredBuffer<BoidLEDData> _BoidLEDBuffer;
		// #endif

		//	 struct v2f
		//	 {
		//		 float4 pos : SV_POSITION; // the clip space position
		//		 float4 posWorld: TEXCOORD1; // world space pos of each vertex
		//		 float3 normal : NORMAL;
		//		 float2 uv : TEXCOORD0;
		//		 //            SHADOW_COORDS(2) // create a struct member
		//					 //UNITY_FOG_COORDS(1)
		//		 //            fixed3 diff : COLOR0;
		//		 //            fixed3 ambient : COLOR1;
		//					 fixed4 color : COLOR2;

		//		};


		//	 float3x3 inverse3x3(float3x3 A)
		//	 {
		//		 float3 r0 = A[0]; // the first row
		//		 float3 r1 = A[1];
		//		 float3 r2 = A[2];

		//		 float3 c0 = A._m00_m10_m20;  // first column
		//		 float3 c1 = A._m01_m11_m21; // the second column
		//		 float3 c2 = A._m02_m12_m22; // the third;

		//		 float detA = dot(c0, -cross(c1, c2)); // the cross product is reversed
		//		 // to change from the right hand system of CG to the left hand system of 
		//		 // UNITY; All the vectors and matrices in this shader are assumed to 
		//		 // follow the Unity convention. 

		//		 float3x3 invA = (1 / detA)
		//			 * float3x3(-cross(c1, c2), // first row
		//				 -cross(c2, c0), // second row
		//				 -cross(c0, c1)); // third row

		//		 return invA;

		//	 }
		//	 //float3x3 inverse3x3(float3x3 A)
		//	 //{
		//		// //float3 r0 = A[0]; // the first row
		//		// //float3 r1 = A[1];
		//		// //float3 r2 = A[2];

		//		// //float3 c0 = float3(r0[0], r1[0], r2[0]); // first column
		//		// //float3 c1 = float3(r0[1], r1[1], r2[1]);
		//		// //float3 c2 = float3(r0[2], r1[2], r2[2]);

		//		// float3 r0 = A[0]; // the first row
		//		// float3 r1 = A[1]; // the second row
		//		// float3 r2 = A[2]; // the third row

		//		// float3 c0 = float3(A.00, A.10, A.02); // the first column
		//		// float3 c1 = float3(r0[1], r1[1], r2[1]);
		//		// float3 c2 = float3(r0[2], r1[2], r2[2]);


		//		// float detA = dot(c0, cross(c1, c2));
		//		// 
		//		// float3x3 invA = (1 / detA) 
		//		//	            * float3x3(cross(c1, c2), // first row
		//		//	                       cross(c2, c0), // second row
		//		//	                        cross(c0, c1)); // third row

		//		// return invA;

		//	 //}

		//	 //The right-handed coordinate system use right hand rule to determine the direction of 
		//	 //the cross product, while the left-handed coordinate system use left hand rule,
		//	 //and hence the result is the same. This means cross product can not be used to determine the handedness. 
		//	 //Suppose x = (1,0,0), and y = (0, 1, 0). I think we can all agree that x cross y = z = (0,0,1). Now, is this right or left handed coordinates?
		//	 //It actually doesn't matter. Cross product still works the same

		//	 // shaders with unity3D: http://xboxoneindiedevelopment.blogspot.kr/2015/02/coming-from-shaders-in-xna-to-shaders.html

		//	 v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
		//	 {

		//		 ////////////////////////////////
		//	// #if SHADER_TARGET >= 45


		//		 BoidLEDData b = _BoidLEDBuffer[instanceID];

		//		// float3 scale = _Scale * b.scale; // component wise multiplication
		//		 float scale0 = _Scale[0] * b.scale[0];

		//		 // The boid's head is assumed to be aligned with the z axis. 
		//		 // There is no roll motion about the z axis, only the pitch about the x axis
		//		 // and the yaw about the y axis is allowed
		//		 // rotX is the angle between y axis and the head direction b.headDir; this angle
		//		 // is the same as the angle between the original y axis and the new y aixs
		//		 // obtained when the pitch about the x axis is performed
		//		 // 


		//		 // R = Ry * Rx * Rz 
		//		 //float4x4 object2world = transform(
		//		 //	b.scale,
		//		 //	float3(rotX, rotY, 0), // pitch => yaw => no roll
		//		 //	boidLoc
		//		 //);



		//		 float4x4 scaleMat = float4x4(
		//			 scale[0],    0,        0,           0,
		//			 0,           scale[1],  0,          0,
		//			 0,              0,       scale[2],  0,
		//			 0,             0,            0,        1
		//			 );


		//		 //float4x4 object2world = mul(b.boidFrame, scaleMat);

		//		 //float4x4 object2world =  scale0 * b.boidFrame;


                  //float3x3 R = (float3x3) b.boidFrame; // this is TR, without S (scale)

                  //float3x3 RS = scale0 * R;
                  //float4 T = b.boidFrame._m03_m13_m23_m33;

                  //float4x4 object2world = float4x4(
	                 //                       float4(RS._m00_m01_m02, T[0]),
	                 //                       float4(RS._m10_m11_m12, T[1]),
	                 //                       float4(RS._m20_m21_m22, T[2]),
	                 //                       float4(0, 0, 0, T[3]));


                  //float3x3 Rinv = transpose(R);

		//		 // transform position to the clip space
		//		 v2f o;

		//		 // Note float4 pos : SV_POSITION; // the clip space position
		//		 // mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
		//		 //o.pos = UnityObjectToClipPos( mul( object2world, v.vertex)  ); // v: the current vertex in the current mesh instance
		//		 o.posWorld = mul(object2world, v.vertex);
		//		 o.pos = mul(UNITY_MATRIX_VP, o.posWorld);

		//		 //o.normal = normalize(mul(object2world, v.normal));
		//		 //o.normal = normalize(mul(v.normal, inverse(object2world) ));
		//		 // get the 3x3 part of 4x4 transform object2world
		//		 /*float3x3 L = float3x3(object2world[0].xyz,
		//			                   object2world[1].xyz,
		//			                   object2world[2].xyz);*/



		//		 float3x3 R = (float3x3) b.boidFrame; // this is TR, without S (scale)
		//		 float3x3 Rinv = transpose(R);

		//		 // transform position to the clip space
		//		// v2f o;

		//		 // Note float4 pos : SV_POSITION; // the clip space position
		//		 // mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
		//		 //o.pos = UnityObjectToClipPos( mul( object2world, v.vertex)  ); // v: the current vertex in the current mesh instance
		//		// o.posWorld = mul(object2world, v.vertex);
		//		 //o.pos = mul(UNITY_MATRIX_VP, o.posWorld);

		//		 //NormalMatrix: http://www.lighthouse3d.com/tutorials/glsl-12-tutorial/the-normal-matrix/
		//		 // get the 3x3 part of 4x4 transform object2world
		//		/* float3x3 L = float3x3(object2world[0].xyz,
		//			 object2world[1].xyz,
		//			 object2world[2].xyz);*/

		//		 // o.normal = normalize( mul( v.normal, inverse3x3(L) ) ) ;
		//		 o.normal = normalize(mul(v.normal, (1 / scale0) * Rinv));

		//		// o.normal = normalize(mul(v.normal, inverse3x3(L)));


		//		 // // just pass the texture coordinate
		//		 o.uv = v.texcoord;

		//		 // This is a per-vertex lighting
	 ////            half nl = max(0, dot( o.normal, _WorldSpaceLightPos0.xyz ));
	 ////            o.diff = nl * _LightColor0.rgb; // this is the diffuse color of the surface point
	 ////            o.ambient = ShadeSH9( half4(o.normal.xyz, 1) );
		//		 //

		//		 o.color = b.color;


		//		 //               // A fish mesh will have the same color on the whole of its surface.
		//		 //               // This color is interpreted as the reflection cofficient in the shading process
		//		 //               // in the fragment shader frag()
		//		 //TRANSFER_SHADOW(o);
		//		 return o; // the return o (of type v2f ) is given as the input to frag(v2f i) after being interpolated

		//	 } // vert()

		//	// fixed4 frag (v2f i) : SV_TARGET
		//	 //// pixel shader; returns low precision ("fixed4" type)
		//	 // color ("SV_Target" semantic)
		//	 //SV_TargetN: Multiple render targets
		//	 //SV_Target1, SV_Target2, etc.: These are additional colors written by the shader.
		//	 //This is used when rendering into more than one render target at once(known
		//	 //as the Multiple Render Targets rendering technique, or MRT).SV_Target0 is the same as SV_Target.
		//	 //SV_Depth: Pixel shader depth output
		//	 //Usually the fragment shader does not override the Z buffer value, and a default value is used from the regular triangle rasterization.
		//	 //However, for some effects it is useful to output custom Z buffer depth values per pixel.

		//	 fixed4 frag(v2f i) : SV_Target
		//	 {

		//		 // ignore forward add pass
		//		// return float4(0,0,0,1);

		//		 //int unity_InstanceID;
		//		 //UNITY_SETUP_INSTANCE_ID(i); //  necessary only if any instanced properties are going to be accessed in the fragment Shader

		//		 float3 normalDirection = i.normal;
		//		 float3 viewDirection = normalize(
		//			 _WorldSpaceCameraPos - i.posWorld.xyz);
		//		 float3 lightDirection;
		//		 float attenuation;

		//		 // The second light in forward Add pass could be a point or directional light

		//		 if (0.0 == _WorldSpaceLightPos0.w) // directional light?
		//		 {
		//			 attenuation = 1.0; // no attenuation
		//			 lightDirection =
		//				 normalize(_WorldSpaceLightPos0.xyz);
		//		 }
		//		 else // point or spot light
		//		 {
		//			// return float4(1, 0, 0, 1); // for debugging
		//			 float3 vertexToLightSource =
		//				 _WorldSpaceLightPos0.xyz - i.posWorld.xyz;

		//			 float distance = length(vertexToLightSource);
		//			 attenuation = 1.0 / distance; // linear attenuation 

		//			 lightDirection = normalize(vertexToLightSource);
		//		 }


		//		/* float3 ambientLighting =
		//			 UNITY_LIGHTMODEL_AMBIENT.rgb * i.color.rgb;*/


		//		 float3 ambientLighting = float3(0, 0, 0);

		//		 //// for debugging
		//	     float lightToNormal = dot(normalDirection, lightDirection);
		//		 if (lightToNormal < 0) {
		//	   	  return float4(0, 0, 1, 1);

		//	     }

		//		 float3 diffuseReflection =
		//			 attenuation * _LightColor0.rgb * i.color.rgb
		//						 * max(0.0, dot(normalDirection, lightDirection));

		//		/* float3 diffuseReflection =
		//			 attenuation * _LightColor0.rgb * i.color.rgb
		//			 * abs( dot(normalDirection, lightDirection));*/

		//		 float3 specularReflection;
		//		 if (dot(normalDirection, lightDirection) < 0.0)
		//			 // light source on the wrong side?
		//		 {
		//			 specularReflection = float3(0.0, 0.0, 0.0);
		//			 // no specular reflection
		//			/* specularReflection = attenuation * _LightColor0.rgb
		//				 * _SpecColor.rgb * pow(max(0.0, dot(
		//					 reflect(-lightDirection, normalDirection),
		//					 viewDirection)), _Shininess);*/

		//		 }
		//		 else // light source on the right side
		//		 {
		//			 specularReflection = attenuation * _LightColor0.rgb
		//				 * _SpecColor.rgb * pow(max(0.0, dot(
		//					 reflect(-lightDirection, normalDirection),
		//					 viewDirection)), _Shininess);
		//		 }


		//		 //fixed shadow = SHADOW_ATTENUATION(i);
		//		 //
		//		 //fixed4 col = tex2D(_MainTex, i.uv) * i.col; // get the interpolated color of the current pixel, which is interpolated from the vertex colors of the triangle
		//		 //fixed4 col = i.col;

		//		 //fixed3 lighting = i.diff * shadow + i.ambient;

		//		 //col.rgb *= lighting; // col.rgb is intepreted as the reflection cofficient with respect to the lighting
		//		// apply the fog
		//		// UNITY_APPLY_FOG(i.fogCoord, col); // we do not use FOG
		//		// return col; // the returned col is of type SV_Target

		//		 return float4(ambientLighting + diffuseReflection + specularReflection, 1.0);

		//	 } // frag()

		//	 ENDCG
		//} // PASS forwardAdd pass
		//



			UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
   
    } // Subshader
	
} // Shader
