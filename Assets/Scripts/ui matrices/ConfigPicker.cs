using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;

namespace UIMatrices {

    public static class ConfigPicker {

        public static void Open (System.Action<MatrixConfig> onConfigPicked, float scale) {
            var configTypeArray = System.Enum.GetValues(typeof(MatrixConfig.Type));
            var buttonSetups = new List<Foldout.ButtonSetup>();
            foreach(var configType in configTypeArray){
                var config = MatrixConfig.GetForType((MatrixConfig.Type)configType);
                buttonSetups.Add(new Foldout.ButtonSetup(
                    buttonName: config.name, 
                    buttonHoverMessage: config.description, 
                    buttonClickAction: () => {onConfigPicked.Invoke(config);},
                    buttonInteractable: true
                ));
            }
            Foldout.Create(buttonSetups, () => {onConfigPicked.Invoke(null);}, scale);
        }

    }

}