using HTC.UnityPlugin.Vive;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Direction
{
    Forward,
    Backward
}

public class ControllerManager : MonoBehaviour
{
    public static ControllerManager instance;

    public ControllerButton triggerButton;

    [HideInInspector] public ViveRaycaster rightRay, leftRay;
    [HideInInspector] public GameObject rightGuideLine, leftGuideLine;
    public delegate void grab(HandRole device);
    public grab Grab;
    public delegate void drop(HandRole device);
    public drop Drop;

    [Range(0.01f, 0.3f)]
    public float vibrationInterval = 0.01f;

    private bool leftIsPressed = false, rightIsPressed = false, rightIsTargeting = false, leftIsTargeting;
    private List<GameObject> highlightetTargets = new List<GameObject>();

    [HideInInspector]
    public bool showGuidingArrows = false, castingRays = false;

    public GameObject rightTarget
    {
        get { return _rightTarget; }
        private set
        {
            if (value != rightTarget)
            {
                if (rightTarget != null && rightTarget.GetComponentInChildren<Renderer>() != null)
                {
                    DeHighlightTarget(rightTarget);

                    rightIsTargeting = false;
                }

                _rightTarget = value;


                if (rightTarget != null && rightTarget.GetComponentInChildren<Renderer>() != null)
                {
                    HighlightTarget(rightTarget);

                    rightIsTargeting = true;

                    if (showGuidingArrows) StartCoroutine(WhileTargeting(HandRole.RightHand, value));
                }
            }
        }
    }
    private GameObject _rightTarget;

    public GameObject leftTarget
    {
        get { return _leftTarget; }
        private set
        {
            if (value != leftTarget)
            {
                if (leftTarget != null && leftTarget.GetComponentInChildren<Renderer>() != null)
                {
                    DeHighlightTarget(leftTarget);

                    leftIsTargeting = false;
                }

                _leftTarget = value;

                if (leftTarget != null && leftTarget.GetComponentInChildren<Renderer>() != null)
                {
                    HighlightTarget(leftTarget);

                    leftIsTargeting = true;

                    if (showGuidingArrows) StartCoroutine(WhileTargeting(HandRole.LeftHand, value));
                }
            }
        }
    }
    private GameObject _leftTarget;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        OnSceneLoaded();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (DataRecorder.instance != null) DataRecorder.instance.StopRecording();

            Application.Quit();
        }
    }

    public void OnSceneLoaded()
    {
        ViveInput.AddPressDown(HandRole.RightHand, triggerButton, RightDownHandler);
        ViveInput.AddPressDown(HandRole.LeftHand, triggerButton, LeftDownHandler);
        ViveInput.AddPressUp(HandRole.RightHand, triggerButton, RightUpHandler);
        ViveInput.AddPressUp(HandRole.LeftHand, triggerButton, LeftUpHandler);
    }

    public void RightDownHandler() { if (!GetIsPressed(HandRole.RightHand)) { Grab?.Invoke(HandRole.RightHand); SetIsPressed(HandRole.RightHand, true); } }
    public void LeftDownHandler() { if (!GetIsPressed(HandRole.LeftHand)) { Grab?.Invoke(HandRole.LeftHand); SetIsPressed(HandRole.LeftHand, true); } }
    public void RightUpHandler() { if (GetIsPressed(HandRole.RightHand)) { Drop?.Invoke(HandRole.RightHand); SetIsPressed(HandRole.RightHand, false); } }
    public void LeftUpHandler() { if (GetIsPressed(HandRole.LeftHand)) { Drop?.Invoke(HandRole.LeftHand); SetIsPressed(HandRole.LeftHand, false); } }
    public void ReleaseObject(HandRole device) { SetIsPressed(device, false); }

    public void UpdateViveRayCasters()
    {
        GameObject vivePointers = GameObject.Find("VivePointers");
        if (vivePointers != null)
        {
            rightRay = vivePointers.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<ViveRaycaster>();
            leftRay = vivePointers.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<ViveRaycaster>();
            rightGuideLine = vivePointers.transform.GetChild(0).GetChild(2).gameObject;
            leftGuideLine = vivePointers.transform.GetChild(1).GetChild(2).gameObject;
        }
    }

    public bool GetIsPressed(HandRole device)
    {
        switch (device)
        {
            case HandRole.RightHand:
                return rightIsPressed;
            case HandRole.LeftHand:
                return leftIsPressed;
            default:
                Debug.LogWarning("Unknown Device");
                return false;
        }
    }
    public void SetIsPressed(HandRole device, bool state)
    {
        switch (device)
        {
            case HandRole.RightHand:
                rightIsPressed = state;
                break;
            case HandRole.LeftHand:
                leftIsPressed = state;
                break;
            default:
                Debug.LogWarning("Unknown Device");
                break;
        }
    }

    /*public void RegisterPullPush(Vector3 target, HandRole hand, Action<Direction> func)
    {
        StartCoroutine(PullPush(target, hand, func));
    }*/
    public void RegisterPullPush(HandRole hand, Action<Direction> func)
    {
        StartCoroutine(PullPush(hand, func));
    }
    /*private IEnumerator PullPush(Vector3 target, HandRole hand, Action<Direction> action)
    {
        yield return null;

        float threshold = 20;
        Debug.Log("Registering Pull and Push");

        Vector3 playerForward = target - Camera.main.transform.position;
        if (hand == HandRole.RightHand)
        {
            while (GetIsPressed(hand))
            {
                if (VivePose.GetAngularVelocity(hand).sqrMagnitude > 30)
                {
                    if (Vector3.Cross(playerForward, VivePose.GetAngularVelocity(hand)).x < threshold * -1 && Mathf.Abs(Vector3.Dot(playerForward, VivePose.GetAngularVelocity(hand))) < 30)
                    {
                        Debug.Log("Push");
                        action(Direction.Backward);
                        break;
                    }
                    else if (Vector3.Cross(playerForward, VivePose.GetAngularVelocity(hand)).x > threshold && Mathf.Abs(Vector3.Dot(playerForward, VivePose.GetAngularVelocity(hand))) < 30)
                    {
                        Debug.Log("Pull");
                        action(Direction.Forward);
                        break;
                    }
                }
                yield return null;
            }
        }
        else if (hand == HandRole.LeftHand)
        {
            while (GetIsPressed(hand))
            {
                if (VivePose.GetAngularVelocity(hand).sqrMagnitude > 30)
                {
                    if (Vector3.Cross(playerForward, VivePose.GetAngularVelocity(hand)).x < threshold * -1 && Mathf.Abs(Vector3.Dot(playerForward, VivePose.GetAngularVelocity(hand))) < 30)
                    {
                        Debug.Log("Pull");
                        action(Direction.Forward);
                        break;
                    }
                    else if (Vector3.Cross(playerForward, VivePose.GetAngularVelocity(hand)).x > threshold && Mathf.Abs(Vector3.Dot(playerForward, VivePose.GetAngularVelocity(hand))) < 30)
                    {
                        Debug.Log("Push");
                        action(Direction.Backward);
                        break;
                    }
                }
                yield return null;
            }
        }
    }*/
    private IEnumerator PullPush(HandRole hand, Action<Direction> action)
    {
        yield return null;

        Vector3 velocity;
        Transform camera = Camera.main.transform;
        while (GetIsPressed(hand))
        {
            velocity = VivePose.GetVelocity(hand);

            if (velocity.sqrMagnitude > 2)
            {
                if (velocity.y > -1 && velocity.y < 1)
                {
                    float dot = Vector3.Dot(Vector3.forward, camera.InverseTransformVector(velocity) - camera.InverseTransformVector(VivePose.GetPose(hand).pos));
                    float angle = Vector3.Angle(Vector3.forward, camera.InverseTransformVector(velocity) - camera.InverseTransformVector(VivePose.GetPose(hand).pos));

                    if (angle < 60 || angle > 120)
                    {
                        showGuidingArrows = false;

                        yield return null;

                        if (Mathf.Sign(dot) == 1)
                        {
                            DataRecorder.instance.SetEvent("Push");
                            action(Direction.Backward);

                            break;
                        }
                        else if (Mathf.Sign(dot) == -1)
                        {
                            DataRecorder.instance.SetEvent("Pull");
                            action(Direction.Forward);

                            break;
                        }
                    }
                }
            }

            yield return null;
        }
    }



    public void StartRaycasting()
    {
        StartCoroutine(CastRays());
    }
    public void StopRaycasting()
    {
        castingRays = false;

        rightTarget = null;
        leftTarget = null;
    }
    private IEnumerator CastRays()
    {
        if(rightGuideLine == null || leftGuideLine == null)
        {
            UpdateViveRayCasters();
        }

        yield return null;

        rightGuideLine.SetActive(true);
        leftGuideLine.SetActive(true);

        castingRays = true;

        while (castingRays)
        {
            if (rightRay != null || leftRay != null)
            {
                rightTarget = rightRay.FirstRaycastResult().gameObject;
                leftTarget = leftRay.FirstRaycastResult().gameObject;
            }
            else
            {
                UpdateViveRayCasters();
            }

            yield return null;
        }

        rightTarget = null;
        leftTarget = null;
        rightGuideLine.SetActive(false);
        leftGuideLine.SetActive(false);
    }

    public IEnumerator WhileTargeting(HandRole origin, GameObject target)
    {
        if (origin == HandRole.RightHand)
        {
            GameObject animation = UIManager.instance.PlayAnimation(AnimationType.ArrowPullPush);

            while (rightIsTargeting && showGuidingArrows)
            {
                UIManager.instance.SetArrowPosition(animation, Vector3.Lerp(VivePose.GetPose(HandRole.RightHand).pos, target.transform.position, 0.5f), VivePose.GetPose(HandRole.RightHand).pos);
                yield return null;
            }

            UIManager.instance.StopAnimation(animation);
        }
        else if (origin == HandRole.LeftHand)
        {
            GameObject animation = UIManager.instance.PlayAnimation(AnimationType.ArrowPullPush);

            while (leftIsTargeting && showGuidingArrows)
            {
                UIManager.instance.SetArrowPosition(animation, Vector3.Lerp(VivePose.GetPose(HandRole.LeftHand).pos, target.transform.position, 0.5f), VivePose.GetPose(HandRole.LeftHand).pos);
                yield return null;
            }

            UIManager.instance.StopAnimation(animation);
        }
    }

    public void HighlightTarget(GameObject target)
    {
        if (!highlightetTargets.Contains(target))
        {
            Renderer renderer = target.GetComponentInChildren<Renderer>();
            Color color = new Color(renderer.material.color.r * UIManager.instance.colorFactor, renderer.material.color.g * UIManager.instance.colorFactor, renderer.material.color.b * UIManager.instance.colorFactor);
            renderer.material.color = color;
        }

        highlightetTargets.Add(target);
    }
    public void DeHighlightTarget(GameObject target)
    {
        highlightetTargets.Remove(target);

        if (!highlightetTargets.Contains(target))
        {
            Renderer renderer = target.GetComponentInChildren<Renderer>();
            Color color = new Color(renderer.material.color.r / UIManager.instance.colorFactor, renderer.material.color.g / UIManager.instance.colorFactor, renderer.material.color.b / UIManager.instance.colorFactor);
            renderer.material.color = color;
        }
    }

    //void RenderLine1(Vector3[] vertexes)
    //{
    //    GameObject _object = GameObject.Find("line1");
    //    LineRenderer lineRenderer = _object.GetComponent<LineRenderer>();
    //    lineRenderer.enabled = true;
    //    lineRenderer.positionCount = vertexes.Length;
    //    lineRenderer.SetPositions(vertexes);
    //}
    //void RenderLine2(Vector3[] vertexes)
    //{
    //    GameObject _object = GameObject.Find("line2");
    //    LineRenderer lineRenderer = _object.GetComponent<LineRenderer>();
    //    lineRenderer.enabled = true;
    //    lineRenderer.positionCount = vertexes.Length;
    //    lineRenderer.SetPositions(vertexes);
    //}

}