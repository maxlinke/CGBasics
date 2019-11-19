using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VertexMain : MonoBehaviour {

    [SerializeField] Camera matrixCam;
    [SerializeField] Camera externalCam;

    [SerializeField] TextMeshProUGUI tempTextField;
    CustomCamera matrixCustomCam;

    void Awake () {
        // matrixCam.gameObject.SetActive(false);
        // externalCam.gameObject.SetActive(false);
        matrixCustomCam = matrixCam.gameObject.GetComponent<CustomCamera>();
    }

    void Start () {
        
    }

    void Update () {
        // tempTextField.text = matrixCustomCam.GetProjectionMatrix().ToString();
        tempTextField.text = $"{matrixCustomCam.GetRealCameraViewMatrix().ToString()}\n{matrixCustomCam.GetCustomViewMatrix().ToString()}";
    }

    void LateUpdate () {
        // matrixCam.Render();
        // externalCam.Render();
    }


	
}
