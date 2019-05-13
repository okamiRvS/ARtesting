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
using OpenCvYolo3;
using OpenCVForUnity.ImgcodecsModule;

public class MenuButton : MonoBehaviour {

    public RawImage RawImage;

    /// <summary>
    /// The snackbar Game Object.
    /// </summary>
    [Tooltip("The debugBar Game Object.")]
    [SerializeField] private GameObject debugBar = null;

    /// <summary>
    /// The snackbar text.
    /// </summary>
    [Tooltip("The debugText text.")]
    [SerializeField] private Text debugText = null;

    /// <summary>
    /// The snackbar Game Object.
    /// </summary>
    [Tooltip("The currentFrme Game Object.")]
    [SerializeField] private GameObject currentFrme = null;

    private bool GridStatus = true;

    // [] 1
    public ARCoreSession ARSessionManager;
    private ARCoreSession.OnChooseCameraConfigurationDelegate m_OnChoseCameraConfiguration = null;

    public void Start()
    {
        // Register the callback to set camera config before arcore session is enabled.
        m_OnChoseCameraConfiguration = _ChooseCameraConfiguration;
        ARSessionManager.RegisterChooseCameraConfigurationCallback(m_OnChoseCameraConfiguration);

        // Pause and resume the ARCore session to apply the camera configuration.
        ARSessionManager.enabled = false;
        ARSessionManager.enabled = true;
    }

    private int _ChooseCameraConfiguration(List<CameraConfig> supportedConfigurations)
    {
        return supportedConfigurations.Count - 1;
    }
    // [] 1

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
        // https://stackoverflow.com/questions/55495030/how-to-use-the-arcore-camera-image-in-opencv-in-an-unity-android-app/55495031#55495031

        CameraImageBytes image = Frame.CameraImage.AcquireCameraImageBytes();
        if (!image.IsAvailable) return;

        Debug.Log(image.Width + " " + image.Height);

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

        Mat output_image = new Mat(image.Height, image.Width, CvType.CV_8UC3);
        Utils.copyToMat(RGBhandle.AddrOfPinnedObject(), output_image);

        Imgproc.cvtColor(input_image, output_image, Imgproc.COLOR_YUV2RGBA_NV12);
        Debug.Log("input_image" + CvType.typeToString(input_image.type()));
        Debug.Log("output_image" + CvType.typeToString(output_image.type()));
        Debug.Log("output_imageToString " + output_image.ToString());
        
        // Create a new texture object
        Texture2D result = new Texture2D(image.Width, image.Height, TextureFormat.RGB24, false);

        Utils.matToTexture2D(output_image, result);

        YUVhandle.Free();
        RGBhandle.Free();

        RawImage.texture = result;

        // Save pic on streamngAssets
        byte[] encodedPng = result.EncodeToPNG();
        string destPath = Path.Combine(Application.streamingAssetsPath, "dnn/0.png");

        if (Application.platform == RuntimePlatform.Android)
        {
            destPath = Path.Combine(Application.persistentDataPath, "opencvforunity");
            destPath = Path.Combine(destPath, "dnn/0.png");
        }

        debugText.text = debugText.text + "\n" + "destPath: " + destPath;

        File.WriteAllBytes(destPath, encodedPng);

        // Send Mat output_image to Yolo3Android to process objectDetection
        Yolo3Android a = RawImage.GetComponent<Yolo3Android>();
        a.InitializeImage("0.png");
    }

    public void DeleteObj()
    {
        foreach (GameObject anchor in GameObject.FindGameObjectsWithTag("anchor"))
        {
            Destroy(anchor);
        }
    }

    public void QuitGame() {

        Debug.Log("QUIT !");
        Application.Quit();
    }

    public void Console()
    {
        if(debugBar.activeSelf)
        {
            debugBar.SetActive(false);
        } 
        else
        {
            debugBar.SetActive(true);
        }
    }

    public void PicView()
    {
        if (currentFrme.activeSelf)
        {
            currentFrme.SetActive(false);
        }
        else
        {
            currentFrme.SetActive(true);
        }
    }
}
