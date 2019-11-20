Shader "Danbi/Prewarper" {
    Properties {
        _MainTex("CylinderTexture", 2D) = "white" {}

	// Cylinder properties should be included: MOON

	    _CylinderOriginInCamera("Cylinder Origin In Camera", Vector) = (0.0, 0.0, 0.0, 0.0)
		_CylinderHeight("Cylinder Height", Float) = 0.0
		_CylinderRadius("Cylinder Radius", Float) = 0.0


		_ScreenWidth("Screen Width", Float) = 0.0
		_ScreenHeight("Screen Height", Float) = 0.0
    }
        SubShader {
            Tags { "RenderType" = "Opaque" }

            Pass {
                CGPROGRAM


				//DirectX  ; https://docs.unity3d.com/Manual/UsingDX11GL3Features.html

				#pragma enable_d3d11_debug_symbols
                #pragma vertex vert
                #pragma fragment frag

			 /*   #pragma target 2.5 (default)
               Almost the same as 3.0 target(see below), except still only has 8 interpolators, and does not have explicit LOD texture sampling.
               Compiles into SM3.0 on DX9, and DX11 feature level 9.3 on Windows Phone.*/
				//#pragma target 5.0	   // to use RWStructuredBuffer 
				//                       // DX11 shader model 5.0.

				#pragma target 4.5	
                #include "UnityCG.cginc"
          
			// The _CylinderOrigin coordinates are defined with respect to the camera coordinate system which is the same
			// as used in glsl (OpenGL standard): The right hand system with the negative z axis being the view direction.
			 
			float4 GetIntersectedPointRayCylinder(float3 origin, float3 dir,  float cylRadius, float cylHeight, float4 cylOrigin);
		

            float2 Transform2FragCoordsCylinder(float3 intersected_point);
            //
            // Shader Data Layout
			// Using a computeBuffer in a regular graphic shader

			 //https://forum.unity.com/threads/write-in-a-custom-buffer-in-a-regular-shader-non-post-process.515357/
			 //https://gamedev.stackexchange.com/questions/128976/writing-and-reading-computebuffer-in-a-shader
		 	 //Semantics with the SV prefix are "system value" semantics. This means that they have a specific meaning to the pipeline. 
			
			//In the case of SV_Position, if it's attached to a vertex shader output, that means that the output will contain
			//the final TRANSFORMED vertex position used for rasterization. Or if you use it for a pixel shader input,
			//it will contain the screenspace position of the pixel. 
			//Any other semantics are defined completely by the user, and have no specific meaning.
			//Their only purpose is to match the outputs from one stage of the pipeline to the inputs of another stage.

			//Prior to DX10, all semantics were essentially system vale semantics and they were all predefined.
			//Back then, the POSITION semantic was used for both the input vertex position AS WELL AS 
			//the TRANSFORMED  output vertex position.
			//So you'll see it a lot in older shader code, or DX10 code compiled in compatibilty mode.

			/*VertexID
				A vertex id is used by each shader stage to identify each vertex.It is a 32 -
				bit unsigned integer whose default value is 0. It is assigned to a vertex when the primitive is processed by the IA stage.Attach the vertex - id semantic to the shader input declaration to inform the IA stage to generate a per - vertex id.

				The IA will add a vertex id to each vertex for use by shader stages.
				For each draw call, the vertex id is incremented by 1. 
				Across indexed draw calls, the count resets back to the start value.For ID3D11DeviceContext::DrawIndexed 
				and ID3D11DeviceContext::DrawIndexedInstanced, the vertex id represents the index value.
				If the vertex id overflows(exceeds 2³²– 1), it wraps to 0.*/
			struct appdata {
				uint vIndex : SV_VertexID;
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

			//In Direct3D 10 and later, the SV_Position semantic (when used in the context of a pixel shader) 
			//specifies screen space coordinates (offset by 0.5).
			struct v2f {
                float2 uv : TEXCOORD0;
                float4 posInClip : SV_POSITION;
				float4 screenPos :  TEXCOORD1;
                float3 normalInCamera : NORMAL;
                float3 posInCamera : TEXCOORD2;
				float3 hitPosOnCylinder : TEXCOORD3;
            };
     

            sampler2D _MainTex;
            float4 _MainTex_ST;

			float4 _CylinderOriginInCamera;
            float _CylinderHeight, _CylinderRadius;
			float _ScreenWidth, _ScreenHeight;

			/*For pixel shaders, the render targets and unordered - access views share the same resource
				slots when being written out.This means that UAVs must be given an offset 
				so that they are placed in the slots after the render target views that are being bound.

				Note  RTVs, DSV, and UAVs cannot be set independently; they all need to be set at the same time.*/

			/*For pixel shaders, UAV's use the same slots as render target views. 
			As explained by the docs for OMSetRenderTargetsAndUnorderedAccessViews,
			you'll need to bind your render target to slot 0 and your UAV to slot 1.*/
		    // https://www.gamedev.net/forums/topic/672525-writing-to-uav-buffer/
		

			RWStructuredBuffer<float4> _IntersectionBuffer : register(u1);
			RWStructuredBuffer<float4> 	_PosInCameraBuffer :  register(u2);
			RWStructuredBuffer<float4> 	_PosInClipBuffer : register(u3); 
			RWStructuredBuffer<float4>  _NormalInCameraBuffer :   register(u4);


			RWStructuredBuffer<float2> _UVMapBuffer : register(u5);

		   	// ComputeScreenPos(float4 pos): defined in unitycg.cginc): 
			//   Given a position in clip space (o.pos in the vertex shader),
			//   it returns the position of that point on the screen.	 
			//   not yet normalized (divided by w)
   //         #define V2F_SCREEN_TYPE float4
			//inline float4 ComputeScreenPos(float4 pos) {
			//	float4 o = pos * 0.5f; //why myltiply by .5f
   //         #if defined(UNITY_HALF_TEXEL_OFFSET)
			//	o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w * _ScreenParams.zw;
   //         #else
			//	o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
   //         #endif

   //         #if defined(SHADER_API_FLASH)
			//	o.xy *= unity_NPOTScale.xy;
   //         #endif

			//	o.zw = pos.zw;
			//	return o;
			// //ComputScreenPos() just remap [-w,w] to [0,w], it do not do any magic:
			//  ComputeScreenPos() expect you sample the texture in fragment shader using tex2Dproj(float4).
				//tex2Dproj() is similar to tex2D(), it just divide input's xy by w in hardware before sampling;
				//ComputeScreenPos() will just transform input from clip coordinate vertex position[-w, w] into[0, w]
				//then calling tex2DProj() will transform[0, w] into[0, 1], which is a valid texture sampling value.
				//	Refer: https://forum.unity.com/threads/what-does-the-function-computescreenpos-in-unitycg-cginc-do.294470/

			// USAGE: vertex shader
			//o.pos = UnityObjectToClipPos(v.vertex.xyz);
			//o.screenPos = ComputeScreenPos(o.pos); // using the UnityCG.cginc version unmodified

			//// fragment shader
			//float2 screenUV = i.screenPos.xy / i.screenPos.w;

			//}

            float4 GetIntersectedPointRayCylinder(float3 p, float3 dir, 
				                float cylRadius, float cylHeight, float4 cylOrigin)
			{
				// p = the pyramid hit position

                float px = p.x;
                float px2 = px * px; // px^2
                float dx = dir.x;

                float dx2 = dx * dx; // dx^2
                float pxdx =  px * dx;

                float py = p.y;
                float py2 = py * py; // py^2
                float dy = dir.y;

                float dy2 = dy * dy; // dy^2
                float pydy = py * dy;

                float r = cylRadius;
                float r2 = r * r; // r^2

                // 충돌점 계산에 필요한 't' 를 근의 공식으로 계산.
                // Calculate the 't' by using the quadratic formula for making a intersected point.
                // 이 식은 전개되었습니다.
                // This formular is unfolded.
                /*float t = -pxdx - pydy + sqrt(
                    ( (px2 * dx2) + (py2 * dy2) + (2 * pxdx * pydy)) +
                    (-(dx2 * px2) + -(dx2 * py2) + (dx2 * r2) + -(dy2 * px2) + -(dy2 * py2) + (dy2 * r2))
                );
                t /= (dx2 + dy2);*/




				float t = ( -(pxdx + pydy) 
					          + sqrt( (pxdx + pydy) * (pxdx + pydy)
					       	           - (dx2 + dy2)*(px2 + py2 - r2)
							    ) 
					      ) /  (dx2 + dy2);

                // 직선의 방정식과 원기둥의 방정식으로 충돌검사.
                // Check collision by using the equations of cylinder and the equation of line.
               // float hitPositionOnCylinder = cylOrigin.z + (cylOrigin.z + dir.z * t) * t;  // commented out by Moon

				float hitCylPositionX = px + dx * t;
				float hitCylPositionY = py + dy * t;
				float hitCylPositionZ = p.z +  dir.z * t;
                // 원기둥의 높이 범위 검사.
                // Check the range of heights of the cylinder. MOON:  the view direction is the negative Z axis of the camera
				// coordinate system, which looks down.  So, if you go up (relative to the world system), the coordinate
				// becomes greater. So cylHeight is added to go up.  

				//debug
				return float4(

					hitCylPositionX,
					hitCylPositionY,
					hitCylPositionZ,
					t
					);

      //          if (cylOrigin.z <= hitCylPositionZ  && hitCylPositionZ  <= cylOrigin.z + cylHeight) {
      //              // 충돌 점 리턴.
      //              return float4(
						//
						//hitCylPositionX,
						//hitCylPositionY,
      //                  hitCylPositionZ,
						//t
      //                  );
      //          }

      //          // Return the float3(0, 0, 0) If there's no intersection .
      //          // 충돌하지 않으면 zero 값 리턴.
      //          else return float4(1.0f, 0.0f, 0.0f, 0.0f);
            } //	GetIntersectedPointRayCylinder

            float2 Transform2TexCoordsCylinder(float4 intersected_point)
			{
				// theta = acos( x_0/ R); there are different cases of theta depending on x_0 and y_0:
				float yTex = ( intersected_point.z - _CylinderOriginInCamera.z ) / _CylinderHeight;
				float R = _CylinderRadius;

				float xTex;
				// find the angle theta that corresponds to the intersection point (x,y)
				float x = intersected_point.x;
				float y = intersected_point.y;
				float theta;

				// case (1)
				if (x >= 0.0 && y >= 0.0)
				{
					theta = acos(x / R);   xTex = theta / UNITY_TWO_PI;	   // 	// PI/2 > theta > 0.
				}
				else  
					if (x >= 0.0 && y <  0.0)		// case (2)
					{			   											
						
					theta = acos(x / R);   xTex = (UNITY_TWO_PI - theta) / UNITY_TWO_PI;
					// PI/2 > theta > 0.

					}
					else 
						if (x < 0.0 && y >= 0.0)		   	// case (3)
						{
							theta = acos(x / R);   xTex = theta / UNITY_TWO_PI;	   // 	//  PI > theta > PI/2.

						}
						else 

							if (x < 0.0 && y < 0.0)		  	// case (4)
							{
								theta = acos(x / R);      // 	//  PI > theta > PI/2.
								xTex = (UNITY_TWO_PI - theta) / UNITY_TWO_PI;
								// theta2 = pi + (pi - theta )	   = 2 * pi - theta
							}


                return float2( xTex, yTex);
            }

			// This is the shader for the "Pyramid" gameObject; It is because this shader is used
			//by the PyramidMat used by the MeshRenderer component of the Pyramid gameObject



			v2f vert(appdata v) {

				v2f o;

				//get the camera space coordinate of each vertex of the Pyramid mesh
				o.posInCamera = mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, v.vertex) ).xyz;

				// get the clip space coordinate of each vertex of the Pyramid mesh

				o.posInClip = UnityObjectToClipPos(v.vertex);

				// get the texture coordinate of each vertex of the Pyramid mesh
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				// get the normal coordinate of each vertex of the Pyramid mesh
				float3 normalInCamera =  mul(UNITY_MATRIX_V, mul(unity_ObjectToWorld, float4(v.normal, 0.0)) ).xyz;

				//o.normalInCamera = normalize( normalInCamera  );

				o.normalInCamera = normalInCamera;

				o.screenPos = ComputeScreenPos(o.posInClip); 	 // just remap [-w,w]x[-w,w]x[-w,w] to [0,w]^3

			
				float3 reflDirInCamera = normalize(reflect(-o.posInCamera, o.normalInCamera));

				float4 hitPosOnCylinder = GetIntersectedPointRayCylinder(
					o.posInCamera, reflDirInCamera,
					_CylinderRadius, _CylinderHeight, _CylinderOriginInCamera);
			
				 // debug

				o.hitPosOnCylinder = hitPosOnCylinder;


				_PosInCameraBuffer[v.vIndex]  = float4( o.posInCamera, 1.0);
				_PosInClipBuffer[v.vIndex] = o.posInClip;
				_NormalInCameraBuffer[v.vIndex] = float4( o.normalInCamera, 0.0);

				// Write t of the current pixel to the debug compute buffer

				_IntersectionBuffer[v.vIndex]  = hitPosOnCylinder;
			
				 
			   //

				return o;
			}

			// https://forum.unity.com/threads/writing-depth-value-in-fragment-program.66153/
			// SV_DEPTH
			// fragment main
			float4 frag(v2f i) : SV_Target
			{

							
                float3 ndc = i.screenPos.xyz / i.screenPos.w;	 // i.screenPos is affine and can be linearly interpoldated;
				                                             // uv = the normalized coordinate
															 // (v/w_n)/(1/w_n)
								

				// reflect() is a standard Cg library function included automatically, that is, not by   #include "UnityCG.cginc"
				// In reflect( L, N), L = -incomingDir, N= normal dir; IncomingDir is the direciton of the incoming vector 
				// See the figure in https://en.wikibooks.org/wiki/Cg_Programming/Unity/Specular_Highlights

				float3 reflDirInCamera = normalize( reflect( -i.posInCamera,  i.normalInCamera) );
			   				
				float4 hitPosOnCylinder = GetIntersectedPointRayCylinder(
					                      i.posInCamera, reflDirInCamera,
					                       _CylinderRadius, _CylinderHeight, _CylinderOriginInCamera);

				

			

				if (hitPosOnCylinder.w == 0.0f )
				{
					return float4(1.0f, 1.0f, 0.0f, 1.0f);// the red color
				}
				else
				{
					//debug
					//return float4(0.0f, 1.0f, 0.0f, 1.0f);// the red color

					float2 uv = Transform2TexCoordsCylinder(hitPosOnCylinder);
					
					return tex2D(_MainTex, uv);

					//save uv map 
					_UVMapBuffer[ndc.y * _ScreenWidth + ndc.x * _ScreenHeight] = uv;

		   //

				}
			}  //float4 frag(v2f i) 



            ENDCG
            }
        }
}
