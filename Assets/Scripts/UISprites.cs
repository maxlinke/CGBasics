using UnityEngine;

// [CreateAssetMenu(menuName = "UISprite Collection", fileName = "New UISprite Collection")]
public class UISprites : ScriptableObject {

    public enum ID {
        MatrixLeft,
        MatrixRight,
        MatrixAdd,
        MatrixDelete,
        MatrixRename,
        MatrixTranspose,
        MatrixInvert,
        MatrixIdentity
    }

    private static UISprites m_instance;

    [SerializeField] UISprite[] uiSprites;

    public static Sprite GetSprite (ID id) {
        if(m_instance == null){
            FindInstance();
        }
        return m_instance.GetSpriteForID(id);
    }

    // returns the FIRST one it finds. if there is more than one sprite per id, that's my bad
    // but i'm one guy and the number of sprites is going to be limited so eh...
    private Sprite GetSpriteForID (ID id) {
        Sprite output = null;
        int matchCounter = 0;
        foreach(var uiSprite in uiSprites){
            if(uiSprite.id == id){
                if(matchCounter == 0){
                    output = uiSprite.sprite;
                }
                matchCounter++;
            }
        }
        if(matchCounter > 1){
            Debug.LogError($"Was asked to find {nameof(UISprite)} for id {id} and found {matchCounter} matches!");
        }else if(matchCounter == 0){
            Debug.LogError($"Found no sprite for ID {id}!");
        }
        return output;
    }

    private static void FindInstance () {
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
	
}
