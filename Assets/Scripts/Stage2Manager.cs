using System.Collections;
using HTC.UnityPlugin.Vive;
using UnityEngine;

public class Stage2Manager : MonoBehaviour
{
    private bool executed = false;

    public GameObject text;

    IEnumerator Start()
    {
        
        ControllerManager.instance.Grab += OnButtonDown;

        if(text != null) StartCoroutine(UIManager.instance.ShowText(text));

        yield return null;

        ControllerManager.instance.showGuidingArrows = true;
        LocalSceneManager.instance.PrepareNextScene();


    }

    private void OnDisable()
    {
        ControllerManager.instance.Grab -= OnButtonDown;
    }

    private void OnButtonDown(HandRole device)
    {
        ControllerManager.instance.RegisterPullPush(device, ChangeScene);
    }

    public void ChangeScene(Direction direction)
    {
        if(direction == Direction.Forward && !executed)
        {
            executed = true;
            LocalSceneManager.instance.LoadNextScene();
        }
        else if(direction == Direction.Backward && !executed)
        {
            executed = true;
            LocalSceneManager.instance.LoadPreviousScene();
        }
    }
}
