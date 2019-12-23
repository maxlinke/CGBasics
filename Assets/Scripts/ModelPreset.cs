using UnityEngine;

[CreateAssetMenu(menuName = "Model Preset", fileName = "New Model Preset")]
public class ModelPreset : ScriptableObject {

    [SerializeField] Mesh m_mesh;
    [SerializeField] Color m_color;
    [SerializeField] Color m_specColor;

    public Mesh mesh => m_mesh;
    public Color color => m_color;
    public Color specColor => m_specColor;
	
}
