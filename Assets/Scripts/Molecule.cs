using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]

public class Molecule : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 10;

    [HideInInspector]
    public bool thresholdReached = false;

    public Transform[] bindings = new Transform[6];
    public Transform[] atoms = new Transform[6];
    public Transform waterOxygen;

    private Transform carbon;
    private Transform oxygen;
    private Transform rightEnd;
    private List<Transform> children = new List<Transform>();

    //Organize references for each child transform in the molecule
    private void Awake()
    {
        ListChildren(transform);
    }
    private void Start()
    { 
        foreach(Transform t in children)
        {
            switch (t.name)
            {
                case "Binding 1":
                    bindings[0] = t;
                    break;
                case "Binding 2":
                    bindings[1] = t;
                    break;
                case "Binding 3":
                    bindings[2] = t;
                    break;
                case "Binding 4":
                    bindings[3] = t;
                    break;
                case "Binding 5":
                    bindings[4] = t;
                    break;
                case "Binding 6":
                    bindings[5] = t;
                    break;
                case "Carbon 1":
                    atoms[0] = t;
                    break;
                case "Carbon 2":
                    atoms[1] = t;
                    break;
                case "Carbon 3":
                    atoms[3] = t;
                    break;
                case "Carbon 4":
                    atoms[4] = t;
                    break;
                case "Carbon 5":
                    atoms[5] = t;
                    break;
                case "Oxygen":
                    atoms[2] = t;
                    break;
                case "RightEnd":
                    rightEnd = t;
                    break;
                case "WaterOx":
                    waterOxygen = t;
                    t.gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }
    }

    private void ListChildren(Transform t)
    {
        foreach (Transform _t in t)
        {
            children.Add(_t);

            if (_t.childCount > 0)
            {
                ListChildren(_t);
            }
        }
    }

    public IEnumerator RotateMolecule()
    {
        Vector3 rotationAxis = carbon.position - oxygen.position;
        Quaternion from = rightEnd.rotation;

        while (Quaternion.Angle(rightEnd.rotation, from) < 15)
        {
            rightEnd.transform.RotateAround(oxygen.position, rotationAxis, Time.deltaTime * rotationSpeed * -1);

            yield return null;
        }
        yield return null;
    }
}
