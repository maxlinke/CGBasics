using UnityEngine;
using TMPro;

public class MatrixScreen : MonoBehaviour {

    [Header("Prefabs")]
    [SerializeField] UIMatrix uiMatrixPrefab;

    [Header("Components")]
    [SerializeField] Camera matrixCam;
    [SerializeField] Camera externalCam;
    [SerializeField] MeshFilter referenceObject;
    [SerializeField] RectTransform uiMatrixParent;                  // TODO matrix groups (object, camera)
    [SerializeField] MatrixScreenPanAndZoom panAndZoomController;

    public float matrixZoom => panAndZoomController.zoomLevel;

    void Awake () {
        matrixCam.GetComponent<CustomGLCamera>().matrixScreen = this;
        externalCam.GetComponent<CustomGLCamera>().matrixScreen = this;

        var newMatrix = Instantiate(uiMatrixPrefab).GetComponent<UIMatrix>();
        newMatrix.rectTransform.SetParent(uiMatrixParent, false);
        newMatrix.rectTransform.anchoredPosition = Vector2.zero;
        newMatrix.Initialize(UIMatrices.MatrixConfig.identityConfig, UIMatrix.Editability.FULL, true);
        newMatrix.matrixScreen = this;
    }

    public void AddMatrix (UIMatrix callingMatrix) {

    }

    public void DeleteMatrix (UIMatrix matrixToDelete) {

    }

    public void MoveMatrixLeft (UIMatrix matrixToMove) {

    }

    public void MoveMatrixRight (UIMatrix matrixToMove) {

    }

    // TODO remember to transpose every matrix and transpose the final product
    public void GetCurrentMeshAndWeightedMatrices (out Mesh outputMesh, out Matrix4x4 outputModelMatrix, out Matrix4x4 outputCameraMatrix) {
        outputMesh = referenceObject.sharedMesh;
        var refT = referenceObject.transform;
        outputModelMatrix = GLMatrixCreator.GetModelMatrix(refT.position, refT.eulerAngles, refT.lossyScale);
        outputCameraMatrix = Matrix4x4.identity;    // TODO this
    }
	
}
