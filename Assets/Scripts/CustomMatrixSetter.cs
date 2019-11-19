using UnityEngine;

[ExecuteAlways]
public class CustomMatrixSetter : MonoBehaviour {

    MeshRenderer mr;
    MaterialPropertyBlock mpb;
    int mvpMatrixID;
    int modelMatrixID;
    int normalMatrixID;
    int inverseModelMatrixID;

    void Awake () {
        mr = GetComponent<MeshRenderer>();
        if(mr == null){
            Debug.Log($"No MeshRenderer on \"{gameObject.name}\"!");
            return;
        }
        mpb = new MaterialPropertyBlock();
        mvpMatrixID = Shader.PropertyToID("CustomMVPMatrix");
        modelMatrixID = Shader.PropertyToID("CustomModelMatrix");
        normalMatrixID = Shader.PropertyToID("CustomNormalMatrix");
        inverseModelMatrixID = Shader.PropertyToID("CustomInverseModelMatrix");
    }

    void OnWillRenderObject () {
        if(mr == null){
            Debug.LogError($"Nullref!!! No MeshRenderer on ${this.gameObject.name}!");
            return;
        }
        if(mpb == null){
            mpb = new MaterialPropertyBlock();
        }

        var cam = Camera.current;
        var customCam = cam.gameObject.GetComponent<CustomCamera>();
        Matrix4x4 mvpMatrix, modelMatrix;
        if(customCam != null){
            customCam.GetMatrices(this.gameObject, out modelMatrix, out mvpMatrix);
        }else{
            var translationMatrix = Matrix4x4.Translate(transform.position);
            var rotationMatrix = Matrix4x4.Rotate(transform.rotation);
            var scaleMatrix = Matrix4x4.Scale(transform.lossyScale);
            modelMatrix = translationMatrix * rotationMatrix * scaleMatrix;
            var viewMatrix = cam.worldToCameraMatrix;
            var projectionMatrix = GL.GetGPUProjectionMatrix(cam.projectionMatrix, true);
            mvpMatrix = projectionMatrix * viewMatrix * modelMatrix;
        }

        var inverseModelMatrix = modelMatrix.inverse;

        mpb.SetMatrix(mvpMatrixID, mvpMatrix);
        mpb.SetMatrix(modelMatrixID, modelMatrix);
        mpb.SetMatrix(inverseModelMatrixID, inverseModelMatrix);
        mr.SetPropertyBlock(mpb);
    }
	
}
