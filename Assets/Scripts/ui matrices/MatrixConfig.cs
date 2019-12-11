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
            RotationZ,
            Rebase,
            PerspProj,
            OrthoProj
        }

        private static Dictionary<Type, MatrixConfig> map;

        public static IdentityConfig identityConfig { get; private set; }
        public static TranslationConfig translationConfig { get; private set; }
        public static InverseTranslationConfig inverseTranslationConfig { get; private set; }
        public static FullEulerRotationConfig fullRotationConfig { get; private set; }
        public static ScaleConfig scaleConfig { get; private set; }
        public static XRotConfig xRotConfig { get; private set; }
        public static YRotConfig yRotConfig { get; private set; }
        public static ZRotConfig zRotConfig { get; private set; }
        public static RebaseConfig rebaseConfig { get; private set; }
        public static PerspectiveProjectionConfig perspProjConfig { get; private set; }
        public static OrthographicProjectionConfig orthoProjConfig { get; private set; }

        public abstract string name { get; }
        public abstract string description { get; }
        public abstract string[] fieldStrings { get; }
        public abstract VarPreset[] defaultVariables { get; }

        static MatrixConfig () {
            map = new Dictionary<Type, MatrixConfig>();
            identityConfig = new IdentityConfig();
            map.Add(Type.Identity, identityConfig);
            translationConfig = new TranslationConfig();
            map.Add(Type.Translation, translationConfig);
            scaleConfig = new ScaleConfig();
            map.Add(Type.Scale, scaleConfig);
            fullRotationConfig = new FullEulerRotationConfig();
            map.Add(Type.RotationZXY, fullRotationConfig);
            xRotConfig = new XRotConfig();
            map.Add(Type.RotationX, xRotConfig);
            yRotConfig = new YRotConfig();
            map.Add(Type.RotationY, yRotConfig);
            zRotConfig = new ZRotConfig();
            map.Add(Type.RotationZ, zRotConfig);
            rebaseConfig = new RebaseConfig();
            map.Add(Type.Rebase, rebaseConfig);
            perspProjConfig = new PerspectiveProjectionConfig();
            map.Add(Type.PerspProj, perspProjConfig);
            orthoProjConfig = new OrthographicProjectionConfig();
            map.Add(Type.OrthoProj, orthoProjConfig);
            // the non-enum ones
            inverseTranslationConfig = new InverseTranslationConfig();
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

    #region Identity, Dynamic

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

        // not to be found in the list. these are to be created from matrices, in case i want to copy them for instance...
        public class DynamicConfig : MatrixConfig {

            private List<string> matrix;
            private List<VarPreset> vars;
            private string m_name;

            public override string name => m_name;
            public override string description => null;     // this should hopefully throw a nice exception if it is ever used
            public override VarPreset[] defaultVariables => vars.ToArray();
            public override string[] fieldStrings => matrix.ToArray();

            public DynamicConfig (string inputName, string[] inputMatrix, IEnumerable<VarPreset> inputVarPresets) {
                if(inputMatrix.Length != 16){
                    throw new System.ArgumentException($"Input string array MUST have 16 values! Input had {inputMatrix.Length}...");
                }
                m_name = inputName;
                matrix = new List<string>();
                for(int i=0; i<inputMatrix.Length; i++){
                    matrix.Add(inputMatrix[i]);
                }
                vars = new List<VarPreset>();
                foreach(var preset in inputVarPresets){
                    vars.Add(preset);
                }
            }

            public DynamicConfig (UIMatrix uiMatrix) {
                m_name = uiMatrix.GetName();
                matrix = new List<string>();
                for(int i=0; i<16; i++){
                    matrix.Add(uiMatrix[i]);
                }
                vars = new List<VarPreset>();
                var varMap = uiMatrix.VariableContainer.GetVariableMap();
                foreach(var key in varMap.Keys){
                    vars.Add(new VarPreset(key, varMap[key]));
                }
            }

        }

    #endregion

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

        // no enum for this one! this one's only for scripts to access!
        public class InverseTranslationConfig : MatrixConfig {

            public const string xPos = "xPos";
            public const string yPos = "yPos";
            public const string zPos = "zPos";

            private List<string> matrix = new List<string>(){
                "1", "0", "0", "0",
                "0", "1", "0", "0",
                "0", "0", "1", "0",
                $"-{xPos}", $"-{yPos}", $"-{zPos}", "1"
            };

            private List<VarPreset> varPresets = new List<VarPreset>(){
                new VarPreset(xPos, 0),
                new VarPreset(yPos, 0),
                new VarPreset(zPos, 0)
            };

            public override string name => "Inverse Translation";
            public override string description => null;
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

        // TODO is this REALLY the best name?
        public class RebaseConfig : MatrixConfig {

            public const string newXx = "newXx";
            public const string newXy = "newXy";
            public const string newXz = "newXz";
            public const string newYx = "newYx";
            public const string newYy = "newYy";
            public const string newYz = "newYz";
            public const string newZx = "newZx";
            public const string newZy = "newZy";
            public const string newZz = "newZz";

            private List<string> matrix = new List<string>(){
                newXx, newXy, newXz, "0",
                newYx, newYy, newYz, "0",
                newZx, newZy, newZz, "0",
                "0", "0", "0", "1"
            };

            private List<VarPreset> vars = new List<VarPreset>(){
                new VarPreset(newXx, 1),
                new VarPreset(newXy, 0),
                new VarPreset(newXz, 0),
                new VarPreset(newYx, 0),
                new VarPreset(newYy, 1),
                new VarPreset(newYz, 0),
                new VarPreset(newZx, 0),
                new VarPreset(newZy, 0),
                new VarPreset(newZz, 1)
            };

            public override string name => "Base Substitution";
            public override string description => "Transforms the vector into a new base. If the base vectors are orthogonal and normalized, this behaves like a rotation matrix.";
            public override VarPreset[] defaultVariables => vars.ToArray();
            public override string[] fieldStrings => matrix.ToArray();

        }

    #endregion
    
    #region Projection

        public class PerspectiveProjectionConfig : MatrixConfig {

            public const string fov = "fov";
            public const string aspect = "aspect";
            public const string nearClip = "nearClip";
            public const string farClip = "farClip";

            private List<string> matrix = new List<string>(){
                $"1 / ({aspect} * tan(({fov} * pi()) / 360))", "0", "0", "0",
                "0", $"1 / tan(({fov} * pi()) / 360)", "0", "0",
                "0", "0", $"({farClip} + {nearClip}) / ({farClip} - {nearClip})", "1",
                "0", "0", $"(-2 * {farClip} * {nearClip}) / ({farClip} - {nearClip})", "0"
            };

            private List<VarPreset> varPresets = new List<VarPreset>(){
                new VarPreset(fov, 60),
                new VarPreset(aspect, 16f/9f),  // this one will have to be set via script from the camera, as long as it is in control
                new VarPreset(nearClip, 0.5f),
                new VarPreset(farClip, 10f)
            };

            public override string name => "Perspective Projection";
            public override string description => "Corrects for aspect ratio and writes the vertex' z-coordinate into the w-coordinate, so perspective projection can be done when dividing by w.";
            public override VarPreset[] defaultVariables => varPresets.ToArray();
            public override string[] fieldStrings => matrix.ToArray();

        }

        public class OrthographicProjectionConfig : MatrixConfig {

            public const string orthoSize = "orthoSize";
            public const string aspect = "aspect";
            public const string nearClip = "nearClip";
            public const string farClip = "farClip";

            private List<string> matrix = new List<string>(){
                $"2 / ({orthoSize} * {aspect})", "0", "0", "0",
                "0", $"2 / {orthoSize}", "0", "0",
                "0", "0", $"2 / ({farClip} - {nearClip})", "0",
                "0", "0", $"-({farClip} + {nearClip}) / ({farClip} - {nearClip})", "1"
            };

            private List<VarPreset> varPresets = new List<VarPreset>(){
                new VarPreset(orthoSize, 3),
                new VarPreset(aspect, 16f/9f),  // this one will have to be set via script from the camera, as long as it is in control
                new VarPreset(nearClip, 0.5f),
                new VarPreset(farClip, 10f)
            };

            public override string name => "Orthographic Projection";
            public override string description => "A projection matrix without perspective distortion. ";
            public override VarPreset[] defaultVariables => varPresets.ToArray();
            public override string[] fieldStrings => matrix.ToArray();

        }

    #endregion

    }

}
