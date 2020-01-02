using System.Collections;
using UnityEngine;

public class MeshDebug : MonoBehaviour {

    [SerializeField] Mesh initDebugMesh;
    [SerializeField] Shader testShader;


    IEnumerator Start () {
        yield return null;
        if(initDebugMesh != null){
            var clone = initDebugMesh.CreateClone(false, true);
            var mat = new Material(testShader);
            mat.hideFlags = HideFlags.HideAndDontSave;
            var mf = gameObject.AddComponent<MeshFilter>();
            mf.sharedMesh = clone;
            var mr = gameObject.AddComponent<MeshRenderer>();
            mr.material = mat;
            var cam = new GameObject("GL Cam", typeof(InternalGLCam)).GetComponent<InternalGLCam>();
            cam.matchCam = Camera.main;
            cam.targetMF = mf;
            cam.drawMat = mat;
        }
    }

    class InternalGLCam : MonoBehaviour {

        public Camera matchCam;
        public MeshFilter targetMF;
        public Material drawMat;

        Camera ownCam;

        void OnEnable () {
            ownCam = gameObject.AddComponent<Camera>();
            ownCam.rect = new Rect(new Vector2(0.5f, 0f), new Vector2(0.5f, 1f));
        }

        void OnPreRender () {
            ownCam.cullingMask = 0;
            ownCam.backgroundColor = matchCam.backgroundColor + (Color.white * 0.05f);
            ownCam.allowMSAA = matchCam.allowMSAA;
        }

        void OnPostRender () {
            var m = GLMatrixCreator.GetModelMatrix(targetMF.transform);
            var im = m.inverse;
            var vp = GLMatrixCreator.GetCameraMatrix(matchCam);
            GL.PushMatrix();
            GL.LoadIdentity();
            GL.LoadProjectionMatrix(vp);
            GL.MultMatrix(m);
            drawMat.EnableKeyword("USE_CUSTOM_MATRICES");
            drawMat.SetMatrix("_CustomModelMatrix", m);
            drawMat.SetMatrix("_CustomInverseModelMatrix", im);
            drawMat.SetPass(0);
            CustomGLCamera.DrawMeshWithNormalsAsColors(targetMF.sharedMesh);
            GL.PopMatrix();
        }

    }
	
}
