using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;

public class ArmScript : MonoBehaviour
{
    public Transform handEnd;
    public Transform shoulderEnd;
    public Transform armModel;

    void FixedUpdate()
    {
        Vector3 direction = handEnd.position - shoulderEnd.position;
        Vector3 position = shoulderEnd.position + direction * 0.5f;

        transform.position = position;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        armModel.transform.localScale = new Vector3(armModel.transform.localScale.x, 0.5f * direction.magnitude, armModel.transform.localScale.z);
    }
}
