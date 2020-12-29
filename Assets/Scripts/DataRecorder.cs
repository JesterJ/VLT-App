using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PupilLabs;
using UnityEngine.UI;
using System;
using HTC.UnityPlugin.Vive;
using System.Linq;

public class DataRecorder : MonoBehaviour
{
    public static DataRecorder instance;

    public RecordingController recorder;
    public TimeSync timeSync;

    [Header("Annotations")]
    public AnnotationPublisher annotationPub;
    private PupilListener pupilListener;
    private GazeListener gazeListener;
    private SubscriptionsController subscriptionsController;

    BaseListener baseListener;
    private GazeData currentGazeData;

    private int aoiCount = 0;
    private float pupilDiameter = 0;

    Dictionary<string, int> aoiVisits = new Dictionary<string, int>();
    Dictionary<string, double> aoiTimespent = new Dictionary<string, double>();
    Dictionary<HandRole, int> controllerHitCount = new Dictionary<HandRole, int>();
    Dictionary<string, int> leftControllerAOICount = new Dictionary<string, int>();
    Dictionary<string, int> rightControllerAOICount = new Dictionary<string, int>();

    private List<string> AOINames;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        subscriptionsController = FindObjectOfType<SubscriptionsController>();

        if (pupilListener == null)
        {
            pupilListener = new PupilListener(subscriptionsController);
        }
        if(gazeListener == null)
        {
            gazeListener = new GazeListener(subscriptionsController);
        }

        pupilListener.Enable();
        gazeListener.Enable();

        pupilListener.OnReceivePupilData += ReceiveData;
        gazeListener.OnReceive3dGaze += ReceiveGazeData;
        ControllerManager.instance.Grab += ReceiveControllerData;

        recorder.StartRecording();
        recording = true;
    }

    public void StopRecording()
    {
        recording = false;
        recorder.StopRecording();
    }

    private void OnDisable()
    {
        if(instance == this)
        {
            recording = false;
            recorder.StopRecording();
        }
    }

    bool recording = false;

    void Update()
    {
        if (recorder.requestCtrl != null && recording)
        {
            if (Time.frameCount % 10 == 0)
            {
                SendAnnotations(); //limit annotation rate
            }
        }
    }

    void SendAnnotations()
    {
        Dictionary<string, object> data = new Dictionary<string, object>();

        data["head_Position_x"] = Camera.main.transform.position.x;
        data["head_Position_y"] = Camera.main.transform.position.y;
        data["head_Position_z"] = Camera.main.transform.position.z;
        data["head_rotation_x"] = Camera.main.transform.rotation.eulerAngles.x;
        data["head_rotation_y"] = Camera.main.transform.rotation.eulerAngles.y;
        data["head_rotation_z"] = Camera.main.transform.rotation.eulerAngles.z;
        data["left_controller_Position_x"] = VivePose.GetPose(HandRole.LeftHand).pos.x;
        data["left_controller_Position_y"] = VivePose.GetPose(HandRole.LeftHand).pos.y;
        data["left_controller_Position_z"] = VivePose.GetPose(HandRole.LeftHand).pos.z;
        data["right_controller_Position_x"] = VivePose.GetPose(HandRole.RightHand).pos.x;
        data["right_controller_Position_y"] = VivePose.GetPose(HandRole.RightHand).pos.y;
        data["right_controller_Position_z"] = VivePose.GetPose(HandRole.RightHand).pos.z;
        data["left_controller_rotation_x"] = VivePose.GetPose(HandRole.LeftHand).rot.eulerAngles.x;
        data["left_controller_rotation_y"] = VivePose.GetPose(HandRole.LeftHand).rot.eulerAngles.y;
        data["left_Controller_rotation_z"] = VivePose.GetPose(HandRole.LeftHand).rot.eulerAngles.z;
        data["right_controller_rotation_x"] = VivePose.GetPose(HandRole.RightHand).rot.eulerAngles.x;
        data["right_controller_rotation_y"] = VivePose.GetPose(HandRole.RightHand).rot.eulerAngles.y;
        data["right_controller_rotation_z"] = VivePose.GetPose(HandRole.RightHand).rot.eulerAngles.z;
        data["left_controller_pressed"] = ControllerManager.instance.GetIsPressed(HandRole.RightHand);
        data["right_controller_pressed"] = ControllerManager.instance.GetIsPressed(HandRole.LeftHand);
        data["AOI_left_controller"] = GetControllerAOI(HandRole.LeftHand);
        data["AOI_right_controller"] = GetControllerAOI(HandRole.RightHand);

        if (GetAOI() != null) data["AOI_name"] = LocalSceneManager.instance.currentStage.thisScene + "_" + GetAOI().name;
        else data["AOI_name"] = "";
        data["Event_name"] = GetEvent();

        annotationPub.SendAnnotation(label: "Custom Data", customData: data);
    }

    Queue<string> interactionEvents = new Queue<string>();

    public void SetEvent(string _event)
    {
        interactionEvents.Enqueue(_event);
    }

    string GetEvent()
    {
        if (interactionEvents.Count == 0) return "";
        else return interactionEvents.Dequeue();
    }

    void ReceiveData(PupilData data)
    {
        pupilDiameter = data.Diameter3d;
        
    }

    private void ReceiveGazeData(GazeData data)
    {
        currentGazeData = data;
        
    }

    private string GetControllerAOI(HandRole hand)
    {
        switch (hand)
        {
            case HandRole.LeftHand:
                if (ControllerManager.instance.leftTarget == null) return "";
                else return ControllerManager.instance.leftTarget.name;
            case HandRole.RightHand:
                if (ControllerManager.instance.rightTarget == null) return "";
                else return ControllerManager.instance.rightTarget.name;
        }

        return "Not a controller";
    }
    private void ReceiveControllerData(HandRole hand)
    {
        string targetName;
        if (currentTarget == null) targetName = "null";
        else targetName = currentTarget.name;

        if (controllerHitCount.ContainsKey(hand))
        {
            controllerHitCount[hand] += 1;

            if(hand == HandRole.LeftHand)
            {
                leftControllerAOICount[targetName] += 1;
            }
            else
            {
                rightControllerAOICount[targetName] += 1;
            }
        }
        else
        {
            controllerHitCount.Add(hand, 1);

            if (hand == HandRole.LeftHand)
            {
                leftControllerAOICount.Add(targetName, 1);
            }
            else
            {
                rightControllerAOICount.Add(targetName, 1);
            }
        }
    }

    //public void AddAOIControllerCount(HandRole hand)
    //{
    //    string name = null;

    //    if (currentTarget == null) name = "null";
    //    else name = currentTarget.name;

    //    if (hand == HandRole.LeftHand)
    //    {
    //        if (leftControllerAOICount.ContainsKey(name))
    //        {
    //            leftControllerAOICount[name] += 1;
    //        }
    //        else
    //        {
    //            leftControllerAOICount.Add(name, 1);
    //        }
    //    }
    //    else
    //    {
    //        if (rightControllerAOICount.ContainsKey(name))
    //        {
    //            rightControllerAOICount[name] += 1;
    //        }
    //        else
    //        {
    //            rightControllerAOICount.Add(name, 1);
    //        }
    //    }
    //}

    private GameObject GetAOI()
    {

        if(currentGazeData != null)
        {
            RaycastHit hit;

            Physics.Raycast(Camera.main.transform.position, currentGazeData.GazeDirection, out hit);

            //RecordAOIData(currentGazeData, hit);

            return hit.transform.gameObject;
        }
        else
        {
            return null;
        }
    }

    GameObject currentTarget = null;

    //double timespent = 0;
    //double timeAtVisit = 0;

    //private void RecordAOIData(GazeData data, RaycastHit hit)
    //{
    //    string targetName;
    //    string hitName;

    //    if (currentTarget != null) targetName = currentTarget.name;
    //    else targetName = "null";

    //    if (hit.transform != null) hitName = hit.transform.gameObject.name;
    //    else hitName = "null";

    //    if (currentTarget != hit.transform.gameObject)
    //    {
    //        if (aoiVisits.ContainsKey(hitName))
    //        {
    //            aoiVisits[hitName] += 1;
    //        }
    //        else
    //        {
    //            aoiVisits.Add(hitName, 1);
    //        }
            
    //        if (aoiTimespent.ContainsKey(targetName))
    //        {
    //            aoiTimespent[targetName] += timespent;
    //        }
    //        else
    //        {
    //            aoiTimespent.Add(targetName, timespent);
    //        }

    //        timeAtVisit = timeSync.ConvertToUnityTime(data.PupilTimestamp);
    //        timespent = 0;
    //    }
    //    else
    //    {
    //        timespent = timeSync.ConvertToUnityTime(data.PupilTimestamp) - timeAtVisit;
    //    }

    //    currentTarget = hit.transform.gameObject;
    //}
}
