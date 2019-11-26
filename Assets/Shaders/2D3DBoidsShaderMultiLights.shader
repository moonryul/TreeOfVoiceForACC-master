Shader "Custom/2D3DBoidsShaderMultiLights" {
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
			
            Tags {"LightMode" = "ForwardBase" }
//"LightMode" = ForwardBase: Used in Forward rendering
//, ambient, main directional light, vertex / SH lights and lightmaps
// are applied.
//ForwardAdd: Used in Forward rendering; additive per - pixel lights are applied, 
//one pass per light. (see below)

//"PassFlags" = OnlyDirectional: When used in ForwardBase pass type, this flag makes it so that only the main directional light
//			and ambient / lightprobe data is passed into the shader.

			Blend SrcAlpha OneMinusSrcAlpha // 

            CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
//#pragma exclude_renderers gles

            #pragma vertex vert
            #pragma fragment frag
            
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

            struct Boid 
            {
		  
			float4x4 boidFrame;
			float3  position; // the position of the  boid in the boid reference frame 	    

		     float3  scale;
		     float3  headDir; // the head dir of the  boid on the local plane: 
		     float speed;            // the speed of a boid

		     float  radius; // the radius of a circle boid
			 float3 colorHSL;
		     float4 color;         // RGBA color
		     float2 soundGrain; //        the freq (pitch) and amp of the boid sound grain
		     float duration; //   the duration of the boid for one frame. 
		     int   wallNo;    // tfloat2  position; // the position of a boid
			 		
            };
            
			

            sampler2D _MainTex;
            //float2 _Scale; // scale factor on x-z plane
            //fixed4 _Color;

	        float3  GroundMinCorner;
			float3  GroundMaxCorner;

			float3  CeilingMinCorner;
			float3  CeilingMaxCorner;

			float3 _Scale;

			int   _BloidsNum;
			float _CohesionRadius;


			//float   Use3DBoids;

        #if SHADER_TARGET >= 45
            //RWStructuredBuffer<Boid> _BoidBuffer;	 // RW buffer does not work
		    StructuredBuffer<Boid> _BoidBuffer;	

			//RWStructuredBuffer<float4x4> _MatrixBuffer : register(u1);
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
					* float3x3(-cross(c1, c2), // first row
						-cross(c2, c0), // second row
						-cross(c0, c1)); // third row

				return invA;

			}

			//float3x3 inverse3x3(float3x3 A)
			//{
			//	float3 r0 = A[0]; // the first row
			//	float3 r1 = A[1];
			//	float3 r2 = A[2];

			//	float3 c0 = float3(r0[0], r1[0], r2[0]); // first column
			//	float3 c1 = float3(r0[1], r1[1], r2[1]);
			//	float3 c2 = float3(r0[2], r1[2], r2[2]);

			//	float detA = dot(c0, cross(c1, c2));

			//	float3x3 invA = (1 / detA)
			//		* float3x3(cross(c1, c2), // first row
			//			cross(c2, c0), // second row
			//			cross(c0, c1)); // third row

			//	return invA;

			//}


          
			//The right-handed coordinate system use right hand rule to determine the direction of 
			//the cross product, while the left-handed coordinate system use left hand rule,
			//and hence the result is the same. This means cross product can not be used to determine the handedness. 
			//Suppose x = (1,0,0), and y = (0, 1, 0). I think we can all agree that x cross y = z = (0,0,1). Now, is this right or left handed coordinates?
			//It actually doesn't matter. Cross product still works the same

			// shaders with unity3D: http://xboxoneindiedevelopment.blogspot.kr/2015/02/coming-from-shaders-in-xna-to-shaders.html

			// backface culling:
			/*By default, backface culling is on, which means everything has one side
			(the back face is culled).You don't want to use Cull Off,
				except for those cases where you do want to render both sides.*/
			//Biface: I totally agree with this post: if you need to have a biface mesh the best way is to duplicate it and flip normals.
			//This is the right way for so many reasons..

            v2f vert (appdata_full v, uint instanceID : SV_InstanceID )
            {
				// save shader matrices to the structured buffer
				/*_MatrixBuffer[0] = UNITY_MATRIX_M;
		        _MatrixBuffer[1] = UNITY_MATRIX_V;
		        _MatrixBuffer[2] = UNITY_MATRIX_P;*/

				////////////////////////////////
           // #if SHADER_TARGET >= 45
                Boid b = _BoidBuffer[instanceID];
                

				//float3 scale = b.scale;
				float3 scale = _Scale * b.scale; // component wise multiplication
				float scale0 = _Scale[0]* b.scale[0]; // component wise multiplication

				float4x4 scaleMat = float4x4(
					scale[0],    0,        0,           0,
					0,           scale[1],  0,          0,
					0,              0,       scale[2],  0,
					0,             0,            0,        1
					);


				//float4x4 object2world = mul(b.boidFrame, scaleMat);
				//float4x4 object2world = scale0 * b.boidFrame;

				float3x3 R = (float3x3) b.boidFrame; // this is TR, without S (scale)

				float3x3 RS = scale0 * R;
				float4 T = b.boidFrame._m03_m13_m23_m33;

				float4x4 object2world = float4x4(
					float4(RS._m00_m01_m02, T[0]),
					float4(RS._m10_m11_m12, T[1]),
					float4(RS._m20_m21_m22, T[2]),
					float4(0, 0, 0, T[3]));


				// transform position to the clip space
                v2f o;
				
				// Note float4 pos : SV_POSITION; // the clip space position
				// mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
                //o.pos = UnityObjectToClipPos( mul( object2world, v.vertex)  ); // v: the current vertex in the current mesh instance
				o.posWorld = mul(object2world, v.vertex);
				o.pos = mul( UNITY_MATRIX_VP, o.posWorld);

				// change the way the normals are computed
               
				//o.normal = normalize( mul( object2world, v.normal));

				// get the 3x3 part of 4x4 transform object2world
				float3x3 L = float3x3( object2world[0].xyz,
					                   object2world[1].xyz,
					                   object2world[2].xyz );



				//float3x3 R = (float3x3) b.boidFrame; // this is TR, without S (scale)
				float3x3 Rinv = transpose(R);

				// transform position to the clip space
			   // v2f o;

				// Note float4 pos : SV_POSITION; // the clip space position
				// mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
				//o.pos = UnityObjectToClipPos( mul( object2world, v.vertex)  ); // v: the current vertex in the current mesh instance
			   // o.posWorld = mul(object2world, v.vertex);
				//o.pos = mul(UNITY_MATRIX_VP, o.posWorld);

				//NormalMatrix: http://www.lighthouse3d.com/tutorials/glsl-12-tutorial/the-normal-matrix/
				// get the 3x3 part of 4x4 transform object2world
			   /* float3x3 L = float3x3(object2world[0].xyz,
					object2world[1].xyz,
					object2world[2].xyz);*/

				//o.normal = normalize( mul( R, v.normal) ) ;
				o.normal = normalize(mul(v.normal, (1 / scale0) * Rinv));



				//o.normal = normalize(mul(v.normal, inverse3x3(L)));

				// // just pass the texture coordinate
                o.uv = v.texcoord;

				// This is a per-vertex lighting
    //            half nl = max(0, dot( o.normal, _WorldSpaceLightPos0.xyz ));
    //            o.diff = nl * _LightColor0.rgb; // this is the diffuse color of the surface point
    //            o.ambient = ShadeSH9( half4(o.normal.xyz, 1) );
				//
				o.color = b.color; // set the color of the boid mesh instance to each vertex of the mesh:
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

				//return float4(i.color.rgb, 1);

				//int unity_InstanceID;
				//UNITY_SETUP_INSTANCE_ID(i); //  necessary only if any instanced properties are going to be accessed in the fragment Shader


				float3 normalDirection = i.normal;

				float3 viewDirection = normalize(
					_WorldSpaceCameraPos - i.posWorld.xyz);

				float3 lightDirection;
				float attenuation;

				//Also if "LightMode"="ForwardBase" is true then the _WorldSpaceLightPos0 is always for a directional light, even if none exists in the scene.
				//Only "LightMode"="ForwardAdd" has support for passing the position of point lights. 
				if (0.0 == _WorldSpaceLightPos0.w) // directional light?
				{
					attenuation = 1.0; // no attenuation
					lightDirection = -_WorldSpaceLightPos0.xyz; // light direction is the
					// direction TO the light
					//lightDirection =
					//	normalize(_WorldSpaceLightPos0.xyz);
				}
				else // point or spot light
				{
					float3 surfacePointToLightSource =
						_WorldSpaceLightPos0.xyz - i.posWorld.xyz;
					float distance = length(surfacePointToLightSource);
					attenuation = 1.0 / distance; // linear attenuation 

					lightDirection = normalize(surfacePointToLightSource);
				}

				float3 ambientLighting =
					UNITY_LIGHTMODEL_AMBIENT.rgb * i.color.rgb;

				ambientLighting = float3(0, 0, 0);
				//return float4(ambientLighting, 1); // for debugging
				


				//// for debugging for the directinal light
				float lightToNormal = dot(normalDirection, lightDirection);
				if (lightToNormal < 0) {
					//return float4(1, 0, 0, 1);
					return float4(0, 0, 0, 1); // for debugging

				}
				else return //float4(0, 1, 0, 1); // for debugging
					float4(0, 0, 0, 1);

				float3 diffuseReflection =
					attenuation * _LightColor0.rgb * i.color.rgb
					            * max(0.0, dot(normalDirection, lightDirection));

				//return float4(diffuseReflection, 1); // for debugging
				//float3 diffuseReflection =
				//	attenuation * _LightColor0.rgb * i.color.rgb
				//		* abs ( dot(normalDirection, lightDirection) );

				float3 specularReflection;

				if (dot(normalDirection, lightDirection) < 0.0)
					// light source on the wrong side?
				{
					
					specularReflection = float3(0.0, 0.0, 0.0); // black: Wrong side

					//specularReflection = attenuation * _LightColor0.rgb
					//	* _SpecColor.rgb *
					//	pow(max(0.0,
					//		dot(reflect(-lightDirection, normalDirection),
					//			viewDirection)),
					//		_Shininess); // _Shininess =10


					// no specular reflection
				}
				else // light source on the right side
				{
					specularReflection = attenuation * _LightColor0.rgb
						* _SpecColor.rgb * 
						pow( max(0.0, 
							     dot( reflect( -lightDirection, normalDirection),
							          viewDirection) ),
							_Shininess); // _Shininess =10

					// for debug
					//if (dot(reflect(-lightDirection, normalDirection),
					//	viewDirection) < 0)
					//{
					//	specularReflection = float3(0, 1, 0); // green: right side

					//}
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
        } // PASS forwardBase pass for the directional light
//
		
		Pass{
			// pass for additional light sources ( point light in our case)
		  Tags {"LightMode" = "ForwardAdd" }
		  ///ForwardAdd: Used in Forward rendering; additive per - pixel lights are
			//applied, one pass per light. 

		  //Blend SrcAlpha OneMinusSrcAlpha
		 Blend One One //// additive blending 

		  CGPROGRAM
			// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
			//#pragma exclude_renderers gles

						#pragma vertex vert
						#pragma fragment frag

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
			 #pragma multi_compile_fwdadd nolightmap nodirlightmap nodynlightmap novertexlight
			 #pragma target 4.5

		 // shader side buffer struct:

			 #define M_PI 3.1415926535897932384626433832795

			 struct Boid
			 {

				float4x4 boidFrame;
				float3  position; // the position of the  boid in the boid reference frame 	    

				 float3  scale;
				 float3  headDir; // the head dir of the  boid on the local plane: 
				 float speed;            // the speed of a boid

				 float  radius; // the radius of a circle boid
				 float3 colorHSL;
				 float4 color;         // RGBA color
				 float2 soundGrain; //        the freq (pitch) and amp of the boid sound grain
				 float duration; //   the duration of the boid for one frame. 
				 int   wallNo;    // tfloat2  position; // the position of a boid

				};



				sampler2D _MainTex;
				//float2 _Scale; // scale factor on x-z plane
				//fixed4 _Color;

				float3  GroundMinCorner;
				float3  GroundMaxCorner;

				float3  CeilingMinCorner;
				float3  CeilingMaxCorner;

				float3 _Scale;

				int   _BloidsNum;
				float _CohesionRadius;


				//float   Use3DBoids;

			#if SHADER_TARGET >= 45
				//RWStructuredBuffer<Boid> _BoidBuffer;	 // RW buffer does not work
				StructuredBuffer<Boid> _BoidBuffer;
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
						* float3x3(-cross(c1, c2), // first row
							-cross(c2, c0), // second row
							-cross(c0, c1)); // third row

					return invA;

				}

				//float3x3 inverse3x3(float3x3 A)
				//{
				//	float3 r0 = A[0]; // the first row
				//	float3 r1 = A[1];
				//	float3 r2 = A[2];

				//	float3 c0 = float3(r0[0], r1[0], r2[0]); // first column
				//	float3 c1 = float3(r0[1], r1[1], r2[1]);
				//	float3 c2 = float3(r0[2], r1[2], r2[2]);

				//	float detA = dot(c0, cross(c1, c2));

				//	float3x3 invA = (1 / detA)
				//		* float3x3(cross(c1, c2), // first row
				//			cross(c2, c0), // second row
				//			cross(c0, c1)); // third row

				//	return invA;

				//}


						   //The right-handed coordinate system use right hand rule to determine the direction of 
						   //the cross product, while the left-handed coordinate system use left hand rule,
						   //and hence the result is the same. This means cross product can not be used to determine the handedness. 
						   //Suppose x = (1,0,0), and y = (0, 1, 0). I think we can all agree that x cross y = z = (0,0,1). Now, is this right or left handed coordinates?
						   //It actually doesn't matter. Cross product still works the same

						   // shaders with unity3D: http://xboxoneindiedevelopment.blogspot.kr/2015/02/coming-from-shaders-in-xna-to-shaders.html

				v2f vert(appdata_full v, uint instanceID : SV_InstanceID)
						   {

							   ////////////////////////////////
						  // #if SHADER_TARGET >= 45
							   Boid b = _BoidBuffer[instanceID];

							   //float3 scale = b.scale;
							   float scale0 = _Scale[0] * b.scale[0]; // component wise multiplication


							  /* float4x4 scaleMat = float4x4(
								   scale[0],    0,        0,           0,
								   0,           scale[1],  0,          0,
								   0,              0,       scale[2],  0,
								   0,             0,            0,        1
								   );
*/

							  // float4x4 object2world = mul(b.boidFrame, scaleMat);

							   float3x3 R = (float3x3) b.boidFrame; // this is TR, without S (scale)

							   float3x3 RS = scale0 * R;
							   float4 T = b.boidFrame._m03_m13_m23_m33;

							   float4x4 object2world = float4x4(
									   float4(RS._m00_m01_m02, T[0]),
									   float4(RS._m10_m11_m12, T[1]),
									   float4(RS._m20_m21_m22, T[2]),
									   float4(0, 0, 0,         T[3]));

							   // transform position to the clip space
							   v2f o;

							   // Note float4 pos : SV_POSITION; // the clip space position
							   // mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
							   //o.pos = UnityObjectToClipPos( mul( object2world, v.vertex)  ); // v: the current vertex in the current mesh instance
							   o.posWorld = mul(object2world, v.vertex);
							   o.pos = mul(UNITY_MATRIX_VP, o.posWorld);

							   //o.normal = normalize(mul(object2world, v.normal));

							   // get the 3x3 part of 4x4 transform object2world
							  /* float3x3 L = float3x3(object2world[0].xyz,
								                     object2world[1].xyz,
								                     object2world[2].xyz);*/



							   //float3x3 R = (float3x3) b.boidFrame; // this is TR, without S (scale)
							   float3x3 Rinv = transpose(R);

							   // transform position to the clip space
							  // v2f o;

							   // Note float4 pos : SV_POSITION; // the clip space position
							   // mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
							   //o.pos = UnityObjectToClipPos( mul( object2world, v.vertex)  ); // v: the current vertex in the current mesh instance
							  // o.posWorld = mul(object2world, v.vertex);
							   //o.pos = mul(UNITY_MATRIX_VP, o.posWorld);

							   //NormalMatrix: http://www.lighthouse3d.com/tutorials/glsl-12-tutorial/the-normal-matrix/
							   // get the 3x3 part of 4x4 transform object2world
							  /* float3x3 L = float3x3(object2world[0].xyz,
								   object2world[1].xyz,
								   object2world[2].xyz);*/

							   //o.normal = normalize( mul( R, v.normal )) ; // for debugging
							   o.normal = normalize( mul(v.normal, (1 / scale0) * Rinv) );


							   //o.normal = normalize(mul(v.normal, inverse3x3(L)));

							   // // just pass the texture coordinate
							   o.uv = v.texcoord;

							   // This is a per-vertex lighting
				   //            half nl = max(0, dot( o.normal, _WorldSpaceLightPos0.xyz ));
				   //            o.diff = nl * _LightColor0.rgb; // this is the diffuse color of the surface point
				   //            o.ambient = ShadeSH9( half4(o.normal.xyz, 1) );
							   //
							   o.color = b.color; // set the color of the boid mesh instance to each vertex of the mesh:
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
							   // Ignore forward add pass
							   //return float4(0,0,0,1);

							   //int unity_InstanceID;
							   //UNITY_SETUP_INSTANCE_ID(i); //  necessary only if any instanced properties are going to be accessed in the fragment Shader

							   float3 normalDirection = i.normal;

							   float3 viewDirection = normalize(
								   _WorldSpaceCameraPos - i.posWorld.xyz);

							   float3 lightDirection;
							   float attenuation;

							   //https://docs.unity3d.com/Manual/SL-UnityShaderVariables.html?_ga=2.51048685.591554826.1574537830-1174358732.1569135042

							   //Directional lights: (world space direction, 0).
							   //Other lights: (world space position, 1).
							   if (0.0 == _WorldSpaceLightPos0.w) // directional light?
							   {
								   attenuation = 1.0; // no attenuation
								   lightDirection =
									   normalize(- _WorldSpaceLightPos0.xyz); // lightDirection is the direction toward the light

							   }
							   else // point or spot light
							   {
								   float3 surfacePointToLightSource =
									   _WorldSpaceLightPos0.xyz - i.posWorld.xyz;
								   float distance = length(surfacePointToLightSource);
								   attenuation = 1.0 / distance; // linear attenuation 

								   lightDirection = normalize(surfacePointToLightSource);
							   }

							  /* float3 ambientLighting =
							   	UNITY_LIGHTMODEL_AMBIENT.rgb * i.color.rgb;*/
							  // float3 ambientLighting = float3(0, 0, 0);

							   float3 ambientLighting = float3(0, 0, 0);

							   //// for debugging for the point light
							   float lightToNormal = dot(normalDirection, lightDirection);
							   if (lightToNormal < 0) {
								   return float4(1, 0, 0, 1);

							   }
							   else return float4(0, 1, 0, 1); // for debugging

							   float3 diffuseReflection =
								   attenuation * _LightColor0.rgb * i.color.rgb
											   * max(0.0, dot(normalDirection, lightDirection));

								/*float3 diffuseReflection =
									attenuation * _LightColor0.rgb * i.color.rgb
												* abs( dot(normalDirection, lightDirection) );*/

							   float3 specularReflection;
							   if (dot(normalDirection, lightDirection) < 0.0)
								   // light source on the wrong side?
							   {
								   specularReflection = float3(0.0, 0.0, 0.0);
								  // specularReflection = float3(0.0, 0.0, 1.0); // blue

								   //specularReflection = attenuation * _LightColor0.rgb
									  // * _SpecColor.rgb *
									  // pow(max(0.0,
										 //  dot(reflect(-lightDirection, normalDirection),
											//   viewDirection)),
										 //  _Shininess); // _Shininess =10

								   // no specular reflection
							   }
							   else // light source on the right side
							   {
								   specularReflection = attenuation * _LightColor0.rgb
									   * _SpecColor.rgb *
									   pow(max(0.0,
												dot(reflect(-lightDirection, normalDirection),
													 viewDirection)),
										   _Shininess); // _Shininess =10

								   // for debug
								   //if (dot(reflect(-lightDirection, normalDirection),
									  // viewDirection) < 0)
								   //{
									  // specularReflection = float3(0, 1, 0); // green

								   //}
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

							   return float4(ambientLighting + diffuseReflection + specularReflection, 1.0);

						   } // frag()

						   ENDCG
		} // Pass forwardAdd 
		  
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
  
   } // Subshader

} // Shader
