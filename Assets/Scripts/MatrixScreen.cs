using UnityEngine;
using TMPro;

public class MatrixScreen : MonoBehaviour {

    [Header("Prefabs")]
    [SerializeField] UIMatrix uiMatrixPrefab;
    [SerializeField] UIMatrixGroup matrixGroupPrefab;

    [Header("Components")]
    [SerializeField] Camera matrixCam;
    [SerializeField] Camera externalCam;
    [SerializeField] MeshFilter referenceObject;
    [SerializeField] RectTransform uiMatrixParent;                  // TODO matrix groups (object, camera)
    [SerializeField] MatrixScreenPanAndZoom panAndZoomController;

    [Header("Settings")]
    [SerializeField] float multiplicationSignSize;

    public float matrixZoom => panAndZoomController.zoomLevel;

    UIMatrixGroup TESTGROUP;

    void Awake () {
        matrixCam.GetComponent<CustomGLCamera>().matrixScreen = this;
        externalCam.GetComponent<CustomGLCamera>().matrixScreen = this;

        // var newMatrix = Instantiate(uiMatrixPrefab).GetComponent<UIMatrix>();
        // newMatrix.rectTransform.SetParent(uiMatrixParent, false);
        // newMatrix.rectTransform.anchoredPosition = Vector2.zero;
        // newMatrix.Initialize(UIMatrices.MatrixConfig.identityConfig, UIMatrix.Editability.FULL, true);
        // newMatrix.matrixScreen = this;

        TESTGROUP = Instantiate(matrixGroupPrefab);
        TESTGROUP.rectTransform.SetParent(uiMatrixParent, false);
        TESTGROUP.rectTransform.SetAnchor(0.5f * Vector2.one);
        TESTGROUP.rectTransform.pivot = new Vector2(0f, 0.5f);
        TESTGROUP.rectTransform.anchoredPosition = Vector2.zero;
        TESTGROUP.Initialize(this);
    }

    public void AddMatrix (UIMatrix callingMatrix) {
        if(callingMatrix.matrixGroup.TryGetIndexOf(callingMatrix, out var index)){
            callingMatrix.matrixGroup.CreateMatrixAtIndex(UIMatrices.MatrixConfig.identityConfig, UIMatrix.Editability.FULL, index + 1);
        }else{
            Debug.Log("wat");
        }
    }

    public void DeleteMatrix (UIMatrix matrixToDelete) {
        matrixToDelete.matrixGroup.DeleteMatrix(matrixToDelete);
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
