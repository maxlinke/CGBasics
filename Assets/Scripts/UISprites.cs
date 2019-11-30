using UnityEngine;

// [CreateAssetMenu(menuName = "UISprite Collection", fileName = "New UISprite Collection")]
public class UISprites : ScriptableObject {

    private static UISprites m_instance;

    private static UISprites instance {
        get {
            if(m_instance == null){
                var inRAM = Resources.FindObjectsOfTypeAll<UISprites>();
                if(inRAM.Length < 1){
                    Debug.LogError($"Found no instances of {nameof(UISprites)}! That's bad!");
                    m_instance = null;
                }else if(inRAM.Length > 1){
                    Debug.LogError($"There should only be 1 instance of {nameof(UISprites)} but there are apparently {inRAM.Length}!");
                    m_instance = inRAM[0];
                }else{
                    m_instance = inRAM[0];
                }
            }   
            return m_instance;
        }
    }

    [Header("Matrix Sprites")]
    [SerializeField] Sprite matrixLeft;
    [SerializeField] Sprite matrixRight;
    [SerializeField] Sprite matrixUp;
    [SerializeField] Sprite matrixDown;
    [SerializeField] Sprite matrixAdd;
    [SerializeField] Sprite matrixDelete;
    [SerializeField] Sprite matrixRename;
    [SerializeField] Sprite matrixTranspose;
    [SerializeField] Sprite matrixInvert;
    [SerializeField] Sprite matrixIdentity;

    public static Sprite MatrixLeft => instance.matrixLeft;
    public static Sprite MatrixRight => instance.matrixRight;
    public static Sprite MatrixUp => instance.matrixUp;
    public static Sprite MatrixDown => instance.matrixDown;
    public static Sprite MatrixAdd => instance.matrixAdd;
    public static Sprite MatrixDelete => instance.matrixDelete;
    public static Sprite MatrixRename => instance.matrixRename;
    public static Sprite MatrixTranspose => instance.matrixTranspose;
    public static Sprite MatrixInvert => instance.matrixInvert;
    public static Sprite MatrixIdentity => instance.matrixIdentity;

}
