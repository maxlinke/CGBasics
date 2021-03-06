﻿using UnityEngine;

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

    [Header("Generic Sprites")]
    [SerializeField] Sprite uiTemp;
    [SerializeField] Sprite uiCircle;
    [SerializeField] Sprite uiCircleHighres;
    [SerializeField] Sprite uiReset;
    [SerializeField] Sprite uiCheckmark;
    [SerializeField] Sprite uiDirLeft;
    [SerializeField] Sprite uiDirUp;
    [SerializeField] Sprite uiDirRight;
    [SerializeField] Sprite uiDirDown;
    [SerializeField] Sprite uiConfig;
    [SerializeField] Sprite uiInfo;
    [SerializeField] Sprite uiOneOne;

    public static Sprite UITemp => instance.uiTemp;
    public static Sprite UICircle => instance.uiCircle;
    public static Sprite UICircleHighres => instance.uiCircleHighres;
    public static Sprite UIReset => instance.uiReset;
    public static Sprite UICheckmark => instance.uiCheckmark;
    public static Sprite UIDirLeft => instance.uiDirLeft;
    public static Sprite UIDirUp => instance.uiDirUp;
    public static Sprite UIDirRight => instance.uiDirRight;
    public static Sprite UIDirDown => instance.uiDirDown;
    public static Sprite UIConfig => instance.uiConfig;
    public static Sprite UIInfo => instance.uiInfo;
    public static Sprite UIOneOne => instance.uiOneOne;

    [Header("Matrix Screen")]
    [SerializeField] Sprite matrixScreenGL;
    [SerializeField] Sprite matrixScreenOrtho;
    [SerializeField] Sprite matrixScreenVector;

    public static Sprite MatrixScreenGL => instance.matrixScreenGL;
    public static Sprite MatrixScreenOrtho => instance.matrixScreenOrtho;
    public static Sprite MatrixScreenVector => instance.matrixScreenVector;

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
    [SerializeField] Sprite matrixConfig;
    [SerializeField] Sprite matrixMultiply;
    [SerializeField] Sprite matrixEquals;

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
    public static Sprite MatrixConfig => instance.matrixConfig;
    public static Sprite MatrixMultiply => instance.matrixMultiply;
    public static Sprite MatrixEquals => instance.matrixEquals;

    [Header("Matrix Camera Controls")]
    [SerializeField] Sprite mCamCtrlDrawFloor;
    [SerializeField] Sprite mCamCtrlDrawOrigin;
    [SerializeField] Sprite mCamCtrlDrawSeeThrough;
    [SerializeField] Sprite mCamCtrlDrawWireframe;
    [SerializeField] Sprite mCamCtrlDrawCamera;
    [SerializeField] Sprite mCamCtrlDrawClipBox;
    [SerializeField] Sprite mCamCtrlShowCulling;
    [SerializeField] Sprite mCamCtrlBackfaceCullingOff;
    [SerializeField] Sprite mCamCtrlBackfaceCullingOn;
    [SerializeField] Sprite mCamCtrlBackfaceHighlight;

    public static Sprite MCamCtrlDrawFloor => instance.mCamCtrlDrawFloor;
    public static Sprite MCamCtrlDrawOrigin => instance.mCamCtrlDrawOrigin;
    public static Sprite MCamCtrlDrawSeeThrough => instance.mCamCtrlDrawSeeThrough;
    public static Sprite MCamCtrlDrawWireframe => instance.mCamCtrlDrawWireframe;
    public static Sprite MCamCtrlDrawCamera => instance.mCamCtrlDrawCamera;
    public static Sprite MCamCtrlDrawClipBox => instance.mCamCtrlDrawClipBox;
    public static Sprite MCamCtrlShowCulling => instance.mCamCtrlShowCulling;
    public static Sprite MCamCtrlBackfaceCullingOff => instance.mCamCtrlBackfaceCullingOff;
    public static Sprite MCamCtrlBackfaceCullingOn => instance.mCamCtrlBackfaceCullingOn;
    public static Sprite MCamCtrlBackfaceHighlight => instance.mCamCtrlBackfaceHighlight;

    [Header("Lighting Screen")]
    [SerializeField] Sprite lsRenderAmbientLight;
    [SerializeField] Sprite lsConcentricCircles;
    [SerializeField] Sprite lsPlanarModeToggle;

    public static Sprite LSRenderAmbientLight => instance.lsRenderAmbientLight;
    public static Sprite LSConcentricCircles => instance.lsConcentricCircles;
    public static Sprite LSPlanarModeToggle => instance.lsPlanarModeToggle;

}
