using UnityEngine;

public class ScriptableObjectLoader : MonoBehaviour {

    [SerializeField] ScriptableObject[] scriptableObjectsToLoad;     // that's it... just reference them and they'll be in RAM so Resources.FindObjectsOfTypeAll can find them
	
}
