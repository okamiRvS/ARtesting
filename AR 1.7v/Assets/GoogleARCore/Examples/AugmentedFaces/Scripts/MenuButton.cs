using GoogleARCore;
using GoogleARCore.Examples.Common;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Runtime.InteropServices;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCvYolo3;
using GoogleARCore.Examples.ObjectManipulation;

public class MenuButton : MonoBehaviour
{

    public RawImage RawImage;

    /// <summary>
    /// The prefab to measure.
    /// </summary>
    [Tooltip("The prefab to measure.")]
    [SerializeField] private GameObject prefab = null;

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

    private static AndyPlacementManipulator instantObj;
    public GameObject controller;

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

        instantObj = controller.GetComponent<AndyPlacementManipulator>();
    }

    private int _ChooseCameraConfiguration(List<CameraConfig> supportedConfigurations)
    {
        return supportedConfigurations.Count - 1;
    }
    // [] 1
    
    public void SetGrid()
    {
        foreach (GameObject plane in GameObject.FindGameObjectsWithTag("plane"))
        {
            DetectedPlaneVisualizer t = plane.GetComponent<DetectedPlaneVisualizer>();
            t.UpdateGridView(GridStatus);
        }

        if (GridStatus)
        {
            GridStatus = false;
        }
        else
        {
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
        int bufferSize = (int)(image.Width * image.Height * 1.5f);
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

        // Send Mat output_image to Yolo3Android to process objectDetection
        Yolo3Android a = RawImage.GetComponent<Yolo3Android>();

        if (false)
        {
            Imgproc.cvtColor(input_image, output_image, Imgproc.COLOR_YUV2RGB_NV12);
            a.InitializeImage(output_image);
        }
        else
        {
            Imgproc.cvtColor(input_image, output_image, Imgproc.COLOR_YUV2RGBA_NV12);

            // Create a new texture object
            Texture2D result = new Texture2D(image.Width, image.Height, TextureFormat.RGB24, false);
            Utils.matToTexture2D(output_image, result);
            RawImage.texture = result;
            savePic(result);

            a.InitializeImage("0.png");
        }

        /*
        Debug.Log("input_imageToString" + input_image.ToString());
        Debug.Log("output_imageToString " + output_image.ToString());

        debugText.text = debugText.text + "\n" + "input_imageToString " + input_image.ToString();
        debugText.text = debugText.text + "\n" + "output_imageToString " + output_image.ToString();
        */

        YUVhandle.Free();
        RGBhandle.Free();
    }

    public void savePic(Texture2D result)
    {
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
    }

    public void QuitGame()
    {

        Debug.Log("QUIT !");
        Application.Quit();
    }

    public void Console()
    {
        if (debugBar.activeSelf)
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


    Vector3[] points = new Vector3[2];
    bool init = true;
    List<GameObject> l2p = new List<GameObject>();
    LineRenderer line;

    public void Distance2Point()
    {
        debugText.text = debugText.text + "\n" + "Volume banana: " + test.volume["banana"] + "cm^3";
        debugText.text = debugText.text + "\n" + "Volume apple: " + test.volume["apple"] + "cm^3";

        line = GetComponent<LineRenderer>();

        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;
        if (Frame.Raycast(Display.main.systemWidth / 2, Display.main.systemHeight / 2, raycastFilter, out hit))
        {
            if (hit.Trackable is DetectedPlane)
            {
                if (init)
                {
                    if (l2p.Count != 0)
                    {
                        Destroy(l2p[0]);
                        Destroy(l2p[1]);
                        l2p.Clear();

                        line.enabled = false;
                    }

                    points[0] = hit.Pose.position;
                    l2p.Add(Instantiate(prefab, hit.Pose.position, hit.Pose.rotation));
                    l2p[0].tag = "anchor";
                    init = false;
                }
                else
                {
                    points[1] = hit.Pose.position;
                    debugText.text = debugText.text + "\n" + "Lenght: " + Vector3.Distance(points[0],points[1]) * 100 + "cm" ;
                    l2p.Add(Instantiate(prefab, hit.Pose.position, hit.Pose.rotation));
                    l2p[1].tag = "anchor";

                    line.enabled = true;
                    line.SetPositions(points);
                    init = true;
                }
            }
        }
    }

    public void DeleteObj()
    {
        foreach (GameObject anchor in GameObject.FindGameObjectsWithTag("anchor"))
        {
            Destroy(anchor);
        }

        line.enabled = false;
    }

    public void InstantiateRandom()
    {
        instantObj.instantiateObj(Display.main.systemWidth / 2, Display.main.systemHeight / 2, "banana");
    }
}
