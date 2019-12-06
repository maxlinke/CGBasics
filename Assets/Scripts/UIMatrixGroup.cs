﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIMatrixGroup : MonoBehaviour {

    [Header("Prefabs")]
    [SerializeField] UIMatrix matrixPrefab;

    [Header("Components")]
    [SerializeField] RectTransform m_rectTransform;
    [SerializeField] RectTransform headerRT;
    [SerializeField] RectTransform contentRT;
    [SerializeField] Image backgroundImage;
    [SerializeField] Image headerBackground;
    [SerializeField] TextMeshProUGUI headerLabel;
    [SerializeField] TextMeshProUGUI headerDropShadow;

    [Header("Settings")]
    [SerializeField] float verticalMatrixMargin;
    [SerializeField] float horizontalMatrixMargin;
    [SerializeField] float multiplicationSignSize;

    MatrixScreen matrixScreen;
    List<UIMatrix> matrices;
    List<RectTransform> multiplicationSigns;
    bool initialized;

    public RectTransform rectTransform => m_rectTransform;

    public UIMatrix this[int index] => matrices[index];
    public int matrixCount => matrices.Count;
    
    public void Initialize (MatrixScreen matrixScreen) {
        if(initialized){
            Debug.LogWarning($"Duplicate init of {nameof(UIMatrixGroup)}, aborting!", this.gameObject);
            return;
        }
        this.matrixScreen = matrixScreen;
        matrices = new List<UIMatrix>();
        multiplicationSigns = new List<RectTransform>();
        EnsureTheresAtLeastOneMatrix(blockWarning: true);
        RebuildContent();
        initialized = true;
    }

    public IEnumerator<UIMatrix> GetEnumerator () {
        foreach(var m in matrices){
            yield return m;
        }
    }

    public void LoadColors (Color headerColor, ColorScheme cs) {
        headerBackground.color = headerColor;
        backgroundImage.color = cs.MatrixScreenMatrixGroupBackground;
        headerLabel.color = cs.UiMatrixLabel;
        headerDropShadow.color = cs.UiMatrixLabelDropShadow;
        foreach(var signRT in multiplicationSigns){
            signRT.gameObject.GetComponent<Image>().color = cs.MatrixScreenMultiplicationSign;
        }
    }

    public void SetName (string name) {
        if(name == null){
            Debug.LogError("Name can't be null!", this.gameObject);
            return;
        }
        name = name.Trim();
        if(name.Length < 1){
            Debug.LogError("Name can't be empty!", this.gameObject);
            return;
        }
        this.gameObject.name = name;
        headerLabel.text = name;
        headerDropShadow.text = name;
    }

    // TODO adding buttons to the header (resizing the text recttransform), editing the text, loading the colors...

    void RebuildContent () {
        EnsureTheresAtLeastOneMatrix();
        EnsureRightAmountOfMultiplicationSigns();
        float x = 0f;
        for(int i=0; i<matrixCount; i++){
            x += horizontalMatrixMargin;
            matrices[i].rectTransform.SetAnchor(new Vector2(0, 1));
            matrices[i].rectTransform.pivot = new Vector2(0, 1);
            matrices[i].rectTransform.anchoredPosition = new Vector2(x, -verticalMatrixMargin);
            x += matrices[i].rectTransform.rect.width;
            x += horizontalMatrixMargin;
            if(i+1 < matrixCount){
                multiplicationSigns[i].SetAnchor(new Vector2(0, 0.5f));
                multiplicationSigns[i].pivot = new Vector2(0, 0.5f);
                multiplicationSigns[i].anchoredPosition = new Vector2(x, 0);
                x += multiplicationSigns[i].rect.width;
            }
        }
        float totalHeight = headerRT.rect.height + 2 * verticalMatrixMargin + matrices[0].minHeight;        // there will always be a matrices[0]...
        rectTransform.sizeDelta = new Vector2(x, totalHeight);
        LoadColors(headerBackground.color, ColorScheme.current);

        void EnsureRightAmountOfMultiplicationSigns () {
            int numberOfMultiplicationSignsRequired = matrixCount - 1;
            int delta = numberOfMultiplicationSignsRequired - multiplicationSigns.Count;
            for(; delta < 0; delta++){
                int lastIndex = multiplicationSigns.Count - 1;
                var lastSign = multiplicationSigns[lastIndex];
                multiplicationSigns.RemoveAt(lastIndex);
                Destroy(lastSign.gameObject);
            }
            for(; delta > 0; delta--){
                var newSignRT = new GameObject("Multiplication Sign", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
                newSignRT.SetParent(contentRT, false);
                newSignRT.localScale = Vector3.one;
                newSignRT.sizeDelta = Vector2.one * multiplicationSignSize;
                newSignRT.gameObject.GetComponent<Image>().sprite = UISprites.MatrixMultiply;
                multiplicationSigns.Add(newSignRT);
            }
        }
    }

    public bool TryGetIndexOf (UIMatrix matrix, out int index) {
        if(matrices.Contains(matrix)){
            index = matrices.IndexOf(matrix);
            return true;
        }
        index = -1;
        return false;
    }

    public UIMatrix CreateMatrixAtIndex (UIMatrices.MatrixConfig config, UIMatrix.Editability editability, int index, bool rebuildContent = true) {
        var newMatrix = Instantiate(matrixPrefab);
        newMatrix.rectTransform.SetParent(contentRT, false);
        newMatrix.rectTransform.localScale = Vector3.one;
        newMatrix.Initialize(config, editability, false);
        newMatrix.matrixScreen = this.matrixScreen;
        newMatrix.matrixGroup = this;
        if(index < matrices.Count){
            matrices.Insert(index, newMatrix);
        }else{
            matrices.Add(newMatrix);
        }
        if(rebuildContent){
            RebuildContent();
        }
        return newMatrix;
    }

    public bool DeleteMatrix (UIMatrix matrixToRemove, bool rebuildContent = true) {
        if(ReleaseMatrix(matrixToRemove, rebuildContent)){
            Destroy(matrixToRemove.gameObject);
            return true;
        }else{
            return false;
        }
    }

    public bool ReleaseMatrix (UIMatrix matrixToRelease, bool rebuildContent = true) {
        if(TryGetIndexOf(matrixToRelease, out int removeIndex)){
            matrixToRelease.matrixGroup = null;
            matrixToRelease.rectTransform.SetParent(null, false);
            matrixToRelease.rectTransform.localScale = Vector3.one;
            matrices.RemoveAt(removeIndex);
            EnsureTheresAtLeastOneMatrix();
            if(rebuildContent){
                RebuildContent();
            }
            return true;
        }else{
            Debug.LogError($"Couldn't remove matrix {matrixToRelease} because it wasn't in this group!", this.gameObject);
            return false;
        }
    }

    public void InsertMatrix (UIMatrix matrixToInsert, int insertIndex, bool rebuildContent = true) {
        if(matrixToInsert.matrixGroup != null){
            Debug.LogError("Matrix is (apparently) still in another group! Aborting...", this.gameObject);
            return;
        }
        matrixToInsert.matrixGroup = this;
        matrixToInsert.rectTransform.SetParent(contentRT, false);
        matrixToInsert.rectTransform.localScale = Vector3.one;
        if(insertIndex < matrices.Count){
            matrices.Insert(insertIndex, matrixToInsert);
        }else{
            matrices.Add(matrixToInsert);
        }
        if(rebuildContent){
            RebuildContent();
        }
    }

    public bool TryMoveMatrix (UIMatrix matrixToMove, int offset, bool rebuildContent = true) {
        if(TryGetIndexOf(matrixToMove, out var matrixIndex)){
            int finalIndex = matrixIndex + offset;
            if(finalIndex < 0 || finalIndex >= matrixCount){
                return false;
            }
            matrices.RemoveAt(matrixIndex);
            if(finalIndex < matrices.Count){
                matrices.Insert(finalIndex, matrixToMove);
            }else{
                matrices.Add(matrixToMove);
            }
            if(rebuildContent){
                RebuildContent();
            }
            return true;
        }else{
            Debug.LogError("Can't move matrix because it isn't a part of this group!", this.gameObject);
            return false;
        }
    }

    void EnsureTheresAtLeastOneMatrix (bool blockWarning = false) {
        if(matrices.Count > 0){
            return;
        }
        if(!blockWarning){
            Debug.LogWarning("There should ALWAYS be one matrix! I'll create one here but this REALLY shouldn't be happening!");
        }
        var newMatrix = CreateMatrixAtIndex(UIMatrices.MatrixConfig.identityConfig, UIMatrix.Editability.FULL, 0, false);
    }
	
}
