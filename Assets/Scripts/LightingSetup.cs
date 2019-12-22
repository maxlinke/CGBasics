using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Lighting Setup", fileName = "New Lighting Setup")]
public class LightingSetup : ScriptableObject {

    [SerializeField] Color m_ambientColor;
    [SerializeField] Vector3 m_defaultEuler;
    [SerializeField] LightingSetup.Light[] m_lights;

    public Color ambientColor => m_ambientColor;
    public Vector3 defaultEuler => m_defaultEuler;
    public int lightCount => m_lights.Length;           // not entirely safe (because null-lights will be counted here) but the nullref exceptions and the debug log should be enough...

    public IEnumerable<LightingSetup.Light> Lights () {
        foreach(var l in m_lights){
            if(l == null){
                Debug.LogWarning("Null light in collection!");
                continue;
            }
            yield return l;
        }
    }

    public IEnumerator<LightingSetup.Light> GetEnumerator () {
        foreach(var l in Lights()){
            yield return l;
        }
    }

    [System.Serializable]
    public class Light {
        
        [SerializeField] Color m_color;
        [SerializeField] Vector3 m_position;
        [SerializeField] string m_name;

        public Color color => m_color;
        public Vector3 position => m_position;
        public string name => m_name;

    }
	
}
