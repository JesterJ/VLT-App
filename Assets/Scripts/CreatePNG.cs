using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePNG : MonoBehaviour
{
    private RenderTexture left, right, equirect;

    private void Start()
    {
        StartCoroutine(MakeMap());
    }

    IEnumerator MakeMap()
    {
        yield return new WaitForEndOfFrame();

        left = new RenderTexture( 4096, 4096, 24, RenderTextureFormat.ARGB32);
        left.dimension = UnityEngine.Rendering.TextureDimension.Cube;


        right = new RenderTexture( 4096, 4096, 24, RenderTextureFormat.ARGB32);
        right.dimension = UnityEngine.Rendering.TextureDimension.Cube;


        equirect = new RenderTexture( 4096, 4096, 24, RenderTextureFormat.ARGB32);
        equirect.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;

        GetComponent<Camera>().RenderToCubemap(left, 63, Camera.MonoOrStereoscopicEye.Left);
        GetComponent<Camera>().RenderToCubemap(right, 63, Camera.MonoOrStereoscopicEye.Right);

        left.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Left);
        right.ConvertToEquirect(equirect, Camera.MonoOrStereoscopicEye.Right);

        Debug.Log(Application.persistentDataPath);

        MakeSquarePng(equirect);

        yield return null;
    }

    public void MakeSquarePng(RenderTexture rt)
    {
        RenderTexture.active = rt;
        Texture2D virtualPhoto = new Texture2D(4096, 4096);
        // false, meaning no need for mipmaps
        virtualPhoto.ReadPixels(new Rect(0, 0, 4096, 4096), 0, 0);

        RenderTexture.active = null; //can help avoid errors 
        Debug.Log("Creates PNG");

        byte[] bytes;
        bytes = virtualPhoto.EncodeToPNG();
        
        System.IO.File.WriteAllBytes(OurTempSquareImageLocation(), bytes);
        // virtualCam.SetActive(false); ... no great need for this.
    }

    private string OurTempSquareImageLocation()
    {
        string r = Application.persistentDataPath + "/p.png" ;
        Debug.Log("Saving to " + r);
        return r;
    }
}
