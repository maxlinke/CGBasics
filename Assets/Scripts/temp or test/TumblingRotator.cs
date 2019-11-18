using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TumblingRotator : MonoBehaviour {

    [SerializeField] Vector3 rotationSpeed;

    Vector3 randomStartOffset;

    void Start() {
        randomStartOffset = Random.insideUnitSphere * 100f;
    }

    void Update() {
        var rot = randomStartOffset + Vector3.Scale(rotationSpeed, Vector3.one * Time.time);
        transform.rotation = Quaternion.Euler(rot);
    }
	
}
