using UnityEngine;
using TMPro;

public class MatrixScreen : MonoBehaviour {

    [SerializeField] Camera matrixCam;
    [SerializeField] Camera externalCam;
    [SerializeField] MeshFilter referenceObject;

    void Awake () {
        matrixCam.GetComponent<CustomGLCamera>().matrixScreen = this;
        externalCam.GetComponent<CustomGLCamera>().matrixScreen = this;
    }

    void Start () {
        
    }

    void Update () {

    }

    void LateUpdate () {

    }

    // TODO remember to transpose every matrix and transpose the final product
    public void GetCurrentMeshAndWeightedMatrices (out Mesh outputMesh, out Matrix4x4 outputModelMatrix, out Matrix4x4 outputCameraMatrix) {
        outputMesh = referenceObject.sharedMesh;
        var refT = referenceObject.transform;
        outputModelMatrix = GLMatrixCreator.GetModelMatrix(refT.position, refT.eulerAngles, refT.lossyScale);
        outputCameraMatrix = Matrix4x4.identity;    // TODO this
    }
	
}
