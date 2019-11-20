using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLCamera : MonoBehaviour {

    [SerializeField] Camera unityCam;
    [SerializeField] Mesh meshToDraw;
    [SerializeField] Material drawMat;
    [SerializeField] Vector3 offset;

    public float fov;
    public float zNear;
    public float zFar;
    public float aspectRatio => (Screen.width * unityCam.rect.width) / (Screen.height * unityCam.rect.height);

    void Start () {
        
    }

    void Update () {
        
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
        if(meshToDraw == null){
            return;
        }
        GL.PushMatrix();
        GL.LoadIdentity();
        var projectionMatrix = GLMatrixCreator.GetProjectionMatrix(fov, aspectRatio, zNear, zFar);
        var fixAttemptMatrix = new Matrix4x4(
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, -1, 0),
            new Vector4(0, 0, 0, 1)
        );
        projectionMatrix = projectionMatrix * fixAttemptMatrix;
        GL.LoadProjectionMatrix(projectionMatrix);
        drawMat.SetPass(0);
        GL.Color(Color.white);
        GL.Begin(GL.TRIANGLES);
        var verts = meshToDraw.vertices;
        var tris = meshToDraw.triangles;
        // var vertOffset = Vector3.Scale(offset, new Vector3(1, 1, -1));
        var vertOffset = offset;
        for(int i=0; i<tris.Length; i+=3){
            GL.Vertex(verts[tris[i+0]] + vertOffset);
            GL.Vertex(verts[tris[i+1]] + vertOffset);
            GL.Vertex(verts[tris[i+2]] + vertOffset);
        }
        GL.End();
        GL.PopMatrix();
    }
	
}
