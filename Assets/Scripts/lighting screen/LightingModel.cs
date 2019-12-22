using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Lighting Model", menuName = "Lighting Model")]
public class LightingModel : ScriptableObject {

    public enum Type {
        Diffuse,
        Specular
    }

    [SerializeField] LightingModel.Type m_type;
    [SerializeField] Shader m_shader;
    [SerializeField, Multiline] string m_description;
    [SerializeField] Sprite m_equation;
    [SerializeField] ShaderProperty[] m_usedProperties;

    public LightingModel.Type type => m_type;
    public Shader shader => m_shader;
    public string description => m_description;
    public Sprite equation => m_equation;
    
    public IEnumerator<ShaderProperty> GetEnumerator () {
        foreach(var prop in UsedProperties()){
            yield return prop;
        }
    }

    public IEnumerable<ShaderProperty> UsedProperties () {
        foreach(var prop in m_usedProperties){
            if(prop != null){
                yield return prop;
            }else{
                Debug.LogWarning($"There are null-properties in {this.name} ({nameof(LightingModel)}!");
            }
        }
    }

    public bool UsesProperty (ShaderProperty inputProp) {
        if(inputProp == null){
            return false;
        }
        foreach(var prop in m_usedProperties){
            if(prop == inputProp){
                return true;
            }
        }
        return false;
    }
	
}
