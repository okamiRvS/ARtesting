using GoogleARCore.Examples.Common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;


public class MenuButton : MonoBehaviour {

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

    public void QuitGame() {

        Debug.Log("QUIT !");
        Application.Quit();
    }
}
