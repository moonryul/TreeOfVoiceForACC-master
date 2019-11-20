using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;


class BitmapEncoder2
{
	public static void WriteBitmap2(Stream stream, int width, int height, byte[] imageData)
	{
		using (BinaryWriter bw = new BinaryWriter(stream)) {

			// define the bitmap file header
			bw.Write ((UInt16)0x4D42); 								// bfType;
			bw.Write ((UInt32)(14 + 40 + (width * height * 4))); 	// bfSize;
			bw.Write ((UInt16)0);									// bfReserved1;
			bw.Write ((UInt16)0);									// bfReserved2;
			bw.Write ((UInt32)14 + 40);								// bfOffBits;
	 
			// define the bitmap information header
			bw.Write ((UInt32)40);  								// biSize;
			bw.Write ((Int32)width); 								// biWidth;
			bw.Write ((Int32)height); 								// biHeight;
			bw.Write ((UInt16)1);									// biPlanes;
			bw.Write ((UInt16)32);									// biBitCount;
			bw.Write ((UInt32)0);  									// biCompression;
			bw.Write ((UInt32)(width * height * 4));  				// biSizeImage;
			bw.Write ((Int32)0); 									// biXPelsPerMeter;
			bw.Write ((Int32)0); 									// biYPelsPerMeter;
			bw.Write ((UInt32)0);  									// biClrUsed;
			bw.Write ((UInt32)0);  									// biClrImportant;

			// switch the image data from RGB to BGR
			for (int imageIdx = 0; imageIdx < imageData.Length; imageIdx += 3) {
				bw.Write(imageData[imageIdx + 2]);
				bw.Write(imageData[imageIdx + 1]);
				bw.Write(imageData[imageIdx + 0]);
				bw.Write((byte)255);
			}
			
		}
	}

}

/// <summary>
/// Captures frames from a Unity camera in real time
/// and writes them to disk using a background thread.
/// </summary>
/// 
/// <description>
/// Maximises speed and quality by reading-back raw
/// texture data with no conversion and writing 
/// frames in uncompressed BMP format.
/// Created by Richard Copperwaite.
/// </description>
/// 
/// Attach this component to each camera to screen record the image rendered to that camera
/// 
[RequireComponent(typeof(Camera))]
public class ScreenRecorder2 : MonoBehaviour 
{
	// Public Properties
	public int maxFrames; // maximum number of frames you want to record in one video
	public int frameRate = 30; // number of frames to capture per second

	// The Encoder Thread ( background thread to write camera images to disk)
	private Thread encoderThread;

	// Texture Readback Objects
	private RenderTexture tempRenderTexture;
	private Texture2D tempTexture2D;

	// Timing Data
	private float captureFrameTime;
	private float lastFrameTime;
	private int frameNumber;
	private int savingFrameNumber;

	// Encoder Thread Shared Resources
	private Queue<byte[]> frameQueue;
	private string persistentDataPath;
	private int screenWidth;
	private int screenHeight;
	private bool threadIsProcessing;
	private bool terminateThreadWhenDone;
	
	void Start () 
	{
		// Set target frame rate (optional)
		Application.targetFrameRate = frameRate;

        Camera camera = GetComponent<Camera>();

        // Prepare the data directory
        //persistentDataPath = Application.persistentDataPath + "/ScreenRecorder" + camera.name;

        persistentDataPath = "./ScreenRecorder2" + camera.name;
        print("Capturing to: " + persistentDataPath + "/");

		if (!System.IO.Directory.Exists(persistentDataPath))
		{
			System.IO.Directory.CreateDirectory(persistentDataPath);
		}


        //https://answers.unity.com/questions/254159/modify-camera-pixelwidth-and-pixelheight.html
        // Prepare textures and initial values


        //Cameras by default take up the entire screen, but they can be changed to render only to a part of the screen(e.g. for an overhead minimap or a rear - view mirror) or to a texture(e.g. for an in-game security cam).
        //If you want to know the width of the whole screen available to unity, use Screen.width. If you want to know the width of the camera viewport (which might in some cases be smaller), use Camera.pixelWidth. 

        //screenWidth = GetComponent<Camera>().pixelWidth; // this getcomponent<Camera> will get the camera component
        // attached to the gameobject "CameraToGround" or "CameraToCeiling" to which the current script component ("ScreenRecorder")
        // is attached

        screenWidth = camera.pixelWidth; 
        screenHeight = camera.pixelHeight;

        // RenderTexture(int width, int height, int depth);// depth refers to the depth data bits
        // depth = 0 => no Z buffer is created by the RenderTexture

        tempRenderTexture = new RenderTexture(screenWidth, screenHeight, 0);

		tempTexture2D = new Texture2D(screenWidth, screenHeight, TextureFormat.RGB24, false); // no mipmap
		frameQueue = new Queue<byte[]> ();

		frameNumber = 0;
		savingFrameNumber = 0;

		captureFrameTime = 1.0f / (float)frameRate; // the time duration for a single frame
		lastFrameTime = Time.time;

		// Kill the encoder thread if running from a previous execution
		if (encoderThread != null && (threadIsProcessing || encoderThread.IsAlive)) {
			threadIsProcessing = false;
			encoderThread.Join();
		}

		// Start a new encoder thread
		threadIsProcessing = true;

		encoderThread = new Thread (EncodeAndSave);

		encoderThread.Start ();
	}
	
	void OnDisable() 
	{
		// Reset target frame rate
		Application.targetFrameRate = -1;

		// Inform thread to terminate when finished processing frames
		terminateThreadWhenDone = true;
	}

    //https://interplayoflight.wordpress.com/2015/07/03/adventures-in-postprocessing-with-unity/
    //MonoBehavior.OnRenderImage(): OnRenderImage is called OnRenderImage() which will be called 
    //every frame after rendering of the main scene has been completed.
    //OnRenderImage is used for image effects. The method receives 2 RenderTexture arguments, the source which contains the main scene rendered 
    //and the destination which will receive the result of our post effect pass.   
    void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
        // yield return WaitForEndOfFrame();

        //You can think of yield as a thread operation. In each frame the Unity engine waits 
        //    for all of its 'threads' to finish before advancing to the next frame
        //    (where 'threads' might be coroutines, OnGUI() methods, etc.).
        //    So you are correct: yield finishes(or pauses) execution of the code at that point 
        //    and tells the Unity engine 'I am done for this frame.' 
        //    When the next frame starts, it picks up where it left off, 
        //    with all variable and state information that it had the previous frame.

        print("SCREENRECORDER2 image capture 'OnRenderImage() thread' STARTED");

        if (frameNumber <= maxFrames)
		{
			// Check if render target size has changed, if so, terminate
            // If there are two cameras, source will be one of the multiple rendered images.
            // This camera may not match the camera of the current ScreenRecorder.

			if(source.width != screenWidth || source.height != screenHeight)
			{
				threadIsProcessing = false;
				this.enabled = false;
				throw new UnityException("ScreenRecorder2 render target size has changed!");
			}

            // now there a new frame to captureif there is a new frame to capture

            // check if enough time has been passed to record this frame. If more time has passed
            // record the same frame multiple times. 

            // Calculate number of video frames to produce from this game frame
            // Generate 'padding' frames if desired framerate is higher than actual framerate
            float thisFrameTime = Time.time;
			int framesToCapture = ((int)(thisFrameTime / captureFrameTime)) - ((int)(lastFrameTime / captureFrameTime));

			// Capture the frame
			if(framesToCapture > 0) // now there a new frame to captureif there is a new frame to capture
			{
                
                // Blit sets dest as the render target, sets source _MainTex property on the material, and draws a full - screen quad.
                //https://docs.unity3d.com/Manual/PostProcessingWritingEffects.html
                //Blit() without specifying a material will just copy the input to the output RenderTexture.
                // If the destination is null, the backbuffer will be rendered to unless that Camera.main.tergetTexture is set to
                // a RenderTexture.

                Graphics.Blit (source, tempRenderTexture);
				
				RenderTexture.active = tempRenderTexture; // equiv to Graphics.SetRenderTarget( tempRenderTexture)

                // Create a texture and read the  RenderTexture image to it.
                tempTexture2D.ReadPixels(new Rect(0, 0, Screen.width, Screen.height),0,0);

                RenderTexture.active = null;

                // All rendering goes into the active RenderTexture. If the active RenderTexture  null,
                // all rendering will go into the main window
			}

			// Add the required number of copies to the queue
			for(int i = 0; i < framesToCapture && frameNumber <= maxFrames; ++i)
			{
				frameQueue.Enqueue( tempTexture2D.GetRawTextureData() );
                // tempTexture2D may refer to the image rendered at the previous execuation of this
                // ScreenRecorder script

				frameNumber ++;

				if( frameNumber % frameRate == 0 )
				{
					print ("Frame " + frameNumber);
				}
			}
			
			lastFrameTime = thisFrameTime;

        } //for(int i = 0; i < framesToCapture && frameNumber <= maxFrames; ++i)

        else //keep making screenshots until it reaches the max frame amount
        { // frameNumber > maxFrames
          // Inform thread to terminate when finished processing frames
            terminateThreadWhenDone = true;

			// Disable script
			this.enabled = false;
		}

		// Passthrough: render to the default screen
		Graphics.Blit (source, destination); // destination is null
	} //OnRenderImage()
	
	private void EncodeAndSave()
	{
		print ("SCREENRECORDER2  IO THREAD STARTED");

		while (threadIsProcessing) 
		{
			if(frameQueue.Count > 0)
			{
				// Generate file path
				string path = persistentDataPath + "/frame" + savingFrameNumber + ".bmp";

				// Dequeue the frame, encode it as a bitmap, and write it to the file
				using(FileStream fileStream = new FileStream(path, FileMode.Create))
				{
					BitmapEncoder2.WriteBitmap2(fileStream, screenWidth, screenHeight, frameQueue.Dequeue());
					fileStream.Close();
				}

				// Done
				savingFrameNumber ++;
				print ("Saved " + savingFrameNumber + " frames. " + frameQueue.Count + " frames remaining.");
			}
			else
			{
				if(terminateThreadWhenDone)
				{
					break;
				}

				Thread.Sleep(1);
			}
		}

		terminateThreadWhenDone = false;
		threadIsProcessing = false;

		print ("SCREENRECORDER2 IO THREAD FINISHED");
	}
}