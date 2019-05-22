using GoogleARCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TMPro
{

    public class DistanceCameraToPlaneCloser : MonoBehaviour
    {

        public GameObject distance;
        Camera main;
        float minDistance;

        // Use this for initialization
        void Start()
        {
            distance.GetComponent<TextMeshProUGUI>().text = "";
            main = Camera.main;
            minDistance = Mathf.Infinity;
        }

        // Update is called once per frame
        void Update()
        {

            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;

            if (Frame.Raycast(Display.main.systemHeight/2, Display.main.systemWidth/2, raycastFilter, out hit))
            {
                if (hit.Trackable is DetectedPlane)
                {
                    Debug.DrawRay(gameObject.transform.position, hit.Pose.position - gameObject.transform.position, Color.green, 1);
                }
            }

            foreach (GameObject plane in GameObject.FindGameObjectsWithTag("plane"))
            {
                Vector3 tmp = plane.transform.position;
                float distance = Vector3.Distance(main.transform.position, tmp);
                if (distance < minDistance)
                {
                    minDistance = distance;
                }
            }
            distance.GetComponent<TextMeshProUGUI>().text = minDistance.ToString();


        }
    }
}