using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntensityGraphDrawer : MonoBehaviour {

    [SerializeField] Camera targetCam;
    [SerializeField] Material blitMat;

    void Start () {
        targetCam.gameObject.AddComponent<IntensityGraphCam>().onRenderImage += BlitGraphEffect;
    }

    void Update () {
        
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
