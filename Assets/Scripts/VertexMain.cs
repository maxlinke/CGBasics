using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VertexMain : MonoBehaviour {

    [SerializeField] Camera matrixCam;
    [SerializeField] Camera externalCam;

    void Awake () {
        matrixCam.gameObject.SetActive(false);
        externalCam.gameObject.SetActive(false);
    }

    void Start() {
        
    }

    void Update() {
        if(Input.GetKeyDown(KeyCode.Q)){
            matrixCam.Render();
        }
    }


	
}
