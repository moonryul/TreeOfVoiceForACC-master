Shader "Custom/BoidLEDCylinders" {
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


		

			//public struct BoidLEDData
			//{

			//	public Vector3 Position; //
			//	public Vector3 HeadDir; // heading direction of the boid on the local plane
			//	public Vector4 Color;         // RGBA color
			//	public Vector3 Scale;
			//	public int WallNo;      // the number of the wall whose boids defined the light sources of the branch cylinder
			//							// 0=> the inner  circular wall. 
			//							// 1 => the outer circular wall;
			//	public int NearestBoidID;
			//}


			struct BoidLEDData
			{
				float3  position; //
				float3  headDir; // heading direction of the boid on the local plane
				float4  color;         // RGBA color
				float3  scale;
				int    wallNo;      // the number of the wall whose boids defined the light sources of the branch cylinder
										// 0=> the inner  circular wall. 
										// 1 => the outer circular wall;
				int    nearestBoidID;
			};


		// shader side buffer struct:

            #define M_PI 3.1415926535897932384626433832795

       
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
			RWStructuredBuffer<BoidLEDData> _BoidLEDBuffer;	

			//RWStructuredBuffer<float4> _BoidLEDDebugData : register(u1);
        #endif
			
			struct v2f
            {
				float4 pos : SV_POSITION; // the clip space position
				float4 posWorld: TEXCOORD1; // world space pos of each vertex
                float4 normal : NORMAL;
                float2 uv : TEXCOORD0;
    //            SHADOW_COORDS(2) // create a struct member
				//UNITY_FOG_COORDS(1)
    //            fixed3 diff : COLOR0;
    //            fixed3 ambient : COLOR1;
				fixed4 color : COLOR2;
                
            };

			
            
            
			float4x4 identity()
			{
				return float4x4(
					1, 0, 0, 0,
					0, 1, 0, 0,
					0, 0, 1, 0,
					0, 0, 0, 1);
			}


			float3x3 rot_x(float u)
			{
				return float3x3(
					1,  0,       0,
					0, cos(u), -sin(u),
					0, sin(u),  cos(u) );
			}

			float3x3 rot_y( float v )
			{
				return float3x3(
					  cos(v),  0,  sin(v),
				       0,       1,   0,  
					  -sin(v),  0,  cos(v) );
			}

			float3x3 rot_z(float w)
			{
				return float3x3(
					cos(w),  -sin(w),     0,
					sin(w), cos(w),       0,
					0,        0,          1);
			}
			// EULER ANGLE: https://en.wikipedia.org/wiki/Euler_angles. It is assumed that Z-X-Y Euler angles are used
            // It belongs to Tait–Bryan angles which  are also called Cardan angles; Z = Roll, X = Pitch, Y = Yaw
			// The first is axis order; Unity uses the order Z-X-Y, which is fairly sensible given Unity's conventions. => Z = Roll, X = pitch, Y=Yaw
         
			// 	// scale->rotation->translation is a common order
			float4x4 transform(float3 scale, float3 angles, float3 position)
            {
                float4x4 mat = 0;
                
                // Scale
                mat._11_22_33_44 = float4( scale, 1); // scale along the x-z plane
				//mat.m_00_m11_m22_m33 = float4( scale, 1); // scale along the x-z plane
                // Angle
				float sh, ch, cb, sb, ca, sa;
				
				//sincos(float x, out s, out c): s is set to the sine of x, and c is set to the cosine of x.
				
                sincos(angles.y, sh, ch); // rotate about the y axis
				sincos(angles.x, sa, ca);  // rotate about x axis
                sincos(angles.z, sb, cb);    // rotate about z axis
				

                // R = Ry * Rx * Rz = Y1 * X2 * Z3. Z = Roll, X = Pitch, Y = Yaw

				// 
                float4x4 rot = float4x4(
                         ch * ca + sh * sb * sa, -ch * sa + sh * sb * ca, sh * cb, 0,
                                    cb * sa,                 cb * ca,     -sb,      0,
                        -sh * ca + ch * sb * sa,  sh * sa + ch * sb * ca, ch * cb,  0,
                                    0,                      0,       0,             1
                );

			
				
				 /*float4x4 rot4 = float4x4(
					 rot[0][0], rot[0][1], rot[0][2], 0,
					 rot[1][0], rot[1][1], rot[1][2], 0,
					 rot[2][0], rot[2][1], rot[2][2], 0,
					 0,           0,           0,     1
					 );*/
                mat = mul(rot, mat);
                
				// maty = T*R*S
                // Position
                mat._14_24_34 = position;  
			
                return mat;
            } // transform()

			float4x4 transform(float3 scale, float3x3 rot, float3 position)
			{
				float4x4 mat = 0;

				// Scale
				mat._11_22_33_44 = float4(scale, 1); // scale along the x-z plane
													 //mat.m_00_m11_m22_m33 = float4( scale, 1); // scale along the x-z plane
				//									 // Angle
				//float sh, ch, cb, sb, ca, sa;

				////sincos(float x, out s, out c): s is set to the sine of x, and c is set to the cosine of x.

				//sincos(angles.y, sh, ch); // rotate about the y axis
				//sincos(angles.x, sa, ca);  // rotate about x axis
				//sincos(angles.z, sb, cb);    // rotate about z axis


				//							 // R = Ry * Rx * Rz = Y1 * X2 * Z3. Z = Roll, X = Pitch, Y = Yaw

				//							 // 
				//float4x4 rot = float4x4(
				//	ch * ca + sh * sb * sa, -ch * sa + sh * sb * ca, sh * cb, 0,
				//	cb * sa, cb * ca, -sb, 0,
				//	-sh * ca + ch * sb * sa, sh * sa + ch * sb * ca, ch * cb, 0,
				//	0, 0, 0, 1
				//	);



				float4x4 rot4 = float4x4(
				rot[0][0], rot[0][1], rot[0][2], 0,
				rot[1][0], rot[1][1], rot[1][2], 0,
				rot[2][0], rot[2][1], rot[2][2], 0,
				0,           0,           0,     1
				);
				mat = mul(rot4, mat);

				// maty = T*R*S
				// Position
				mat._14_24_34 = position;

				return mat;
			} // transform()


			float4x4 transformRot(float3 angles)
			{
				//float4x4 mat = 0;

				// Scale
				//mat._11_22_33_44 = float4(1,1,1,1); 
				float sh, ch, cb, sb, ca, sa;

				//sincos(float x, out s, out c): s is set to the sine of x, and c is set to the cosine of x.

				sincos(angles.y, sh, ch); // rotate about the y axis
				sincos(angles.x, sa, ca);  // rotate about x axis
				sincos(angles.z, sb, cb);    // rotate about z axis


											 // R = Ry * Rx * Rz = Y1 * X2 * Z3. Z = Roll, X = Pitch, Y = Yaw

											 // 
				float4x4 rot = float4x4(
					ch * ca + sh * sb * sa, -ch * sa + sh * sb * ca, sh * cb, 0,
					cb * sa, cb * ca, -sb, 0,
					-sh * ca + ch * sb * sa, sh * sa + ch * sb * ca, ch * cb, 0,
					0, 0, 0, 1
					);



				/*float4x4 rot4 = float4x4(
				rot[0][0], rot[0][1], rot[0][2], 0,
				rot[1][0], rot[1][1], rot[1][2], 0,
				rot[2][0], rot[2][1], rot[2][2], 0,
				0,           0,           0,     1
				);*/
				//mat = mul(rot, mat);

				
				//return mat;

				return rot;
			} // transformRot()

			//The right-handed coordinate system use right hand rule to determine the direction of 
			//the cross product, while the left-handed coordinate system use left hand rule,
			//and hence the result is the same. This means cross product can not be used to determine the handedness. 
			//Suppose x = (1,0,0), and y = (0, 1, 0). I think we can all agree that x cross y = z = (0,0,1). Now, is this right or left handed coordinates?
			//It actually doesn't matter. Cross product still works the same

			// shaders with unity3D: http://xboxoneindiedevelopment.blogspot.kr/2015/02/coming-from-shaders-in-xna-to-shaders.html

            v2f vert (appdata_full v, uint instanceID : SV_InstanceID )
				// instanceID => each BoidLEDCylinder mesh; v = each vertex in the current mesh instance
            {


				////////////////////////////////
           // #if SHADER_TARGET >= 45
                BoidLEDData b = _BoidLEDBuffer[instanceID];
                	
				//int wallNo = b.wallNo;

				int wallNo = 1; // Boid LEDs are on the ceiling;

				float3 wallOrigin = float3(0.0, 0.0, 0.0); // used to determine the origin of the boid frame
				//float3 eulerAngles; // ZXY Euler angles
				
				float4x4 rot1 = identity();
				float4x4 rot2; // used to determine the axes of the boid frame

				if (wallNo == 0) // boid on the ground domain whose reference position is (0, GroundMinCorner.y,0)
				{
					wallOrigin = float3(0.0, 0.0, 0.0);
					//eulerAngles = float3(0.0, 0.0, 0.0);
					rot1 = identity();
					
				}

				if (wallNo == 1) // boid on the ceiling domain whose reference position is (0, CeilingMaxCorner.y, 0)
				{
					wallOrigin = float3 (0.0, CeilingMaxCorner.y, 0.0);

					//wallOrigin = float3 (0.0, 12.0, 0.0);
					//eulerAngles = float3(M_PI, 0.0, 0.0); // pitch  = 180
				//	rot = rot_x(M_PI);
					// This rot is used to determine the axes of the boid frame

					rot1 = float4x4(1, 0, 0,0,
						           0, -1, 0, 0,
						           0, 0, -1, 0,
						           0 ,0,  0, 1          
						 
						   );

					// consider the head direction of the boid

				}

				float3 boidLoc = wallOrigin + b.position;

				float3 scale = b.scale;

				// The boid's head is assumed to be aligned with the z axis. 
				// There is no roll motion about the z axis, only the pitch about the x axis
				// and the yaw about the y axis is allowed
				// rotX is the angle between y axis and the head direction b.headDir; this angle
				// is the same as the angle between the original y axis and the new y aixs
				// obtained when the pitch about the x axis is performed
				// 

				// if headDir.y ==0 (boid on 2D xz plane) => rotX =0. Only yaw on xz plane

				float rotX = -asin(b.headDir.y / length(b.headDir.xyz) + 1e-8); // 0으로 나누기 방지
				float rotY = atan2(b.headDir.x, b.headDir.z); // rotY is the angle between z axis and the
				// projection of the boid dir on the xz plane; it is the angle about the y axis

				// R = Ry * Rx * Rz 
				//float4x4 object2world = transform(
				//	b.scale,
				//	float3(rotX, rotY, 0), // pitch => yaw => no roll
				//	boidLoc
				//);

				

				float4x4 scaleMat = float4x4(
					scale[0],    0,        0,           0,
					0,           scale[1],  0,          0,
					0,              0,       scale[2],  0,
					0,             0,            0,        1
					);


				rot2 = transformRot( float3(rotX, rotY, 0) );

				float4x4 rot = mul(rot1, rot2);

				/*float4x4 boidTRMat = float4x4(
						rot[0][0], rot[0][1], rot[0][2], boidLoc[0],
						rot[1][0], rot[1][1], rot[1][2], boidLoc[1],
						rot[2][0], rot[2][1], rot[2][2], boidLoc[2],
						0,            0,            0,      1
					);
				

				

				float4x4 object2world = mul( boidTRMat, scaleMat);*/

				//_Scale = float3(1, 1, 1);
				float4x4 object2world = transform( _Scale * scale, rot, boidLoc);


				// transform position to the clip space
                v2f o;
				
				// Note float4 pos : SV_POSITION; // the clip space position
				// mul(UNITY_MATRIX_VP, mul(unity_ObjectToWorld, float4(pos, 1.0)));
                //o.pos = UnityObjectToClipPos( mul( object2world, v.vertex)  ); // v: the current vertex in the current mesh instance
				
				o.posWorld = mul(object2world, v.vertex);
				o.pos = mul( UNITY_MATRIX_VP, o.posWorld);

                o.normal = normalize( mul( object2world, v.normal));
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

				// save the boid LED color the debug buffer
				//_BoidLEDRenderDebugBuffer[instanceID] =  b.color;

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
				return i.color;

				//return float4(i.color.rgb, 1.0);
				//return float4(1.0, 1.0,0.0,1.0);

				//int unity_InstanceID;
				//UNITY_SETUP_INSTANCE_ID(i); //  necessary only if any instanced properties are going to be accessed in the fragment Shader

				float3 normalDirection = i.normal;
				float3 viewDirection = normalize(
					_WorldSpaceCameraPos - i.posWorld.xyz);
				float3 lightDirection;
				float attenuation;

				if (0.0 == _WorldSpaceLightPos0.w) // directional light?
				{
					attenuation = 1.0; // no attenuation
					lightDirection =
						normalize(_WorldSpaceLightPos0.xyz);
				}
				else // point or spot light
				{
					float3 vertexToLightSource =
						_WorldSpaceLightPos0.xyz - i.posWorld.xyz;

					float distance = length(vertexToLightSource);

					attenuation = 1.0 / distance; // linear attenuation 
					lightDirection = normalize(vertexToLightSource);
				}

				//float3 ambientLighting =
				//	UNITY_LIGHTMODEL_AMBIENT.rgb * i.color.rgb;

				float3 diffuseReflection =
					attenuation * _LightColor0.rgb * i.color.rgb
					            * max(0.0, dot(normalDirection, lightDirection));

				float3 specularReflection;

				if (dot(normalDirection, lightDirection) < 0.0)
					// light source on the wrong side?
				{
					specularReflection = float3(0.0, 0.0, 0.0);
					// no specular reflection
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

				//return float4( ambientLighting +  diffuseReflection + specularReflection, 1.0);
				return float4(   diffuseReflection + specularReflection, 1.0);

            } // frag()

            ENDCG
        } // forward basepass


			UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
   
    } // Subshader
	
} // Shader
