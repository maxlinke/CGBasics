using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLCamera : MonoBehaviour {

    [SerializeField] Camera matchCam;
    [SerializeField] MeshFilter meshToDraw;
    [SerializeField] Material drawMat;
    [SerializeField] Vector3 offset;

    Camera attachedUnityCam;
    Material lineMaterial;

    void Awake () {
        attachedUnityCam = GetComponent<Camera>();
    }

    void Start () {
        
    }

    void Update () {
        
    }

    void LateUpdate () {
        attachedUnityCam.transform.position = matchCam.transform.position;
        attachedUnityCam.transform.rotation = matchCam.transform.rotation;
        attachedUnityCam.fieldOfView = matchCam.fieldOfView;
        attachedUnityCam.nearClipPlane = matchCam.nearClipPlane;
        attachedUnityCam.farClipPlane = matchCam.farClipPlane;
    }

    void OnPreRender () {
        attachedUnityCam.cullingMask = 0;       // just making sure we're not rendering anything the unity way...
    }

    void OnPostRender () {
        if(drawMat == null){
            drawMat = new Material(Shader.Find("Hidden/Internal-Colored"));
            drawMat.hideFlags = HideFlags.HideAndDontSave;
            drawMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            drawMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            drawMat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back);
            drawMat.SetInt("_ZWrite", 1);
            drawMat.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        }
        if(lineMaterial == null){
            // from https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnPostRender.html
            var shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            // Turn off backface culling, depth writes, depth test.
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.LessEqual);
        }
        if(meshToDraw == null){
            return;
        }
        GL.PushMatrix();
        
        GL.LoadIdentity();
        var projectionMatrix = GLMatrixCreator.GetProjectionMatrix(
            fov: attachedUnityCam.fieldOfView,
            aspectRatio: attachedUnityCam.aspect,
            zNear: attachedUnityCam.nearClipPlane,
            zFar: attachedUnityCam.farClipPlane
        );
        var viewMatrix = GLMatrixCreator.GetViewMatrix(
            eye: attachedUnityCam.transform.position,
            center: attachedUnityCam.transform.position + attachedUnityCam.transform.forward,
            up: attachedUnityCam.transform.up
        );
        var modelMatrix = GLMatrixCreator.GetTranslationMatrix(meshToDraw.transform.position);
        GL.MultMatrix(viewMatrix);
        GL.LoadProjectionMatrix(projectionMatrix);

        drawMat.SetPass(0);
        GL.Color(Color.white);
        GL.Begin(GL.TRIANGLES);
        var verts = meshToDraw.sharedMesh.vertices;
        var tris = meshToDraw.sharedMesh.triangles;
        for(int i=0; i<tris.Length; i+=3){
            // GL.Vertex((Vector3)(modelMatrix * ToV4(verts[tris[i+0]])));
            // GL.Vertex((Vector3)(modelMatrix * ToV4(verts[tris[i+1]])));
            // GL.Vertex((Vector3)(modelMatrix * ToV4(verts[tris[i+2]])));

            // Vector4 ToV4(Vector3 v3) {
            //     return new Vector4(v3.x, v3.y, v3.z, 1);
            // }
            GL.Vertex(verts[tris[i+0]]);
            GL.Vertex(verts[tris[i+1]]);
            GL.Vertex(verts[tris[i+2]]);
        }
        GL.End();

        lineMaterial.SetPass(0);
        GL.Color(new Color(1,1,1));
        GL.Begin(GL.LINES);
        for(int x=-10; x<=10; x++){
            GL.Vertex(new Vector3(x, 0, -10));
            GL.Vertex(new Vector3(x, 0, 10));
        }
        for(int z=-10; z<=10; z++){
            GL.Vertex(new Vector3(-10, 0, z));
            GL.Vertex(new Vector3(10, 0, z));
        }
        GL.End();

        GL.PopMatrix();
    }
	
}
