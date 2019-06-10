using GoogleARCore;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCvYolo3;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class takeFrame : MonoBehaviour {

    public RawImage RawImage;
    Yolo3Android a;
    public bool takeF = false;
    public ARCoreSession ARSessionManager;
    public GameObject Caricamento;

    private void Start()
    {
        // Send Mat output_image to Yolo3Android to process objectDetection
        a = RawImage.GetComponent<Yolo3Android>();
    }

    [SerializeField] public float time = 5f;
    private bool r = true;

    // Update is called once per frame
    void Update()
    {
        if (r && takeF)
        {
            Caricamento.SetActive(true);
            StartCoroutine(move());
            r = false;
        }
        else
        {
            Caricamento.SetActive(false);
        }
    }

    IEnumerator move()
    {
        yield return new WaitForSeconds(time);

        //------------------
        ARSessionManager.enabled = false;
        //------------------

        CameraImageBytes image = Frame.CameraImage.AcquireCameraImageBytes();

        Mat rgb = yuv2rgb(image);
        a.InitializeImage(rgb);     //a.InitializeImage("0.png");

        image.Release();
        r = true;

        //-----------------
        ARSessionManager.enabled = true;
        //-----------------
    }

    public Mat yuv2rgb(CameraImageBytes image)
    {
        // YUV TO RGB
        // https://github.com/google-ar/arcore-unity-sdk/issues/221
        // https://stackoverflow.com/questions/49579334/save-acquirecameraimagebytes-from-unity-arcore-to-storage-as-an-image
        // https://github.com/google-ar/arcore-unity-sdk/issues/527
        // https://stackoverflow.com/questions/55495030/how-to-use-the-arcore-camera-image-in-opencv-in-an-unity-android-app/55495031#55495031

        if (!image.IsAvailable) { return null; }

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

        if (true)
        {
            Imgproc.cvtColor(input_image, output_image, Imgproc.COLOR_YUV2RGB_NV12);
        }
        else
        {
            Imgproc.cvtColor(input_image, output_image, Imgproc.COLOR_YUV2RGBA_NV12);
            // Create a new texture object
            Texture2D result = new Texture2D(image.Width, image.Height, TextureFormat.RGB24, false);
            Utils.matToTexture2D(output_image, result);
            savePic(result);
        }

        YUVhandle.Free();
        RGBhandle.Free();

        return output_image;
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

        // debugText.text = debugText.text + "\n" + "destPath: " + destPath;

        File.WriteAllBytes(destPath, encodedPng);
    }
}
