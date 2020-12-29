using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System;
using System.Collections;

public class CanvasPositioner : MonoBehaviour
{

    [HideInInspector] public bool fade, stopRaycastingAfter;
    [HideInInspector] public int readTime;

    [HideInInspector] public float canvasDistance;

    void OnEnable()
    {
        StartCoroutine(SetPosAtStart());
    }

    IEnumerator SetPosAtStart()
    {
        yield return null;

        if (canvasDistance > 0) UIManager.instance.SetCanvasPosition(transform.root, canvasDistance);
        else UIManager.instance.SetCanvasPosition(transform.root);

        if (GetComponentInChildren<Button>() != null)
        {
            ControllerManager.instance.StartRaycasting();
        }
    }

    private void OnDisable()
    {
        if(stopRaycastingAfter) ControllerManager.instance.StopRaycasting();
    }
}
#if (UNITY_EDITOR)
[CustomEditor(typeof(CanvasPositioner))]
public class ScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var myScript = target as CanvasPositioner;

        myScript.stopRaycastingAfter = GUILayout.Toggle(myScript.stopRaycastingAfter, "Stop Raycasting: ");

        myScript.fade = GUILayout.Toggle(myScript.fade, "Fade: ");
        

        if (myScript.fade)
            myScript.readTime = EditorGUILayout.IntSlider("Read Time: ", myScript.readTime, 1, 60);

        myScript.canvasDistance = EditorGUILayout.FloatField("Canvas Distance: ", myScript.canvasDistance);

        Undo.RecordObject(myScript, "Canvas Changed");
    }
}
#endif