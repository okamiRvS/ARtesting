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

    /// <summary>
    /// The takePhoto Game Object.
    /// </summary>
    [Tooltip("The takePhoto Game Object.")]
    [SerializeField] private GameObject takePhoto = null;

    private bool GridStatus = true;

    private static AndyPlacementManipulator instantObj;
    public GameObject controller;

    public void Start()
    {
        instantObj = controller.GetComponent<AndyPlacementManipulator>();
    }

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
        if (takePhoto.GetComponent<takeFrame>().takeF)
        {
            takePhoto.GetComponent<takeFrame>().takeF = false;
        }
        else
        {
            takePhoto.GetComponent<takeFrame>().takeF = true;
        }
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
                    debugText.text = debugText.text + "\n" + "Lenght: " + Vector3.Distance(points[0], points[1]) * 100 + "cm";
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
        instantObj.instantiateObj(Display.main.systemWidth / 2, Display.main.systemHeight / 2);
    }
}
