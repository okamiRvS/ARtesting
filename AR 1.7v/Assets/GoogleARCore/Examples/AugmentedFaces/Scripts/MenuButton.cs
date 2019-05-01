using GoogleARCore;
using GoogleARCore.Examples.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using GoogleARCore.Examples.ComputerVision;
using System;
using System.IO;
using System.Runtime.InteropServices;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

public class MenuButton : MonoBehaviour {

    public RawImage RawImage;

    // [] 1
    public ARCoreSession ARSessionManager;
    private ARCoreSession.OnChooseCameraConfigurationDelegate m_OnChoseCameraConfiguration = null;
    // [] 1

    /// <summary>
    /// A prefab for tracking and visualizing detected planes.
    /// </summary>
    bool GridStatus = true;

    // [] 2
    public void Start()
    {
        m_OnChoseCameraConfiguration = _ChooseCameraConfiguration;
        ARSessionManager.RegisterChooseCameraConfigurationCallback(m_OnChoseCameraConfiguration);

        ARSessionManager.enabled = true;
    }

    private int _ChooseCameraConfiguration(List<CameraConfig> supportedConfigurations)
    {
        return supportedConfigurations.Count - 1;
    }
    // [] 2

    public void SetGrid () {
        foreach (GameObject plane in GameObject.FindGameObjectsWithTag("plane")) {
            DetectedPlaneVisualizer t = plane.GetComponent<DetectedPlaneVisualizer>();
            t.UpdateGridView(GridStatus);
        }

        if(GridStatus) {
            GridStatus = false;
        } else {
            GridStatus = true;
        }
    }

    public void TakeFrame()
    {
        // YUV TO RGB
        // https://github.com/google-ar/arcore-unity-sdk/issues/221
        // https://stackoverflow.com/questions/49579334/save-acquirecameraimagebytes-from-unity-arcore-to-storage-as-an-image
        // https://github.com/google-ar/arcore-unity-sdk/issues/527

        CameraImageBytes image = Frame.CameraImage.AcquireCameraImageBytes();
        if (!image.IsAvailable) return;

        // To save a YUV_420_888 image, you need 1.5*pixelCount bytes.
        // I will explain later, why.
        int bufferSize = (int)(image.Width * image.Height *1.5f);
        byte[] YUVimage = new byte[bufferSize];

        // As CameraImageBytes keep the Y, U and V data in three separate
        // arrays, we need to put them in a single array. This is done using
        // native pointers, which are considered unsafe in C#.
        unsafe
        {
            for (int i = 0; i < image.Width * image.Height; i++)
            {
                YUVimage[i] = *((byte*)image.Y.ToPointer() + (i * sizeof(byte)));
            }

            for (int i = 0; i < image.Width * image.Height / 4; i++)
            {
                YUVimage[(image.Width * image.Height) + 2 * i] = *((byte*)image.U.ToPointer() + (i * image.UVPixelStride * sizeof(byte)));
                YUVimage[(image.Width * image.Height) + 2 * i + 1] = *((byte*)image.V.ToPointer() + (i * image.UVPixelStride * sizeof(byte)));
            }
        }

        // Create the output byte array. RGB is three channels, therefore
        // we need 3 times the pixel count
        byte[] RGBimage = new byte[image.Width * image.Height * 3];

        // GCHandles help us "pin" the arrays in the memory, so that we can
        // pass them to the C++ code.
        GCHandle YUVhandle = GCHandle.Alloc(YUVimage, GCHandleType.Pinned);
        GCHandle RGBhandle = GCHandle.Alloc(RGBimage, GCHandleType.Pinned);

        Mat input_image = new Mat(image.Height + image.Height / 2, image.Width, CvType.CV_8UC1);
        Utils.copyToMat(YUVhandle.AddrOfPinnedObject(), input_image);
        Debug.Log(input_image.rows());

        Mat output_image = new Mat(image.Height, image.Width, CvType.CV_8UC3);
        Utils.copyToMat(RGBhandle.AddrOfPinnedObject(), output_image);

        Imgproc.cvtColor(input_image, output_image, Imgproc.COLOR_YUV2RGBA_NV12);

        // Create a new texture object
        Texture2D result = new Texture2D(image.Width, image.Height, TextureFormat.RGB24, false);

        Utils.matToTexture2D(output_image, result);

        RawImage.texture = result;

        YUVhandle.Free();
        RGBhandle.Free();
        
/*
var encodedPng = frame.EncodeToPNG();
var path = Application.persistentDataPath;
File.WriteAllBytes(path + "/images/" + date + ".png", encodedPng);
*/
    }

    public void QuitGame() {

        Debug.Log("QUIT !");
        Application.Quit();
    }
}
