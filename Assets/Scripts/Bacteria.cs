using System.Collections;
using UnityEngine;
using HTC.UnityPlugin.Vive;

public class Bacteria : MonoBehaviour
{
    public GameObject warp;

    public bool isRotateable;
    public int targetScene;

    public float speed = 1;

    private bool executed = false;
    public void MoveBacteria(Direction direction)
    {
        
        if(direction == Direction.Forward && !executed)
        {
            executed = true;
            warp.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 3;
            warp.transform.LookAt(Camera.main.transform);
            warp.SetActive(true);
            warp.GetComponent<WarpSpeed>().Engage();

            Debug.Log("Moving Bacteria");

            StartCoroutine(Timer());
        }
    }

    IEnumerator Timer()
    {
        float timer = 0;
        float delay = LocalSceneManager.instance.transitionSpeed;
        Vector3 initialPos = transform.position;
        Vector3 targetPos = initialPos + (Camera.main.transform.position - initialPos) * 0.3f;

        while (timer < delay)
        {
            timer += Time.deltaTime;

            transform.position = Vector3.Lerp(initialPos, targetPos, timer / delay);

            yield return null;
        }

        LocalSceneManager.instance.LoadNextScene(targetScene);

        yield return null;
    }

    public void StartRotating(HandRole hand)
    {
        StartCoroutine(RotateBacteria(hand));
    }

    IEnumerator RotateBacteria(HandRole hand)
    {
        Vector3 initialControllerPosition = VivePose.GetPose(hand).pos;
        Vector3 controllerOffset;

        while (ControllerManager.instance.GetIsPressed(hand))
        {
            controllerOffset = VivePose.GetPose(hand).pos - initialControllerPosition;

            Rotate(controllerOffset);

            yield return null;
        }
    }

    private void Rotate(Vector3 delta)
    {
        transform.Rotate(Vector3.up, -Vector3.Dot(delta, Camera.main.transform.right), Space.World);
        transform.Rotate(Camera.main.transform.right, Vector3.Dot(delta, Vector3.up), Space.World);
    }
}
