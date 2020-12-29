using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HTC.UnityPlugin.Vive;

public class MoleculeManager : MonoBehaviour
{
    [HideInInspector]
    public bool reactionFinished = false, isSolved = false;

    public Molecule nag, nam;

    public Transform[] graphs = new Transform[4];

    private Transform oxygen, oBinding1, oBinding2;

    float lerpTime = 2;

    public enum MoleculeState { none, state1, state2, sn1, sn2 };

    public MoleculeState currentState
    {
        get { return _currentState; }
        set
        {
            if(value != currentState)
            {
                switch(value)
                {
                    case MoleculeState.none:
                        ActivateGraph(5);
                        break;
                    case MoleculeState.state1:
                        ActivateGraph(0);
                        break;
                    case MoleculeState.state2:
                        ActivateGraph(1);
                        break;
                    case MoleculeState.sn1:
                        ActivateGraph(2);
                        break;
                    case MoleculeState.sn2:
                        ActivateGraph(2);
                        break;
                    default:
                        break;
                }
            }

            _currentState = value;
        }
    }
    MoleculeState _currentState = MoleculeState.none;

    void Awake()
    {
        foreach(Transform t in this.transform)
        {
            switch (t.name)
            {
                case "Nag":
                    nag = t.GetComponent<Molecule>(); ;
                    break;
                case "Nam":
                    nam = t.GetComponent<Molecule>(); ;
                    break;
                case "Oxygen":
                    oxygen = t;
                    foreach(Transform p in t)
                    {
                        switch (p.name)
                        {
                            case "Binding O1":
                                oBinding1 = p;
                                break;
                            case "Binding O2":
                                oBinding2 = p;
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case "Graph 1":
                    graphs[0] = t;
                    graphs[0].gameObject.SetActive(false);
                    break;
                case "Graph 2":
                    graphs[1] = t;
                    graphs[1].gameObject.SetActive(false);
                    break;
                case "Graph 3":
                    graphs[2] = t;
                    graphs[2].gameObject.SetActive(false);
                    break;
                case "Graph 4":
                    graphs[3] = t;
                    graphs[3].gameObject.SetActive(false);
                    break;
                default:
                    break;
            }
        }

    }

    //Updates the state of interaction with the specific moleculestring while its in use
    public IEnumerator Active()
    {
        while (!isSolved)
        {
            currentState = GetState();
            yield return null;
        }
    }

    //Checks hand position and angles in order to return what state of the interaction the user is currently in
    //Holds the state if the button remains pressed as the user moves the hands away from the molecules
    public MoleculeState GetState()
    {
        if(Stage3Manager.instance.leftHand.IsCloseToMolecule() && Stage3Manager.instance.rightHand.IsCloseToMolecule())
        {
            if(ControllerManager.instance.GetIsPressed(HandRole.RightHand) && ControllerManager.instance.GetIsPressed(HandRole.LeftHand))
            {
                //Debug.Log(Stage3Manager.instance.rightHand.currentAngle);
                if (Stage3Manager.instance.leftHand.currentAngle < Stage3Manager.instance.twistMinThreshold && Stage3Manager.instance.rightHand.currentAngle > Stage3Manager.instance.twistMinThreshold)
                {
                    return MoleculeState.sn1;
                }
                else if (Stage3Manager.instance.leftHand.currentAngle > Stage3Manager.instance.twistMinThreshold && Stage3Manager.instance.rightHand.currentAngle > Stage3Manager.instance.twistMinThreshold)
                {
                    return MoleculeState.sn2;
                }

                return MoleculeState.state2;
            }

            return MoleculeState.state1;
        }
        else if(ControllerManager.instance.GetIsPressed(HandRole.RightHand) && ControllerManager.instance.GetIsPressed(HandRole.LeftHand))
        {
            if(currentState == MoleculeState.none)
                return MoleculeState.none;
            else
            {
                if (Stage3Manager.instance.leftHand.currentAngle < Stage3Manager.instance.twistMinThreshold && Stage3Manager.instance.rightHand.currentAngle > Stage3Manager.instance.twistMinThreshold)
                {
                    return MoleculeState.sn1;
                }
                else if (Stage3Manager.instance.leftHand.currentAngle > Stage3Manager.instance.twistMinThreshold && Stage3Manager.instance.rightHand.currentAngle > Stage3Manager.instance.twistMinThreshold)
                {
                    return MoleculeState.sn2;
                }

                return MoleculeState.state2;
            }
        }
        else
        {
            return MoleculeState.none;
        }
    }


    //Starts the reaction movements
    public void StartReaction(MoleculeState reaction)
    {
        DataRecorder.instance.SetEvent(reaction.ToString() + "_Initiated");
        StartCoroutine(Reaction(reaction));
    }

    IEnumerator Reaction(MoleculeState reaction)
    {
        ActivateGraph(3);

        if (reaction == MoleculeState.sn1) Stage3Manager.instance.leftHand.lineRenderer.enabled = false;

        Renderer gluORenderer = Stage3Manager.instance.rightHand._o.GetComponent<Renderer>();
        IEnumerator flashingOxygen = MoleculeController.instance.FlashColor(gluORenderer, new Color(gluORenderer.material.color.r, gluORenderer.material.color.g, gluORenderer.material.color.b, 0.5f), new Color(0, 0, 1, 0.5f), 0.5f);

        GameObject hydrogen1 = Instantiate(Stage3Manager.instance.cloudPrefab, Stage3Manager.instance.rightHand._o.position, Quaternion.identity);

        StartCoroutine(flashingOxygen);
        
        yield return StartCoroutine(MoleculeController.instance.MoveToPosition(hydrogen1.transform, Stage3Manager.instance.rightHand._o, oBinding2, 3));

        Destroy(hydrogen1);

        oBinding2.gameObject.SetActive(false);

        GameObject hydrogen2 = Instantiate(Stage3Manager.instance.cloudPrefab, nag.waterOxygen.position, Quaternion.identity);
        GameObject i = Instantiate(Stage3Manager.instance.cloudPrefab, nag.atoms[2].position, Quaternion.identity);
        IEnumerator unstableBinding = MoleculeController.instance.MoveBetween(i.transform, nag.atoms[2], nag.atoms[3], 0.5f);

        nag.waterOxygen.gameObject.SetActive(true);
        
        StartCoroutine(unstableBinding);

        yield return StartCoroutine(MoleculeController.instance.MoveToPosition(hydrogen2.transform, nag.waterOxygen.position, Stage3Manager.instance.rightHand._o.transform, 3));

        ViveInput.TriggerHapticVibration(HandRole.RightHand, 0.2f, 85, 0.125f);
        Destroy(hydrogen2);
        Destroy(i);
        StopCoroutine(flashingOxygen);
        StopCoroutine(unstableBinding);

        gluORenderer.material.color = Color.red;

        ActivateGraph(5);

        yield return new WaitForSeconds(2);

        reactionFinished = true;

        
    }

    public void ActivateGraph(int graph)
    {
        foreach (Transform t in graphs)
        {
            t.gameObject.SetActive(false);
        }

        if (graph < 4)
        {
            graphs[graph].gameObject.SetActive(true);
        }
    }
}



