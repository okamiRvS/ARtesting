using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class test : MonoBehaviour
{
    public static Dictionary<string, float> volume = new Dictionary<string, float>()
        {
            {"banana", 1523.465f},
            {"apple", 1915.687f},
            {"orange", 1915.687f}
        };

    public string whoIam = null;

    public void Update()
    {
        if(String.IsNullOrEmpty(whoIam)) { return; }

        foreach (string k in volume.Keys)
        {
            Debug.Log("update" + whoIam);
            if(whoIam == k)
            {
                TextMesh p = gameObject.GetComponent<TextMesh>();
                p.text = whoIam + ": " + volume[whoIam] + "cm^3";
            }
        }
    }
}
