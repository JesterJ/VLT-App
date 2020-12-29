using UnityEngine;
using HTC.UnityPlugin.Vive;

public class CalibrationManager : MonoBehaviour
{
    public PupilLabs.CalibrationController calibrationController;

    void OnEnable()
    {
        calibrationController.OnCalibrationSucceeded += LocalSceneManager.instance.LoadNextScene;
        calibrationController.OnCalibrationFailed += FailedCalibrationHandler;

        ControllerManager.instance.Grab += OnGrab;
    }

    void OnDisable()
    {
        calibrationController.OnCalibrationSucceeded -= LocalSceneManager.instance.LoadNextScene;
        calibrationController.OnCalibrationFailed -= FailedCalibrationHandler;
        ControllerManager.instance.Grab -= OnGrab;
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Return))
        {
            LocalSceneManager.instance.LoadNextScene();
        }
    }

    void OnGrab(HandRole hand)
    {
        calibrationController.ToggleCalibration();
        ControllerManager.instance.Grab -= OnGrab;
    }

    void FailedCalibrationHandler()
    {
        ControllerManager.instance.Grab += OnGrab;
    }
}