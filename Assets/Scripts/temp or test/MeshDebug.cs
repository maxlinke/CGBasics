using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDebug : MonoBehaviour {

    [SerializeField] Mesh initDebugMesh;

    void Start () {
        if(initDebugMesh != null){
            var clone = initDebugMesh.CreateClone(false, true);
            clone.LogInfo();
            var mf = GetComponent<MeshFilter>();
            if(mf != null){
                mf.sharedMesh = clone;
            }
        }
    }

    void Update () {
        
    }
	
}
