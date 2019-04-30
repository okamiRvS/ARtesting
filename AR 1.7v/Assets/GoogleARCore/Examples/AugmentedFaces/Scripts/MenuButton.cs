using GoogleARCore;
using GoogleARCore.Examples.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuButton : MonoBehaviour {

    public RawImage RawImage;

    /// <summary>
    /// A prefab for tracking and visualizing detected planes.
    /// </summary>
    bool GridStatus = true;

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
        CameraImageBytes image = Frame.CameraImage.AcquireCameraImageBytes();
        Texture2D texture = new Texture2D(image.Width, image.Height, TextureFormat.R8, false, false);
        if (!image.IsAvailable)
        {
            return;
        }

        int size = image.Width * image.Height;
        byte[] yBuff = new byte[size];
        System.Runtime.InteropServices.Marshal.Copy(image.Y, yBuff, 0, size);

        texture.LoadRawTextureData(yBuff);
        texture.Apply();

        RawImage.texture = texture;

    }

    public void QuitGame() {

        Debug.Log("QUIT !");
        Application.Quit();
    }
}
