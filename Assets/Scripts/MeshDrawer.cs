using UnityEngine;
using System.Collections;

public class MeshDrawer : MonoBehaviour
{
    private Mesh mesh;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    private void Start()
    {
        CreateMesh();
    }

    private void CreateMesh()
    {
        Vector3[] vertices = new Vector3[6];
        int iterator = 0;

        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name.Contains("Carbon") || transform.GetChild(i).name.Contains("Oxygen") || transform.GetChild(i).name.Contains("RightEnd"))
            {
                if (transform.GetChild(i).name.Contains("RightEnd"))
                {
                    for (int j = 0; j < transform.GetChild(i).childCount; j++)
                    {
                        if (transform.GetChild(i).GetChild(j).name.Contains("Carbon"))
                        {
                            vertices[iterator] = transform.GetChild(i).GetChild(j).position;

                            iterator++;
                        }
                    }
                }
                else
                {
                    vertices[iterator] = transform.GetChild(i).position;

                    iterator++;
                }
            }
        }

        int[] triangles = new int[] { 0, 1, 5, 1, 2, 5, 2, 4, 5, 2, 3, 4 };

        if (transform.name.Contains("Nag")) triangles = new int[] { 0, 5, 1, 5, 4, 1, 4, 3, 2, 2, 1, 4 };

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = transform.InverseTransformPoint(vertices[i]);
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
    }
}