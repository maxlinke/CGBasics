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
    // TODO formula/equation image?

    [Header("Used Properties")]
    [SerializeField] bool m_usesRoughness;
    [SerializeField] bool m_usesMinnaertExp;
    [SerializeField] bool m_usesSpecIntensity;
    [SerializeField] bool m_usesSpecHardness;
    [SerializeField] bool m_usesSpecHardnessX;
    [SerializeField] bool m_usesSpecHardnessY;

    public LightingModel.Type type => m_type;
    public Shader shader => m_shader;
    public string description => m_description;

    public bool usesRoughness => m_usesRoughness;
    public bool usesMinnaertExp => m_usesMinnaertExp;
    public bool usesSpecIntensity => m_usesSpecIntensity;
    public bool usesSpecHardness => m_usesSpecHardness;
    public bool usesSpecHardnessX => m_usesSpecHardnessX;
    public bool usesSpecHardnessY => m_usesSpecHardnessY;
	
}
