using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using HTC.UnityPlugin.Vive;

public enum AnimationType
{
    None,
    ArrowPullPush,
    ArrowRotation,
    ArrowUp
}

public class UIManager : MonoBehaviour
{

    public static UIManager instance;

    [SerializeField]
    private float textFadeTime = 1.5f, textDistance = 0.1f;

    [SerializeField]
    public List<UIAnimation> animations;

    Dictionary<AnimationType, GameObject> _animations = new Dictionary<AnimationType, GameObject>();

    [Range(0,0.5f)]
    public float upArrowDistance;

    public float colorFactor = 1.5f;

    private bool skip = false;

    [HideInInspector]
    public bool showingText = false;
    
    private void Awake()
    {
        foreach (UIAnimation i in animations)
        {
            _animations.Add(i.type, i._animation);
        }
    }

    private void Start()
    {
        if (instance == null) instance = this;
    }

    public void Skip()
    {
        instance.skip = true;
        DataRecorder.instance.SetEvent("Next Pressed");
    }

    public IEnumerator ShowText(GameObject text)
    {
        skip = true;
        CanvasPositioner cp = text.GetComponentInChildren<CanvasPositioner>();

        while (showingText)
        {
            yield return null;
        }

        text.SetActive(true);

        skip = false;
        showingText = true;

        float timer = 0;

        while (timer < cp.readTime && !skip)
        {
            timer += Time.deltaTime;

            yield return null;
        }

        skip = false;

        yield return FadeText(text, true);

        showingText = false;
    }

    public IEnumerator ShowText(List<GameObject> texts) 
    {
        skip = true;

        for(int g = 1; g < texts.Count; g++)
        {
            foreach(Image i in texts[g].GetComponentsInChildren<Image>())
            {
                AdjustAlpha(0, i, 1);
            }
            foreach(Text t in texts[g].GetComponentsInChildren<Text>())
            {
                AdjustAlpha(0, t, 1);
            }
        }

        while (showingText)
        {
            yield return null;
        }
        
        skip = false;
        showingText = true;

        texts[0].SetActive(true);

        for(int i = 0; i < texts.Count; i++)
        {
            CanvasPositioner cp = texts[i].GetComponentInChildren<CanvasPositioner>();
            float timer = 0;
            while (timer < cp.readTime && !skip)
            {
                timer += Time.deltaTime;

                yield return null;
            }
            skip = false;

            yield return FadeText(texts[i], true);

            if(i < texts.Count - 1)
            {
                texts[i + 1].SetActive(true);
                yield return FadeText(texts[i + 1], false);
            }
        }

        showingText = false;
    }

    private void AdjustAlpha(float gradient, Image image, float time)
    {
        Color newColor = new Color(image.color.r, image.color.g, image.color.b, Mathf.Lerp(image.color.a, gradient, time));
        image.color = newColor;
    }
    private void AdjustAlpha(float gradient, Text text, float time)
    {
        Color newColor = new Color(text.color.r, text.color.g, text.color.b, Mathf.Lerp(text.color.a, gradient, time));
        text.color = newColor;
    }

    private IEnumerator FadeText(GameObject textObj, bool fadeOut)
    {
        float timer = 0;
        float gradient;

        Text[] texts = textObj.GetComponentsInChildren<Text>();
        Image[] images = textObj.GetComponentsInChildren<Image>();
        
        if(fadeOut) gradient = 0;
        else gradient = 1;

        while(timer < 1)
        {
            timer += Time.deltaTime / textFadeTime;

            foreach(Text t in texts)
            {
                AdjustAlpha(gradient, t, timer);
            }
            foreach(Image i in images)
            {
                AdjustAlpha(gradient, i, timer);
            }

            yield return null;
        }

        if(fadeOut) textObj.SetActive(false);
    }

    public IEnumerator WaitUntilTextIsDone()
    {
        yield return null;

        while (showingText) yield return null;
    }

    //Set position and rotation of a canvas at the standard distance set from inspector
    public void SetCanvasPosition(GameObject canvas)
    {
        Vector3 UIPos = Camera.main.transform.position + new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * textDistance;

        canvas.transform.position = UIPos;
        canvas.transform.LookAt(Camera.main.transform.position, Vector3.up);
        canvas.transform.rotation = Quaternion.Euler(0, canvas.transform.rotation.eulerAngles.y, 0);
    }
    public void SetCanvasPosition(Transform canvas)
    {
        Vector3 UIPos = Camera.main.transform.position + new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * textDistance;

        canvas.position = UIPos;
        canvas.LookAt(Camera.main.transform.position, Vector3.up);
        canvas.transform.rotation = Quaternion.Euler(0, canvas.transform.rotation.eulerAngles.y, 0);
    }

    //Set position and rotation of a canvas at a given distance passed as a parameter
    public void SetCanvasPosition(GameObject canvas, float distance)
    {
        Vector3 UIPos = Camera.main.transform.position + new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * distance;

        canvas.transform.position = UIPos;
        canvas.transform.LookAt(Camera.main.transform.position, Vector3.up);
        canvas.transform.rotation = Quaternion.Euler(0, canvas.transform.rotation.eulerAngles.y, 0);
    }
    public void SetCanvasPosition(Transform canvas, float distance)
    {
        Vector3 UIPos = Camera.main.transform.position + new Vector3(Camera.main.transform.forward.x, 0, Camera.main.transform.forward.z) * distance;

        canvas.position = UIPos;
        canvas.LookAt(Camera.main.transform.position, Vector3.up);
        canvas.transform.rotation = Quaternion.Euler(0, canvas.transform.rotation.eulerAngles.y, 0);
    }


    //Methods for instantiating and adjusting position of the animations
    public GameObject PlayAnimation(AnimationType animation)
    {
        GameObject tmp = Instantiate(_animations[animation]);

        return tmp;
    }
    public GameObject PlayAnimation(AnimationType animation, Vector3 pos, Quaternion rot)
    {
        GameObject tmp = Instantiate(_animations[animation], pos, rot);

        return tmp;
    }
    public GameObject PlayAnimation(AnimationType animation, Transform parent, Vector3 pos, Quaternion rot)
    {
        GameObject tmp = Instantiate(_animations[animation], pos, rot, parent);

        return tmp;
    }
    public GameObject PlayAnimation(AnimationType animation, Transform parent)
    {
        GameObject tmp = Instantiate(_animations[animation], parent);

        return tmp;
    }

    public void StopAnimation(GameObject animation)
    {
        Destroy(animation);
    }

    public void SetArrowPosition(GameObject arrow,Vector3 position, Vector3 target)
    {
        arrow.transform.position = position;
        arrow.transform.LookAt(target);
    }
    public void SetArrowPosition(GameObject arrow, Vector3 position)
    {
        arrow.transform.position = position;
    }
    public void SetArrowUpPosition(GameObject arrow, HandRole hand)
    {
        arrow.transform.position = VivePose.GetPose(hand).pos + Vector3.up * upArrowDistance;
    }
}

[Serializable]
public class UIAnimation
{
    public GameObject _animation;
    public AnimationType type;
}