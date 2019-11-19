using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TumblingRotator : MonoBehaviour {

    [SerializeField] Vector3 rotationSpeed;

    void Start() {
        transform.Rotate(360f * Vector3.Scale(Random.insideUnitSphere, rotationSpeed));
    }

    void Update() {
        transform.Rotate(Time.deltaTime * rotationSpeed);
    }
	
}
