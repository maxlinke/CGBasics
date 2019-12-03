using System.Collections.Generic;

namespace UIMatrices {

    public abstract class MatrixConfig {

        public enum Type {
            Identity,
            Translation,
            Scale,
            RotationZXY,
            RotationX,
            RotationY,
            RotationZ
        }

        private static Dictionary<Type, MatrixConfig> map;

        private static IdentityConfig m_identityConfig;
        public static IdentityConfig identityConfig => m_identityConfig;

        private static TranslationConfig m_translationConfig;
        public static TranslationConfig translationConfig => m_translationConfig;

        private static FullEulerRotationConfig m_fullRotationConfig;
        public static FullEulerRotationConfig fullRotationConfig => m_fullRotationConfig;

        private static ScaleConfig m_scaleConfig;
        public static ScaleConfig scaleConfig => m_scaleConfig;

        private static XRotConfig m_xRotConfig;
        public static XRotConfig xRotConfig => m_xRotConfig;

        private static YRotConfig m_yRotConfig;
        public static YRotConfig yRotConfig => m_yRotConfig;

        private static ZRotConfig m_zRotConfig;
        public static ZRotConfig zRotConfig => m_zRotConfig;

        public abstract string name { get; }
        public abstract string description { get; }
        public abstract string[] fieldStrings { get; }
        public abstract VarPreset[] defaultVariables { get; }

        static MatrixConfig () {
            map = new Dictionary<Type, MatrixConfig>();
            m_identityConfig = new IdentityConfig();
            map.Add(Type.Identity, m_identityConfig);
            m_translationConfig = new TranslationConfig();
            map.Add(Type.Translation, m_translationConfig);
            m_scaleConfig = new ScaleConfig();
            map.Add(Type.Scale, m_scaleConfig);
            m_fullRotationConfig = new FullEulerRotationConfig();
            map.Add(Type.RotationZXY, m_fullRotationConfig);
            m_xRotConfig = new XRotConfig();
            map.Add(Type.RotationX, m_xRotConfig);
            m_yRotConfig = new YRotConfig();
            map.Add(Type.RotationY, m_yRotConfig);
            m_zRotConfig = new ZRotConfig();
            map.Add(Type.RotationZ, m_zRotConfig);
        }

        public static MatrixConfig GetForType (Type type) {
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

        public class IdentityConfig : MatrixConfig {

            private List<string> matrix = new List<string>(){
                "1", "0", "0", "0",
                "0", "1", "0", "0",
                "0", "0", "1", "0",
                "0", "0", "0", "1"
            };

            private List<VarPreset> vars = new List<VarPreset>();

            public override string name => "Identity";
            public override string description => "An identity matrix, does nothing when multiplied with another matrix or vector.";
            public override VarPreset[] defaultVariables => vars.ToArray();
            public override string[] fieldStrings => matrix.ToArray();

        }

    #region Translation, Rotation, Scale

        public class TranslationConfig : MatrixConfig {

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
            public override string description => "Moves a vector along all three axes. Only works properly if the vector has 4 dimensions and the fourth component (w) is 1.";
            public override VarPreset[] defaultVariables => varPresets.ToArray();
            public override string[] fieldStrings => matrix.ToArray();

        }

        public class FullEulerRotationConfig : MatrixConfig {

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

            public override string name => "Euler Rotation (ZXY)";
            public override string description => "Rotates a vector around the z-, x- and y-Axis (in that order) by the degrees given.";
            public override VarPreset[] defaultVariables => varPresets.ToArray();
            public override string[] fieldStrings => matrix.ToArray();

        }

        public class ScaleConfig : MatrixConfig {

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
            public override string description => "Scales a vector in all three axes.";
            public override VarPreset[] defaultVariables => varPresets.ToArray();
            public override string[] fieldStrings => matrix.ToArray();

        }

        public class XRotConfig : MatrixConfig {

            public const string angle = "angle";

            private static string sx => $"sindeg({angle})";
            private static string cx => $"cosdeg({angle})";

            private List<string> matrix = new List<string>(){
                "1", "0", "0", "0",
                "0", cx, sx, "0",
                "0", $"-{sx}", cx, "0",
                "0", "0", "0", "1"
            };

            private List<VarPreset> varPresets = new List<VarPreset>(){
                new VarPreset(angle, 0)
            };

            public override string name => "Euler Rotation (X)";
            public override string description => "Rotates a vector around the x-Axis.";
            public override VarPreset[] defaultVariables => varPresets.ToArray();
            public override string[] fieldStrings => matrix.ToArray();

        }

        public class YRotConfig : MatrixConfig {

            public const string angle = "angle";

            private static string sx => $"sindeg({angle})";
            private static string cx => $"cosdeg({angle})";

            private List<string> matrix = new List<string>(){
                cx, "0", $"-{sx}", "0",
                "0", "1", "0", "0",
                sx, "0", cx, "0",
                "0", "0", "0", "1"
            };

            private List<VarPreset> varPresets = new List<VarPreset>(){
                new VarPreset(angle, 0)
            };

            public override string name => "Euler Rotation (Y)";
            public override string description => "Rotates a vector around the y-Axis.";
            public override VarPreset[] defaultVariables => varPresets.ToArray();
            public override string[] fieldStrings => matrix.ToArray();

        }

        public class ZRotConfig : MatrixConfig {

            public const string angle = "angle";

            private static string sx => $"sindeg({angle})";
            private static string cx => $"cosdeg({angle})";

            private List<string> matrix = new List<string>(){
                cx, sx, "0", "0",
                $"-{sx}", cx, "0", "0",
                "0", "0", "1", "0",
                "0", "0", "0", "1"
            };

            private List<VarPreset> varPresets = new List<VarPreset>(){
                new VarPreset(angle, 0)
            };

            public override string name => "Euler Rotation (Z)";
            public override string description => "Rotates a vector around the z-Axis.";
            public override VarPreset[] defaultVariables => varPresets.ToArray();
            public override string[] fieldStrings => matrix.ToArray();

        }

    #endregion
    
    }

}
