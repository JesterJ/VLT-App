using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;
using UnityEditor;

public class Stage1Manager : MonoBehaviour
{
    private bool embodied = false;
    public float embodyThreshold = 0.5f;
    public GameObject leftModel, rightModel, leftControllerModel, rightControllerModel;
    [SerializeField] public List<GameObject> texts;
    public bool hasTexts, embodyAtStart = true, _showGuidingArrows = false;

    private void Start()
    {
        if (LocalSceneManager.instance.stage0done || !embodyAtStart)
        {
            PrepStage1();
        }
        else
        {
            PrepStage0();
        }
    }

    private void PrepStage0()
    {
        StartCoroutine(CheckPose());
    }

    IEnumerator CheckPose()
    {
        if (hasTexts) StartCoroutine(UIManager.instance.ShowText(texts[0]));

        GameObject rightArrow = UIManager.instance.PlayAnimation(AnimationType.ArrowUp);
        GameObject leftArrow = UIManager.instance.PlayAnimation(AnimationType.ArrowUp);

        float counter = 0;
        float interval = 0.1f;

        while (!embodied)
        {
            float threshold = VivePose.GetPose(DeviceRole.Hmd).pos.y * embodyThreshold;
            float rightHandProgress = Mathf.Clamp(VivePose.GetPose(HandRole.RightHand).pos.y / VivePose.GetPose(DeviceRole.Hmd).pos.y, 0, 1);
            float leftHandProgress = Mathf.Clamp(VivePose.GetPose(HandRole.LeftHand).pos.y / VivePose.GetPose(DeviceRole.Hmd).pos.y, 0, 1);

            UIManager.instance.SetArrowUpPosition(rightArrow, HandRole.RightHand);
            UIManager.instance.SetArrowUpPosition(leftArrow, HandRole.LeftHand);

            if (leftHandProgress > threshold && rightHandProgress > threshold)
            {
                counter += Time.deltaTime;

                if (counter >= interval + 0.02f)
                {
                    ViveInput.TriggerHapticVibration(HandRole.RightHand, interval, 85 * rightHandProgress, 0.25f * rightHandProgress);
                    ViveInput.TriggerHapticVibration(HandRole.LeftHand, interval, 85 * leftHandProgress, 0.25f * leftHandProgress);

                    counter = 0;
                }

                if (VivePose.GetPose(HandRole.RightHand).pos.y > VivePose.GetPose(DeviceRole.Hmd).pos.y && VivePose.GetPose(HandRole.LeftHand).pos.y > VivePose.GetPose(DeviceRole.Hmd).pos.y)
                {
                    UIManager.instance.StopAnimation(rightArrow);
                    UIManager.instance.StopAnimation(leftArrow);
                    embodied = true;

                    yield return null;

                    DataRecorder.instance.SetEvent("Embodied");
                    ViveInput.TriggerHapticVibration(HandRole.RightHand, interval, 200, 0.9f);
                    ViveInput.TriggerHapticVibration(HandRole.LeftHand, interval, 200, 0.9f);

                    LocalSceneManager.instance.stage0done = true;
                    PrepStage1();

                    break;
                }
            }

            yield return null;
        }
    }

    private void PrepStage1()
    {
        if (hasTexts && LocalSceneManager.instance.currentStage.thisScene == "1.0 Stage") StartCoroutine(UIManager.instance.ShowText(texts[1]));
        else if (hasTexts && LocalSceneManager.instance.currentStage.thisScene.Contains("Hub"))
        {
            if (!LocalSceneManager.instance.hubDone) 
            { 
                StartCoroutine(UIManager.instance.ShowText(texts.GetRange(0, 3)));
                LocalSceneManager.instance.hubDone = true;
            }
            else StartCoroutine(UIManager.instance.ShowText(texts[3]));
        }


        ControllerManager.instance.showGuidingArrows = true;
        ControllerManager.instance.StartRaycasting();
        rightControllerModel.SetActive(false);
        leftControllerModel.SetActive(false);
        rightModel.SetActive(true);
        leftModel.SetActive(true);

        ControllerManager.instance.Grab += OnButtonDown;
    }

    private void OnDisable()
    {
        if(ControllerManager.instance.Grab != null)
        {
            ControllerManager.instance.Grab -= OnButtonDown;
            ControllerManager.instance.StopRaycasting();
        }
    }

    private void OnButtonDown(HandRole fromSource)
    {
        switch (fromSource)
        {
            case HandRole.RightHand:
                if (ControllerManager.instance.rightTarget != null)
                {
                    if (ControllerManager.instance.rightTarget.GetComponent<Bacteria>() != null)
                    {
                        OnGrab(ControllerManager.instance.rightTarget, fromSource);
                    }
                }
                break;
            case HandRole.LeftHand:
                if (ControllerManager.instance.leftTarget != null)
                {
                    if (ControllerManager.instance.leftTarget.GetComponent<Bacteria>() != null)
                    {
                        OnGrab(ControllerManager.instance.leftTarget, fromSource);
                    }
                }
                break;
        }
    }
    void OnGrab(GameObject target, HandRole hand)
    {
        ControllerManager.instance.RegisterPullPush(hand, target.GetComponent<Bacteria>().MoveBacteria);
        target.GetComponent<Bacteria>().StartRotating(hand);
    }
}
#if (UNITY_EDITOR)
[CustomEditor(typeof(Stage1Manager))]
public class HubEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var myScript = target as Stage1Manager;

        myScript._showGuidingArrows = EditorGUILayout.Toggle("Show Guiding Arrows:", myScript._showGuidingArrows);

        myScript.embodyAtStart = EditorGUILayout.Toggle("Embody at start:", myScript.embodyAtStart);

        if (myScript.embodyAtStart)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Embody Threshold:", GUILayout.Width(110));
            myScript.embodyThreshold = EditorGUILayout.FloatField(myScript.embodyThreshold, GUILayout.Width(35));
            myScript.embodyThreshold = GUILayout.HorizontalSlider(myScript.embodyThreshold, 0.5f, 1);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(200));
            GUILayout.Label("");
            GUILayout.Label("Hand Models:");
            GUILayout.Label("Controller Models:");
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(200));
            GUILayout.Label("Left Models:");
            myScript.leftModel = (GameObject)EditorGUILayout.ObjectField(myScript.leftModel, typeof(GameObject), true, GUILayout.MaxWidth(200));
            myScript.leftControllerModel = (GameObject)EditorGUILayout.ObjectField(myScript.leftControllerModel, typeof(GameObject), true, GUILayout.MaxWidth(200));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(200));
            GUILayout.Label("Right Models:");
            myScript.rightModel = (GameObject)EditorGUILayout.ObjectField(myScript.rightModel, typeof(GameObject), true, GUILayout.MaxWidth(200));
            myScript.rightControllerModel = (GameObject)EditorGUILayout.ObjectField(myScript.rightControllerModel, typeof(GameObject), true, GUILayout.MaxWidth(200));
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        myScript.hasTexts = EditorGUILayout.Toggle("Has Texts:", myScript.hasTexts);

        if (myScript.hasTexts)
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("texts"), true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif