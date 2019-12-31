using UnityEngine;

[CreateAssetMenu(menuName = "Model Preset", fileName = "New Model Preset")]
public class ModelPreset : ScriptableObject {

    [SerializeField] bool m_includeInWebGLBuilds;
    [SerializeField] Mesh m_mesh;
    [SerializeField] Mesh m_flatMesh;
    [SerializeField] Color m_color;
    [SerializeField] Color m_specColor;
    [TextArea(3, 10), SerializeField] string m_description;
    
    public bool includeInWebGLBuilds => m_includeInWebGLBuilds;
    public Mesh mesh => m_mesh;
    public Mesh flatMesh => m_flatMesh;
    public Color color => m_color;
    public Color specColor => m_specColor;
    public string description => m_description;
	
}