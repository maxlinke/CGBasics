using System.Collections.Generic;

public abstract class UIMatrixConfig {

    public enum Type {
        Translation,
        Rotation,
        Scale
    }

    private static Dictionary<Type, UIMatrixConfig> map;

    private static TranslationConfig m_translationConfig;
    public static TranslationConfig translationConfig => m_translationConfig;

    private static RotationConfig m_rotationConfig;
    public static RotationConfig rotationConfig => m_rotationConfig;

    private static ScaleConfig m_scaleConfig;
    public static ScaleConfig scaleConfig => m_scaleConfig;

    public abstract string name { get; }
    public abstract string[] fieldStrings { get; }
    public abstract VarPreset[] defaultVariables { get; }

    static UIMatrixConfig () {
        m_translationConfig = new TranslationConfig();
        map.Add(Type.Translation, m_translationConfig);
        m_rotationConfig = new RotationConfig();
        map.Add(Type.Rotation, m_rotationConfig);
        m_scaleConfig = new ScaleConfig();
        map.Add(Type.Scale, m_scaleConfig);
    }

    public static UIMatrixConfig GetForType (Type type) {
        if(map.TryGetValue(type, out var outputConfig)){
            return outputConfig;
        }
        throw new System.ArgumentException($"Couldn't find a value for type {type}!");
    }

    public class VarPreset {
        public readonly string varName;
        public readonly float varValue;
        public VarPreset (string varName, float varValue) {
            this.varName = varName;
            this.varValue = varValue;
        }
    }

#region Translation, Rotation, Scale

    public class TranslationConfig : UIMatrixConfig {

        public const string xPos = "xPos";
        public const string yPos = "yPos";
        public const string zPos = "zPos";

        private List<string> matrix = new List<string>(){
            "1", "0", "0", "0",
            "0", "1", "0", "0",
            "0", "0", "1", "0",
            xPos, yPos, zPos, "1"
        };

        private List<VarPreset> varPresets = new List<VarPreset>(){
            new VarPreset(xPos, 0),
            new VarPreset(yPos, 0),
            new VarPreset(zPos, 0)
        };

        public override string name => "Translation";
        public override VarPreset[] defaultVariables => varPresets.ToArray();
        public override string[] fieldStrings => matrix.ToArray();

    }

    public class RotationConfig : UIMatrixConfig {

        public const string xAngle = "xAngle";
        public const string yAngle = "yAngle";
        public const string zAngle = "zAngle";

        // it is assumed that the function names accepted by the parser will never change...
        private static string sx => $"sindeg({xAngle})";
        private static string sy => $"sindeg({yAngle})";
        private static string sz => $"sindeg({zAngle})";
        private static string cx => $"cosdeg({xAngle})";
        private static string cy => $"cosdeg({yAngle})";
        private static string cz => $"cosdeg({zAngle})";

        private List<string> matrix = new List<string>(){
            $"({cz} * {cy}) + ({sx} * {sy} * {sz})", $"{sz} * {cx}", $"(-{sy} * {cz}) + ({sx} * {sz} * {cy})", "0",
            $"(-{sz} * {cy}) + ({cz} * {sx} * {sy})", $"{cz} * {cx}", $"({sz} * {sy}) + ({cy} * {cz} * {sx})", "0",
            $"{cx} * {sy}", $"-{sx}", $"{cx} * {cy}", "0",
            "0", "0", "0", "1"
        };

        private List<VarPreset> varPresets = new List<VarPreset>(){
            new VarPreset(xAngle, 0),
            new VarPreset(yAngle, 0),
            new VarPreset(zAngle, 0)
        };

        public override string name => "Rotation";
        public override VarPreset[] defaultVariables => varPresets.ToArray();
        public override string[] fieldStrings => matrix.ToArray();

    }

    public class ScaleConfig : UIMatrixConfig {

        public const string xScale = "xScale";
        public const string yScale = "yScale";
        public const string zScale = "zScale";

        private List<string> matrix = new List<string>(){
            xScale, "0", "0", "0",
            "0", yScale, "0", "0",
            "0", "0", zScale, "0",
            "0", "0", "0", "1"
        };

        private List<VarPreset> varPresets = new List<VarPreset>(){
            new VarPreset(xScale, 1),
            new VarPreset(yScale, 1),
            new VarPreset(zScale, 1)
        };

        public override string name => "Scale";
        public override VarPreset[] defaultVariables => varPresets.ToArray();
        public override string[] fieldStrings => matrix.ToArray();

    }

#endregion
	
}
