using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using HTC.UnityPlugin.Vive;
using System;
using System.IO;

public class LocalSceneManager : MonoBehaviour
{
    [SerializeField]
    public List<Stage> _stages;

    public Dictionary<string, Stage> stages = new Dictionary<string, Stage>();

    public static LocalSceneManager instance;

    public float distance = 3;

    [HideInInspector]
    public bool stage0done = false, hubDone = false;

    [Range(1, 5)]
    public int transitionSpeed = 1;

    [HideInInspector]
    public Stage currentStage;

    private bool task1Done = false, task2Done = false, task3Done = false, task4Done = false;


    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);

        foreach(Stage s in _stages)
        {
            stages.Add(s.thisScene, s);
        }

        SceneManager.sceneLoaded += OnSceneLoad;
        currentStage = stages[SceneManager.GetActiveScene().name];
    }

    public void LoadPreviousScene()
    {
        SceneManager.LoadScene(instance.currentStage.previousScene, LoadSceneMode.Single);
    }
    public void LoadPreviousScene(Direction direction)
    {
        if(direction == Direction.Backward)
        {
            SceneManager.LoadScene(instance.currentStage.previousScene, LoadSceneMode.Single);
        }
    }


    public void LoadNextScene()
    {
        UIManager.instance.StopAllCoroutines();
        UIManager.instance.showingText = false;

        if (instance.currentStage.nextScenes[0] != string.Empty)
        {
            if(instance.preparedScene == null)
            {
                SceneManager.LoadScene(instance.currentStage.nextScenes[0], LoadSceneMode.Single);
            }
            else
            {
                instance.preparedScene.allowSceneActivation = true;
                instance.preparedScene = null;
            }
            
        }
    }
    public void LoadNextScene(int number)
    {
        UIManager.instance.StopAllCoroutines();
        UIManager.instance.showingText = false;

        number -= 1;
        if (instance.currentStage.nextScenes[number] != string.Empty)
        {
            SceneManager.LoadScene(instance.currentStage.nextScenes[number], LoadSceneMode.Single);
        }
    }

    private AsyncOperation preparedScene = null;

    public void PrepareNextScene()
    {
        if (instance.currentStage.nextScenes[0] != string.Empty && instance.preparedScene == null)
        {
            instance.preparedScene = SceneManager.LoadSceneAsync(instance.currentStage.nextScenes[0], LoadSceneMode.Single);
            instance.preparedScene.allowSceneActivation = false;
        }
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        currentStage = stages[SceneManager.GetActiveScene().name];

        if(DataRecorder.instance != null) DataRecorder.instance.SetEvent("Loaded_" + currentStage.thisScene);

        if (currentStage.thisScene.Contains("4.1")) task1Done = true;
        else if (currentStage.thisScene.Contains("4.2")) task2Done = true;
        else if (currentStage.thisScene.Contains("4.3")) task3Done = true;
        else if (currentStage.thisScene.Contains("4.4")) task4Done = true;

        if (currentStage.thisScene.Contains("Hub"))
        {
            if(task1Done && task2Done && task3Done && task4Done)
            {
                DataRecorder.instance.StopRecording();
                Application.Quit();
            }
        }

        ControllerManager.instance.UpdateViveRayCasters();
        ControllerManager.instance.OnSceneLoaded();
    }
}

[Serializable]
public class Stage
{
    [SerializeField]
    public string thisScene, previousScene;
    [SerializeField]
    public string[] nextScenes;
}

