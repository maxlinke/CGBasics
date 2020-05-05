using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class VersionChecker : MonoBehaviour { 

    [SerializeField] bool runTestsOnAwake;

    private static VersionChecker instance;

    public enum VersionCheckResult {
        WEB_ERROR,
        OTHER_ERROR,
        UPDATE_AVAILABLE,
        UP_TO_DATE,
        YOU_ARE_THE_UPDATE,
    }

    void Awake () {
        if(instance != null){
            Debug.LogError($"Singleton violation! Instance of {nameof(VersionChecker)} is not null! Aborting");
            return;
        }
        instance = this;

        if(runTestsOnAwake){
            RunTests();
        }
    }

    void OnDestroy () {
        if(instance == this){
            instance = null;
        }
    }

    public static void CheckVersion (System.Action<VersionCheckResult, string> onCheckComplete) {
        if(instance == null){
            Debug.LogError("Instance was null! Aaaaaahhhhh!");
            onCheckComplete.Invoke(VersionCheckResult.OTHER_ERROR, string.Empty);
            return;
        }
        instance.DoTheVersionCheck(onCheckComplete);
    }

    void DoTheVersionCheck (System.Action<VersionCheckResult, string> onCheckComplete) {
        StartCoroutine(VersionCheckCoroutine(onCheckComplete));
    }

    IEnumerator VersionCheckCoroutine (System.Action<VersionCheckResult, string> onCheckComplete) {
        UnityWebRequest www = UnityWebRequest.Get("https://api.github.com/repos/maxlinke/CGBasics/releases/latest");
        yield return www.SendWebRequest();
        if((www.isNetworkError || www.isHttpError)){
            onCheckComplete.Invoke(VersionCheckResult.WEB_ERROR, string.Empty);
        }else if(TryGetGitVersion(www.downloadHandler.text, out var gitVersion)){
            var compareResult = CompareVersions(Application.version, gitVersion);
            switch(compareResult){
                case ComparisonResult.EQUAL:
                    onCheckComplete.Invoke(VersionCheckResult.UP_TO_DATE, gitVersion);
                    break;
                case ComparisonResult.A_GREATER_THAN_B:
                    onCheckComplete.Invoke(VersionCheckResult.YOU_ARE_THE_UPDATE, gitVersion);
                    break;
                case ComparisonResult.B_GREATER_THAN_A:
                    onCheckComplete.Invoke(VersionCheckResult.UPDATE_AVAILABLE, gitVersion);
                    break;
                default: 
                    Debug.LogError($"Unknown thing: \"{compareResult}\"!");
                    onCheckComplete.Invoke(VersionCheckResult.OTHER_ERROR, string.Empty);
                    break;
            }
        }else{
            onCheckComplete.Invoke(VersionCheckResult.OTHER_ERROR, string.Empty);
        }

        bool TryGetGitVersion (string gitAPIText, out string outputGitVersion) {
            var versionKey = "tag_name";
            var lines = gitAPIText.Split(new string[]{"\n"}, System.StringSplitOptions.RemoveEmptyEntries);
            foreach(var line in lines){
                var trimmed = line.Trim();
                if(trimmed.Length > 0 && trimmed[0] == '"'){
                    trimmed = trimmed.Substring(1);
                    var key = trimmed.Substring(0, trimmed.IndexOf("\"", 0));
                    if(key.Equals(versionKey)){
                        trimmed = trimmed.Remove(0, key.Length + 1).Trim();     // +1 is the closing "
                        trimmed = trimmed.Substring(1).Trim();                  // removing the :
                        trimmed = trimmed.Substring(0, trimmed.Length - 1);     // removing the ,
                        outputGitVersion = trimmed.Substring(1, trimmed.Length - 2);    // removing both "s
                        return true;
                    }
                }
            }
            outputGitVersion = string.Empty;
            return false;
        }
    }

    private enum ComparisonResult {
        A_GREATER_THAN_B = 1,
        B_GREATER_THAN_A = -1,
        EQUAL = 0,
        UNKNOWN = int.MaxValue
    }

    private ComparisonResult CompareVersions (string versionA, string versionB) {
        versionA = TrimExcess(versionA);
        versionB = TrimExcess(versionB);
        if(versionA.Length > 0 && versionB.Length > 0){
            var aParts = versionA.Split(new char[]{'.'}, System.StringSplitOptions.RemoveEmptyEntries);
            var bParts = versionB.Split(new char[]{'.'}, System.StringSplitOptions.RemoveEmptyEntries);
            int max = Mathf.Max(aParts.Length, bParts.Length);
            for(int i=0; i<max; i++){
                string a = (i >= aParts.Length) ? string.Empty : aParts[i].Trim();
                string b = (i >= bParts.Length) ? string.Empty : bParts[i].Trim();
                if(a.Equals(b)){
                    continue;
                }
                SplitNumbersAndLetters(a, out var aNums, out var aLetters);
                SplitNumbersAndLetters(b, out var bNums, out var bLetters); 
                if(TryGetNumValue(aNums, out var aNumVal) && TryGetNumValue(bNums, out var bNumVal)){
                    if(aNumVal > bNumVal){
                        return ComparisonResult.A_GREATER_THAN_B;
                    }
                    if(bNumVal > aNumVal){
                        return ComparisonResult.B_GREATER_THAN_A;
                    }
                }else{
                    return ComparisonResult.UNKNOWN;
                }
                int lMax = Mathf.Max(aLetters.Length, bLetters.Length);
                for(int j=0; j<lMax; j++){
                    int aVal = (j < aLetters.Length) ? (int)(aLetters[j]) : 0;
                    int bVal = (j < bLetters.Length) ? (int)(bLetters[j]) : 0;
                    if(aVal == bVal){
                        continue;
                    }
                    if(aVal > bVal){
                        return ComparisonResult.A_GREATER_THAN_B;
                    }
                    if(bVal > aVal){
                        return ComparisonResult.B_GREATER_THAN_A;
                    }
                }
            }
            return ComparisonResult.EQUAL;
        }

        return ComparisonResult.UNKNOWN;

        string TrimExcess (string inputString) {
            int i = 0;
            foreach(var c in inputString){
                if(c >= '0' || c <= '9'){
                    break;
                }
                i++;
            }
            return inputString.Substring(i);
        }

        void SplitNumbersAndLetters (string inputString, out string outputNumbersPart, out string outputLettersPart) {
            for(int i=0; i<inputString.Length; i++){
                var c = inputString[i];
                if(c >= '0' && c <= '9'){
                    continue;
                }else{
                    outputNumbersPart = inputString.Substring(0, i);
                    outputLettersPart = inputString.Substring(i).ToLower();
                    return;
                }
            }
            outputNumbersPart = inputString.Substring(0);   // copy instead of just =
            outputLettersPart = string.Empty;
        }

        bool TryGetNumValue (string inputString, out int outputNumValue) {
            if(int.TryParse(inputString, out outputNumValue)){
                return true;
            }
            outputNumValue = 0;
            return (inputString.Length == 0);
        }
    }

    void RunTests () {
        TestVersions("v1.0", "v1", ComparisonResult.EQUAL);
        TestVersions("1.0", "1", ComparisonResult.EQUAL);
        TestVersions("1.0", "1.", ComparisonResult.EQUAL);
        TestVersions("1.0", "1.0.0", ComparisonResult.EQUAL);
        TestVersions("1.a", "1.1", ComparisonResult.B_GREATER_THAN_A);
        TestVersions("1.0.1", "1.0.1a", ComparisonResult.B_GREATER_THAN_A);
        TestVersions("1.0", "2", ComparisonResult.B_GREATER_THAN_A);
        TestVersions("2.0", "1", ComparisonResult.A_GREATER_THAN_B);
        TestVersions("1.1", "1.01", ComparisonResult.EQUAL);
        TestVersions("124.0", "1.0", ComparisonResult.A_GREATER_THAN_B);
        TestVersions("0.9", "1.0", ComparisonResult.B_GREATER_THAN_A);
        TestVersions("", "1.0", ComparisonResult.UNKNOWN);
        TestVersions("", "", ComparisonResult.UNKNOWN);
        TestVersions("  01.0  ", " 1.0.0  ", ComparisonResult.EQUAL);
        TestVersions("1.1", "1.1a", ComparisonResult.B_GREATER_THAN_A);
        TestVersions("1.1a", "1.1b", ComparisonResult.B_GREATER_THAN_A);
        TestVersions("1.1b", "1.1ba", ComparisonResult.B_GREATER_THAN_A);
        TestVersions("123.00.123", "122.99.123", ComparisonResult.A_GREATER_THAN_B);
        TestVersions("123.99", "123.99a", ComparisonResult.B_GREATER_THAN_A);

        void TestVersions (string va, string vb, ComparisonResult expected) {
            var result = CompareVersions(va, vb);
            if(result != expected){
                Debug.LogError($"\"{va}\", \"{vb}\", Result: {result}, Expected: {expected}");
                return;
            }
            var inverseResult = CompareVersions(vb, va);
            if(inverseResult == result && (result == ComparisonResult.EQUAL || result == ComparisonResult.UNKNOWN)){
                // symmetry yey
            }else if(result == ComparisonResult.A_GREATER_THAN_B && inverseResult == ComparisonResult.B_GREATER_THAN_A){
                // symmetry yey
            }else if(result == ComparisonResult.B_GREATER_THAN_A && inverseResult == ComparisonResult.A_GREATER_THAN_B){
                // symmetry yey
            }else{
                Debug.LogError($"\"{va}\", \"{vb}\", Result: {result}, Inverse: {inverseResult}");
                return;
            }
            Debug.Log($"successful test: \"{va}\", \"{vb}\"");
        }
    }
	
}
