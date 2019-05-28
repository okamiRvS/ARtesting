using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Volume : MonoBehaviour
{
    //private Text volumeText { get; set; }

    private bool check = true;
    Mesh mesh;
    float volume = 0;

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().sharedMesh;
    }

    private void Update()
    {
        //volumeText.transform.position = gameObject.transform.up * 2;
        if (check)
        {
            StartCoroutine(VolumeOfMesh());
            check = false;
        }
        //volumeText.text = volume.ToString();
        Debug.Log("The volume of the mesh is " + volume * Mathf.Pow(gameObject.transform.lossyScale.x, 3f) * Mathf.Pow(10, 6) + " cm^3.");
    }

    public float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float v321 = p3.x * p2.y * p1.z;
        float v231 = p2.x * p3.y * p1.z;
        float v312 = p3.x * p1.y * p2.z;
        float v132 = p1.x * p3.y * p2.z;
        float v213 = p2.x * p1.y * p3.z;
        float v123 = p1.x * p2.y * p3.z;
        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
    }

    IEnumerator VolumeOfMesh()
    {
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            Vector3 p1 = vertices[triangles[i + 0]];
            Vector3 p2 = vertices[triangles[i + 1]];
            Vector3 p3 = vertices[triangles[i + 2]];
            volume += SignedVolumeOfTriangle(p1, p2, p3);

            if (i % 1000 == 0)
            {
                Debug.Log("mesh.triangles.Length: " + mesh.triangles.Length + ", i: " + i + "gameObject.transform.lossyScale.x: " + gameObject.transform.lossyScale.x);
                yield return new WaitForSeconds(.3f);
            }
        }
        volume = Mathf.Abs(volume);
    }
}
