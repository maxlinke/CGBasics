using UnityEngine;

public class CustomExternalTransform : MonoBehaviour {

    public GameObject dummyObject { get; private set; }

    public void Init (GameObject caller) {
        if(dummyObject != null){
            Debug.LogError("Duplicate Init, dummy object not null! Aborting.");
            return; 
        }
        dummyObject = new GameObject($"{nameof(CustomExternalTransform)} for \"{caller.name}\"");
        dummyObject.transform.SetParent(null, false);
        dummyObject.transform.position = caller.transform.position;
        dummyObject.transform.rotation = caller.transform.rotation;
        dummyObject.transform.localScale = caller.transform.localScale;
    }
	
}
