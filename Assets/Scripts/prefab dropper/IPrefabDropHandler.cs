using System.Collections.Generic;
using UnityEngine;

public interface IPrefabDropHandler {

    void OnDroppedIntoScene (IEnumerable<GameObject> allDroppedObjects);
	
}
