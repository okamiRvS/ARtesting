//-----------------------------------------------------------------------
// <copyright file="AndyPlacementManipulator.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.ObjectManipulation
{
    using GoogleARCore;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Controls the placement of Andy objects via a tap gesture.
    /// </summary>
    public class AndyPlacementManipulator : Manipulator
    {
        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// A model to place when a raycast from a user touch hits a plane.
        /// </summary>
        public GameObject[] Prefab;

        /// <summary>
        /// Manipulator prefab to attach placed objects to.
        /// </summary>
        public GameObject ManipulatorPrefab;

        /// <summary>
        /// List of food.
        /// </summary>
        public string[] foodString;

        /// <summary>
        /// The debugText text.
        /// </summary>
        [Tooltip("The debugText text.")]
        [SerializeField] private Text debugText = null;

        /// <summary>
        /// Returns true if the manipulation can be started for the given gesture.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        /// <returns>True if the manipulation can be started.</returns>
        protected override bool CanStartManipulationForGesture(TapGesture gesture)
        {
            if (gesture.TargetObject == null)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Function called when the manipulation is ended.
        /// </summary>
        /// <param name="gesture">The current gesture.</param>
        protected override void OnEndManipulation(TapGesture gesture)
        {
            if (gesture.WasCancelled)
            {
                return;
            }

            // If gesture is targeting an existing object we are done.
            if (gesture.TargetObject != null)
            {
                return;
            }

            if (gesture.StartPosition.y > Display.main.systemHeight * 0.2f)
            {
                instantiateObj(gesture.StartPosition.x, gesture.StartPosition.y);
            }
        }

        public void instantiateObj(float centerX, float centerY, string classNameObj = null)
        {
            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon;

            if (Frame.Raycast(centerX, centerY, raycastFilter, out hit))
            {
                // Use hit pose and camera pose to check if hittest is from the
                // back of the plane, if it is, no need to create the anchor.
                if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                {
                    Debug.Log("Hit at back of the current DetectedPlane");
                }
                else if (classNameObj==null || foodCheck(classNameObj))
                {
                    // Randomically choose a model for the Trackable that got hit.
                    GameObject prefab =  Prefab[(int)Random.Range(0, Prefab.Length)];

                    // Instantiate Andy model at the hit pose.
                    if (classNameObj == "banana")
                    {
                        prefab = Prefab[0];
                    }
                    else if (classNameObj == "apple")
                    {
                        prefab = Prefab[1];
                    }
                    else if (classNameObj == "orange")
                    {
                        prefab = Prefab[2];
                    }
                    else if (classNameObj == "cubeB")
                    {
                        prefab = Prefab[3];
                    }
                    else if (classNameObj == "sphereB")
                    {
                        prefab = Prefab[4];
                    }

                    debugText.text = debugText.text + "\n" + "classNameObj: " + classNameObj;
                    var andyObject = Instantiate(prefab, hit.Pose.position, hit.Pose.rotation);

                    Debug.Log("hit.Pose.position: " + hit.Pose.position + ",  hit.Pose.rotation: " + hit.Pose.rotation);

                    // Instantiate manipulator.
                    var manipulator = Instantiate(ManipulatorPrefab, hit.Pose.position, hit.Pose.rotation);

                    // Make Andy model a child of the manipulator.
                    andyObject.transform.parent = manipulator.transform;

                    // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                    // world evolves.
                    var anchor = hit.Trackable.CreateAnchor(hit.Pose);
                    
                    //-anchor.gameObject.tag = "anchor";
                   
                    // Make manipulator a child of the anchor.
                    manipulator.transform.parent = anchor.transform;

                    if (classNameObj != null)
                    {
                        test s = manipulator.transform.Find("textVolume").GetComponent<test>();
                        s.whoIam = classNameObj;
                        s.changeText();
                    }

                    // Select the placed object.
                    manipulator.GetComponent<Manipulator>().Select();

                    if (check(anchor.transform.position)) { Destroy(anchor.gameObject); }
                    anchor.gameObject.tag = "anchor";
                }
            }
        }

        private bool check(Vector3 s)
        {
            foreach (GameObject anchor in GameObject.FindGameObjectsWithTag("anchor"))
            {
                Debug.Log("DIFFERENCEEEEEEEEEEEEES: " + (Mathf.Abs(anchor.transform.position.sqrMagnitude - s.sqrMagnitude)));
                if (Mathf.Abs(anchor.transform.position.sqrMagnitude - s.sqrMagnitude) < 0.01f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool foodCheck(string food)
        {
            for(int i=0; i< foodString.Length; i++)
            {
                Debug.Log(i);
                if(food == foodString[i])
                {
                    debugText.text = debugText.text + "\n" + "food: " + foodString[i];
                    return true;
                }
            }
            return false;
        }
    }
}
