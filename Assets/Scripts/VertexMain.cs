using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VertexMain : MonoBehaviour {

    [SerializeField] Camera matrixCam;
    [SerializeField] Camera externalCam;
    [SerializeField] MeshFilter referenceObject;

    [SerializeField] TextMeshProUGUI tempTextField;

    void Awake () {
        matrixCam.GetComponent<CustomGLCamera>().vertexScreen = this;
        externalCam.GetComponent<CustomGLCamera>().vertexScreen = this;
    }

    void Start () {
        
    }

    void Update () {

    }

    void LateUpdate () {

    }

    public void GetCurrentMeshAndModelMatrix (out Mesh outputMesh, out Matrix4x4 outputModelMatrix) {
        outputMesh = referenceObject.sharedMesh;
        // outputModelMatrix = GLMatrixCreator.GetTranslationMatrix(referenceObject.transform.position);
        var refT = referenceObject.transform;
        outputModelMatrix = GLMatrixCreator.GetModelMatrix(refT.position, refT.eulerAngles, refT.lossyScale);
    }
	
}
