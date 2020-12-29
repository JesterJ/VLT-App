using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Stage3Manager : MonoBehaviour
{
    public static Stage3Manager instance;

    [SerializeField]
    private List<GameObject> moleculeStrings;
    [SerializeField]
    private float colorFactor = 2.25f;

    private List<MoleculeSet> moleculeSets;
    private int setIterator = 0;

    public HandScript leftHand;
    public HandScript rightHand;
    public GameObject cloudPrefab;

    public float twistMinThreshold, twistTargetAngle = 90, breakTime;

    public List<GameObject> texts;

    public bool showAnimations, showTexts;

    public MoleculeSet activeMoleculeSet
    {
        get { return _activeMoleculeSet; }
        private set 
        {
            if(activeMoleculeSet.moleculeString != null)
            {
                var tmp = activeMoleculeSet;
                _activeMoleculeSet = value;
                if (tmp.moleculeString != _activeMoleculeSet.moleculeString)
                {
                    HighlightMolecules(_activeMoleculeSet);
                    _activeMoleculeSet.moleculeString.transform.position -= _activeMoleculeSet.moleculeString.transform.up * 0.12f;
                    ShadeMolecules(tmp, colorFactor);
                    tmp.moleculeString.transform.position += tmp.moleculeString.transform.up * 0.12f;
                }
            }
            else
            {
                _activeMoleculeSet = value;
                HighlightMolecules(_activeMoleculeSet);
                _activeMoleculeSet.moleculeString.transform.position -= _activeMoleculeSet.moleculeString.transform.up * 0.12f;
            }
        }
    }
    public MoleculeSet _activeMoleculeSet;

    public struct MoleculeSet
    {
        public GameObject moleculeString;
        public MoleculeManager manager;
        public List<Transform> parts;

        public MoleculeSet(GameObject _moleculeString)
        {
            moleculeString = _moleculeString;

            parts = ListChildrenWithMeshRenderer(_moleculeString.transform, new List<Transform>());

            manager = _moleculeString.GetComponentInChildren<MoleculeManager>();

            List<Transform> ListChildrenWithMeshRenderer(Transform t, List<Transform> tList)
            {
                foreach (Transform _t in t)
                {
                    if (_t.GetComponent<MeshRenderer>() != null)
                    {
                        tList.Add(_t);
                    }
                    if (_t.childCount > 0)
                    {
                        tList = ListChildrenWithMeshRenderer(_t, tList);
                    }
                }

                return tList;
            }
        }
    }
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        moleculeSets = new List<MoleculeSet>();

        foreach (GameObject g in moleculeStrings)
        {
            moleculeSets.Add(new MoleculeSet(g));
        }

        foreach(MoleculeSet m in moleculeSets)
        {
            ShadeMolecules(m, 1.5f);
        }
    }

    private void Start()
    {
        activeMoleculeSet = moleculeSets[setIterator];
        StartCoroutine(activeMoleculeSet.manager.Active());
        ControllerManager.instance.showGuidingArrows = false;

        if (showTexts)
        {
            StartCoroutine(UIManager.instance.ShowText(texts.GetRange(0, 3)));
        }
    }

    public IEnumerator CompleteMoleculeSet()
    {
        DataRecorder.instance.SetEvent(LocalSceneManager.instance.currentStage.thisScene + "_" + activeMoleculeSet.moleculeString.name + "_broken");
        activeMoleculeSet.manager.isSolved = true;
        
        if (leftHand.currentAngle < 10 && rightHand.currentAngle > 10)
        {
            activeMoleculeSet.manager.StartReaction(MoleculeManager.MoleculeState.sn1);
            StartCoroutine(activeMoleculeSet.manager.nag.RotateMolecule());
        }
        else
        {
            activeMoleculeSet.manager.StartReaction(MoleculeManager.MoleculeState.sn2);
        }

        while (!activeMoleculeSet.manager.reactionFinished)
        {
            yield return null;
        }

        if (setIterator == 0 && showTexts)
        {
            StartCoroutine(UIManager.instance.ShowText(texts[3]));
        }
        else if (setIterator == 1 && showTexts)
        {
            StartCoroutine(UIManager.instance.ShowText(texts[4]));
        }

        yield return UIManager.instance.WaitUntilTextIsDone();

        if (setIterator < moleculeSets.Count - 1)
        {
            setIterator++;
            activeMoleculeSet = moleculeSets[setIterator];
            StartCoroutine(activeMoleculeSet.manager.Active());
        }
        else
        {
            ControllerManager.instance.showGuidingArrows = false;
            LocalSceneManager.instance.LoadNextScene();
        }

        yield return null;
    }

    private void HighlightMolecules(MoleculeSet mSet)
    {
        foreach(Transform t in mSet.parts)
        {
            //if (!t.name.Contains("Graph"))
            //{
                var color = t.GetComponent<Renderer>().material.color;
                t.GetComponent<Renderer>().material.color = new Color(color.r * colorFactor, color.g * colorFactor, color.b * colorFactor);
            //}
        }
    }

    private void ShadeMolecules(MoleculeSet mSet, float shadeFactor)
    {
        foreach (Transform t in mSet.parts)
        {
            //if (!t.name.Contains("Graph"))
            //{
                var color = t.GetComponent<Renderer>().material.color;
                t.GetComponent<Renderer>().material.color = new Color(color.r / shadeFactor, color.g / shadeFactor, color.b / shadeFactor);
            //}
        }
    }
}