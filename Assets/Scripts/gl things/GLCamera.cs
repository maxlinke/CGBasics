using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GLCamera : MonoBehaviour {

    [SerializeField] Camera unityCam;
    [SerializeField] Mesh meshToDraw;
    [SerializeField] Material drawMat;
    [SerializeField] Vector3 offset;

    // public float fov;
    // public float zNear;
    // public float zFar;
    // public float aspectRatio => (Screen.width * unityCam.rect.width) / (Screen.height * unityCam.rect.height);

    Material lineMaterial;

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
        GL.MultMatrix(GLMatrixCreator.GetViewMatrix(
            eye: unityCam.transform.position,
            center: unityCam.transform.position + unityCam.transform.forward,
            up: unityCam.transform.up
        ));
        // GL.LoadProjectionMatrix(GLMatrixCreator.GetProjectionMatrix(fov, aspectRatio, zNear, zFar));
        GL.LoadProjectionMatrix(GLMatrixCreator.GetProjectionMatrix(
            fov: unityCam.fieldOfView,
            aspectRatio: unityCam.aspect,
            zNear: unityCam.nearClipPlane,
            zFar: unityCam.farClipPlane
        ));
        drawMat.SetPass(0);
        GL.Color(Color.white);
        GL.Begin(GL.TRIANGLES);
        var verts = meshToDraw.vertices;
        var tris = meshToDraw.triangles;
        var vertOffset = offset;
        for(int i=0; i<tris.Length; i+=3){
            GL.Vertex(verts[tris[i+0]] + vertOffset);
            GL.Vertex(verts[tris[i+1]] + vertOffset);
            GL.Vertex(verts[tris[i+2]] + vertOffset);
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
