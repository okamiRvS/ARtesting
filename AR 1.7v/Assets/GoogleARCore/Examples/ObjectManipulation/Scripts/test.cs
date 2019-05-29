using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class test : MonoBehaviour
{
    public static Dictionary<string, float> volume = new Dictionary<string, float>()
        {
            {"banana", 918.853f},
            {"apple", 1915.687f},
            {"orange", 1915.687f}
        };

    public string whoIam = null;
    public float vol;

    Transform child;
    TextMesh p;

    public void changeText()
    {
        if (String.IsNullOrEmpty(whoIam)) { return; }

        foreach (string k in volume.Keys)
        {
            if (whoIam == k)
            {
                p = gameObject.GetComponent<TextMesh>();
                p.text = whoIam + ": " + volume[whoIam] + "cm^3";
            }
        }

        //find parent
        Transform par = gameObject.transform.parent;

        //find child object food
        child = par.transform.Find(whoIam + "(Clone)");

        if (child != null)
        {
            Debug.Log("Child found: " + child.name);
            vol = child.transform.lossyScale.x;
        }
        else
        {
            Debug.Log("Child not found");
            return;
        }
    }

    public void Update()
    {
        if (String.IsNullOrEmpty(whoIam)) { return; }

        changeVolume();
    }

    public void changeVolume()
    {
        float valScale = child.transform.lossyScale.x;

        if (vol != valScale)
        {
            p.text = whoIam + ": " + volume[whoIam] * Mathf.Pow(child.transform.lossyScale.x, 3f) + "cm^3";
            Debug.Log("The volume of the mesh is " + volume[whoIam] * Mathf.Pow(child.transform.lossyScale.x, 3f) + " cm^3.");
        }
    }
}
