using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PupilLabs;

public class ScreenCastController : MonoBehaviour
{

    public ScreenCast caster;

    private void Awake()
    {
        caster.requestCtrl = FindObjectOfType<RequestController>();
        caster.timeSync = FindObjectOfType<TimeSync>();
    }
}
