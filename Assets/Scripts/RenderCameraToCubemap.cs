using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderCameraToCubemap : MonoBehaviour
{
    public RenderTexture rt;
    bool called = false;
    void LateUpdate()
    {

        if (!called)
        {
            GetComponent<Camera>().RenderToCubemap(rt);
            called = true;
        }
            
    }
}
