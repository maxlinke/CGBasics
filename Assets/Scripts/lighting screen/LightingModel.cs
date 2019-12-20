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
    
    public IEnumerator<ShaderProperty> UsedProperties () {
        foreach(var prop in m_usedProperties){
            yield return prop;
        }
    }

    public bool UsesProperty (ShaderProperty inputProp) {
        foreach(var prop in m_usedProperties){
            if(prop == inputProp){
                return true;
            }
        }
        return false;
    }
	
}
