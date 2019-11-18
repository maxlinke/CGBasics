using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// is this even necessary? 
// i think a bigger-scoped "screen-registry" (?) would be better. as in main menu, vertex, fragment
// settings as overlay

public static class CanvasRegistry {

    private static Dictionary<CanvasID, Canvas> map;

    public static void Register (Canvas canvas, CanvasID id) {
        EnsureMapExists();
        if(map.ContainsKey(id)){
            Debug.LogError($"Error, Canvas with ID \"{id}\" is already registered! Aborting.");
            return;
        }
        map.Add(id, canvas);
    }

    public static bool TryGetCanvas (CanvasID id, out Canvas canvas) {
        return map.TryGetValue(id, out canvas);
    }

    private static void EnsureMapExists () {
        if(map == null){
            map = new Dictionary<CanvasID, Canvas>();
        }
    }
	
}
