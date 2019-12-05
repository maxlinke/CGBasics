using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabDropper : MonoBehaviour {

    private static PrefabDropper instance;

    [SerializeField] GameObject[] prefabsToDrop;

    void Awake () {
        if(instance != null){
            Debug.LogError($"There can only be ONE instance of \"{nameof(PrefabDropper)}\"! Aborting...", this.gameObject);
            return;
        }
        instance = this;
        List<GameObject> droppedPrefabs = new List<GameObject>();
        for(int i=0; i<prefabsToDrop.Length; i++){
            if(prefabsToDrop[i] == null){
                Debug.LogWarning($"Prefab Nr. {i} in {nameof(PrefabDropper)} is null! Not a huge issue, but why?");
                continue;
            }
            droppedPrefabs.Add(Instantiate(prefabsToDrop[i]));
        }
        for(int i=0; i<droppedPrefabs.Count; i++){
            var dropHandler = droppedPrefabs[i].GetComponent<IPrefabDropHandler>();
            if(dropHandler != null){
                dropHandler.OnDroppedIntoScene(droppedPrefabs);
            }
        }
    }
	
}
