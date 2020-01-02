using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour {

    [Header("Prefabs")]
    [SerializeField] MatrixScreen matrixScreenPrefab;
    [SerializeField] LightingScreen lightingScreenPrefab;

    [Header("Components")]
    [SerializeField] MainMenuWindowOverlay windowOverlay;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] TextMeshProUGUI labelDropShadow;

    [Header("Main Buttons")]
    [SerializeField] Button matrixButton;
    [SerializeField] Button lightingButton;
    [SerializeField] Image[] buttonBackgrounds;
    [SerializeField] Image[] buttonOutlines;
    [SerializeField] TextMeshProUGUI[] buttonLabels;
    [SerializeField] TextMeshProUGUI[] buttonLabelDropShadows;

    [Header("Background Settings")]
    [SerializeField] int backgroundObjectLayer;
    [SerializeField] float backgroundCamFOV;
    [SerializeField] Vector3 backgroundCamPosition;
    [SerializeField] float backgroundCamNearClip;
    [SerializeField] float backgroundCamFarClip;
    [SerializeField] Mesh backgroundMesh;
    [SerializeField] Vector3 backgroundObjectPosition;
    [SerializeField] float backgroundObjectScale;
    [SerializeField] Material backgroundMatTemplate;
    [SerializeField] float backgroundMeshRotationSpeed;
    [SerializeField, Range(0, 1)] float wireGizmoColorStrength;
    [SerializeField] bool backgroundObjectCyclesThroughColors;
    [SerializeField] Vector2 backgroundObjectSaturationValue;
    [SerializeField] float colorCycleTime;

    bool initialized = false;
    List<Button> mainButtons;
    Camera backgroundCam;
    Color wireFloorColor;
    Color xAxis, yAxis, zAxis;
    Material backgroundMeshMat;
    GameObject backgroundObject;
    Material wireMat;
    bool glWireCache;
    Color backgroundColor;

    void Start () {
        Initialize();
    }

    void Initialize () {
        if(initialized){
            Debug.LogError("Duplicate init call! Aborting...");
            return;
        }
        mainButtons = new List<Button>();
        matrixButton.onClick.AddListener(() => {OpenScreen(matrixScreenPrefab);});
        lightingButton.onClick.AddListener(() => {OpenScreen(lightingScreenPrefab);});
        SetupBackground();
        windowOverlay.Initialize(this);
        this.initialized = true;
        LoadColors(ColorScheme.current);

        void OpenScreen (CloseableScreen screenPrefab) {
            gameObject.SetActive(false);
            var newScreen = Instantiate(screenPrefab);
            newScreen.SetupCloseAction(() => {
                Destroy(newScreen.gameObject);
                gameObject.SetActive(true);
            });
        }

        void SetupBackground () {
            backgroundCam = new GameObject("Main Menu Background Cam", typeof(Camera)).GetComponent<Camera>();
            backgroundCam.cullingMask = 1 << backgroundObjectLayer;
            backgroundCam.renderingPath = RenderingPath.Forward;
            backgroundCam.allowMSAA = CustomGLCamera.allowMSAA;
            if(backgroundCam.allowMSAA){
                backgroundCam.rect = new Rect(new Vector2(0, 0), new Vector2(1.0000001f, 1f));  // this is really weird but a (0,0),(1,1)-rect camera doesn't get msaa...
            }
            backgroundCam.nearClipPlane = backgroundCamNearClip;
            backgroundCam.farClipPlane = backgroundCamFarClip;
            backgroundCam.orthographic = false;
            backgroundCam.fieldOfView = backgroundCamFOV;
            backgroundCam.transform.position = backgroundCamPosition;
            backgroundCam.transform.rotation = Quaternion.LookRotation(backgroundObjectPosition-backgroundCamPosition, Vector3.up);

            wireMat = CustomGLCamera.GetLineMaterial(false);
            var camScript = backgroundCam.gameObject.AddComponent<BackgroundCam>();
            camScript.onPreRender += () => {
                glWireCache = GL.wireframe;
                GL.wireframe = true;
            };
            camScript.onPostRender += () => {
                GL.wireframe = false;
                var view = GLMatrixCreator.GetViewMatrix(backgroundCam.transform.position, backgroundCam.transform.forward, backgroundCam.transform.up);
                var proj = GLMatrixCreator.GetProjectionMatrix(backgroundCam.fieldOfView, backgroundCam.aspect, backgroundCam.nearClipPlane, backgroundCam.farClipPlane);
                GL.PushMatrix();
                GL.LoadIdentity();
                GL.LoadProjectionMatrix(proj);
                GL.MultMatrix(view);
                wireMat.SetPass(0);
                CustomGLCamera.DrawWireFloor(wireFloorColor, false, true);
                CustomGLCamera.DrawAxes(xAxis, yAxis, zAxis, false);
                GL.PopMatrix();
                GL.wireframe = glWireCache;
            };

            backgroundMeshMat = Instantiate(backgroundMatTemplate);
            backgroundMeshMat.hideFlags = HideFlags.HideAndDontSave;

            backgroundObject = new GameObject("Main Menu Background Object", typeof(MeshFilter), typeof(MeshRenderer));
            backgroundObject.layer = backgroundObjectLayer;
            backgroundObject.transform.position = backgroundObjectPosition;
            backgroundObject.transform.localScale = Vector3.one * backgroundObjectScale;
            backgroundObject.GetComponent<MeshFilter>().sharedMesh = backgroundMesh;
            backgroundObject.GetComponent<MeshRenderer>().sharedMaterial = backgroundMeshMat;
        }
    }

    void Update () {
        if(!initialized){
            return;
        }
        backgroundObject.transform.localEulerAngles += new Vector3(0, Time.deltaTime * backgroundMeshRotationSpeed, 0);
        if(backgroundObjectCyclesThroughColors){
            var rawColor = Color.HSVToRGB(Mathf.Repeat(Time.time / colorCycleTime, 1), backgroundObjectSaturationValue.x, backgroundObjectSaturationValue.y);
            backgroundMeshMat.color = wireGizmoColorStrength * rawColor + (1 - wireGizmoColorStrength) * backgroundColor;
        }
    }

    void OnEnable () {
        LoadColors(ColorScheme.current);
        ColorScheme.onChange += LoadColors;
        if(initialized){
            backgroundCam.gameObject.SetActive(true);
            backgroundObject.SetActive(true);
        }
    }

    void OnDisable () {
        ColorScheme.onChange -= LoadColors;
        if(backgroundCam != null && backgroundCam.gameObject != null){
            backgroundCam.gameObject.SetActive(false);
        }
        if(backgroundObject != null){
            backgroundObject.SetActive(false);
        }
    }

    void LoadColors (ColorScheme cs) {
        if(!initialized){
            return;
        }
        label.color = cs.MainMenuTitle;
        labelDropShadow.color = cs.MainMenuTitleDropShadow;
        foreach(var img in buttonBackgrounds){
            img.color = Color.white;
        }
        foreach(var img in buttonOutlines){
            img.color = cs.MainMenuMainButtonsOutline;
        }
        foreach(var text in buttonLabels){
            text.color = cs.MainMenuMainButtonsText;
        }
        foreach(var text in buttonLabelDropShadows){
            text.color = cs.MainMenuMainButtonsTextDropShadow;
        }
        matrixButton.SetFadeTransition(0f, cs.MainMenuMainButtons, cs.MainMenuMainButtonsHover, cs.MainMenuMainButtonsClick, Color.magenta);
        lightingButton.SetFadeTransition(0f, cs.MainMenuMainButtons, cs.MainMenuMainButtonsHover, cs.MainMenuMainButtonsClick, Color.magenta);
        backgroundCam.backgroundColor = cs.ApplicationBackground;
        backgroundMeshMat.color = WireGizmoColor(cs.MainMenuBackgroundWireObject);
        xAxis = WireGizmoColor(cs.RenderXAxis);
        yAxis = WireGizmoColor(cs.RenderYAxis);
        zAxis = WireGizmoColor(cs.RenderZAxis);
        wireFloorColor = WireGizmoColor(cs.RenderWireFloor);
        backgroundColor = cs.ApplicationBackground;
        windowOverlay.LoadColors(cs);

        Color WireGizmoColor (Color origColor) {
            return wireGizmoColorStrength * origColor + (1 - wireGizmoColorStrength) * cs.ApplicationBackground;
        }
    }

    class BackgroundCam : MonoBehaviour {

        public event System.Action onPreRender = delegate {};
        public event System.Action onPostRender = delegate {};

        void OnPreRender () {
            onPreRender.Invoke();
        }

        void OnPostRender () {
            onPostRender.Invoke();
        }

    }

}
