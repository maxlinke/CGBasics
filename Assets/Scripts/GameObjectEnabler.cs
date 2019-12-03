using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectEnabler : MonoBehaviour {

    [SerializeField] GameObject[] gameObjectsToActivateOnAwake;
    [SerializeField] GameObject[] gameObjectsToActivateOnStart;

    void Awake () {
        foreach(var go in gameObjectsToActivateOnAwake){
            go.SetActive(true);
        }
    }

    void Start () {
        foreach(var go in gameObjectsToActivateOnStart){
            go.SetActive(true);
        }
    }
	
}
