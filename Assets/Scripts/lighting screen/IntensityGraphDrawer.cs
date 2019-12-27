using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntensityGraphDrawer : MonoBehaviour {

    [SerializeField] Camera targetCam;
    [SerializeField] Material blitMatTemplate;
    [SerializeField] float graphScale;
    [SerializeField] float lightAngle;
    [SerializeField] float viewAngle;
    [SerializeField] float roughness;

    Material blitMat;

    void Start () {
        targetCam.gameObject.AddComponent<IntensityGraphCam>().onRenderImage += BlitGraphEffect;
        blitMat = Instantiate(blitMatTemplate);
        blitMat.hideFlags = HideFlags.HideAndDontSave;
    }

    void Update () {
        blitMat.SetFloat("_GraphScale", graphScale);
        blitMat.SetVector("_LightDir", new Vector4(Mathf.Sin(lightAngle), Mathf.Cos(lightAngle), 0, 0));
        blitMat.SetVector("_ViewDir", new Vector4(Mathf.Sin(viewAngle), Mathf.Cos(viewAngle), 0, 0));
        blitMat.SetFloat("_Roughness", roughness);
    }

    void BlitGraphEffect (RenderTexture src, RenderTexture dst) {
        Graphics.Blit(src, dst, blitMat);
    }

    class IntensityGraphCam : MonoBehaviour {

        public event System.Action<RenderTexture, RenderTexture> onRenderImage = delegate {};

        void OnRenderImage (RenderTexture src, RenderTexture dst) {
            onRenderImage.Invoke(src, dst);
        }

    }
	
}
