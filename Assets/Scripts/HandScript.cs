using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HTC.UnityPlugin.Vive;
using HTC.UnityPlugin.Utility;

[RequireComponent(typeof(LineRenderer))]

public class HandScript : MonoBehaviour
{
    public HandRole hand;
    public float progress;

    public Transform _o;

    [SerializeField]
    private float disThreshold = 0.05f;
    
    [HideInInspector]
    public float currentAngle = 0;

    [SerializeField]
    private float angle = 90;

    public LineRenderer lineRenderer;

    private Transform rotationPoint;

    private void Awake()
    {

        Debug.Log("Initializing " + hand);
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.loop = false;

        for(int i = 0; i < transform.childCount; i++)
        {
            if(transform.GetChild(i).name == "RotationPoint")
            {
                rotationPoint = transform.GetChild(i);
            }
        }
    }

    private void OnEnable()
    {
        ControllerManager.instance.Grab += Grab;
    }

    private void OnDisable()
    {
        ControllerManager.instance.Grab -= Grab;
    }

    float tick = 0;
    
    private void Update()
    {        
        if (IsCloseToMolecule() && Stage3Manager.instance.activeMoleculeSet.manager.currentState == MoleculeManager.MoleculeState.none && !Stage3Manager.instance.activeMoleculeSet.manager.isSolved)
        {
            tick += Time.deltaTime;

            if(tick >= 0.8f)
            {
                ViveInput.TriggerHapticVibration(hand, 0.2f, 85f, 0.125f);

                tick = 0;
            }
        }
        else if(Stage3Manager.instance.activeMoleculeSet.manager.currentState == MoleculeManager.MoleculeState.state1 && !Stage3Manager.instance.activeMoleculeSet.manager.isSolved && !ControllerManager.instance.GetIsPressed(hand))
        {
            tick += Time.deltaTime;

            if (tick >= 0.4)
            {
                ViveInput.TriggerHapticVibration(hand, 0.2f, 85f, 0.125f);

                tick = 0;
            }
        }
    }
    
    public void Grab(HandRole _device)
    {
        
        if(_device == hand && IsCloseToMolecule())
        {
            DataRecorder.instance.SetEvent(_device.ToString() + "_Grab_" + Stage3Manager.instance.activeMoleculeSet.moleculeString.name);
            StartCoroutine(Grabbing());
        }
        else if(_device == hand)
        {
            ControllerManager.instance.RegisterPullPush(_device, LocalSceneManager.instance.LoadPreviousScene);
        }
    }

    IEnumerator Grabbing()
    {
        yield return null;

        if (hand == HandRole.LeftHand)
        {
            while (!ControllerManager.instance.GetIsPressed(HandRole.RightHand))
            {
                yield return null;
            }
        }
        else if(hand == HandRole.RightHand)
        {
            while (!ControllerManager.instance.GetIsPressed(HandRole.LeftHand))
            {
                yield return null;
            }
        }

        RigidPose vivePose = VivePose.GetPose(hand);
        Quaternion initialRotation = vivePose.rot;
        float timer = 0;
        float vibrationTimer = 0;
        if (hand == HandRole.RightHand) rotationPoint.transform.LookAt(Stage3Manager.instance.activeMoleculeSet.manager.nam.bindings[5]);
        else if (hand == HandRole.LeftHand) rotationPoint.transform.LookAt(Stage3Manager.instance.activeMoleculeSet.manager.nag.atoms[2]);
        GameObject animation = null;

        if (Stage3Manager.instance.showAnimations) animation = UIManager.instance.PlayAnimation(AnimationType.ArrowRotation, rotationPoint);

        while (ControllerManager.instance.GetIsPressed(hand))
        {
            vivePose = VivePose.GetPose(hand);

            currentAngle = Mathf.Abs(Mathf.DeltaAngle(initialRotation.eulerAngles.z, vivePose.rot.eulerAngles.z));

            //UIManager.instance.SetArrowPosition(animation, rotationPoint.position);
            

            progress = Mathf.Clamp((currentAngle - Stage3Manager.instance.twistMinThreshold) / (angle - Stage3Manager.instance.twistMinThreshold), 0, 1);

            timer += Time.deltaTime;

            //if (timer >= 0.31f) ViveInput.TriggerHapticVibration(hand, 0.3f, 40 + 60 * progress, 0.1f);

            switch (Stage3Manager.instance.activeMoleculeSet.manager.currentState)
            {
                case MoleculeManager.MoleculeState.sn1:
                    if(hand == HandRole.RightHand)
                    {
                        RenderLine(new Vector3[] { _o.position, Stage3Manager.instance.activeMoleculeSet.manager.nam.bindings[5].position });
                    }
                    else
                    {
                        lineRenderer.enabled = false;
                    }
                    break;
                case MoleculeManager.MoleculeState.sn2:

                    if(hand == HandRole.RightHand)
                    {
                        RenderLine(new Vector3[] { _o.position, Stage3Manager.instance.activeMoleculeSet.manager.nam.bindings[5].position });
                    }
                    else
                    {
                        RenderLine(new Vector3[] { _o.position, Stage3Manager.instance.activeMoleculeSet.manager.nag.atoms[3].position });
                    }
                    
                    break;
                case MoleculeManager.MoleculeState.state2:
                    if (hand == HandRole.RightHand)
                    {
                        RenderLine(new Vector3[] { _o.position, Stage3Manager.instance.activeMoleculeSet.manager.nam.bindings[5].position });
                    }
                    else
                    {
                        RenderLine(new Vector3[] { _o.position, Stage3Manager.instance.activeMoleculeSet.manager.nag.bindings[2].position });
                    }
                    break;
                default:
                    lineRenderer.enabled = false;
                    break;
            }

            if (currentAngle > Stage3Manager.instance.twistTargetAngle && hand == HandRole.RightHand)
            {
                vibrationTimer += Time.deltaTime;
                if (vibrationTimer >= 0.5f) 
                {
                    vibrationTimer = 0;
                    ViveInput.TriggerHapticVibration(hand, 0.5f, Mathf.Clamp(20 * Stage3Manager.instance.breakTime, 0, 85));
                }

                if(timer >= Stage3Manager.instance.breakTime && !(Stage3Manager.instance.breakTime >= 10))
                {
                    Stage3Manager.instance.activeMoleculeSet.manager.nag.thresholdReached = true;
                    StartCoroutine(Stage3Manager.instance.CompleteMoleculeSet());
                    ControllerManager.instance.SetIsPressed(HandRole.RightHand, false);
                    ControllerManager.instance.SetIsPressed(HandRole.LeftHand, false);
                }
            }

            yield return new WaitForEndOfFrame();
        }

        if (Stage3Manager.instance.showAnimations) UIManager.instance.StopAnimation(animation);

        currentAngle = 0;
        progress = 0;
        lineRenderer.enabled = false;

        yield return null;
    }

    void RenderLine(Vector3[] vertexes)
    {
        lineRenderer.enabled = true;
        lineRenderer.positionCount = vertexes.Length;
        lineRenderer.SetPositions(vertexes);
    }

    public bool IsCloseToMolecule()
    {
        bool _isClose = false;
        if(hand == HandRole.LeftHand)
        {
            if (Vector3.SqrMagnitude(Stage3Manager.instance.activeMoleculeSet.manager.nag.transform.position - _o.position) < disThreshold)
            {
                _isClose = true;
            }
        }
        else
        {
            if (Vector3.SqrMagnitude(Stage3Manager.instance.activeMoleculeSet.manager.nam.transform.position - _o.position) < disThreshold)
            {
                _isClose = true;
            }
        }

        return _isClose;
    }
}
