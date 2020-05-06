using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class VersionChecker : MonoBehaviour { 

    private static VersionChecker instance;

    public enum UpdateCheckResult {
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
    }

    void OnDestroy () {
        if(instance == this){
            instance = null;
        }
    }

    public static void LookForUpdates (System.Action<UpdateCheckResult, string> onCheckComplete) {
        if(instance == null){
            Debug.LogError("Instance was null! Aaaaaahhhhh!");
            onCheckComplete.Invoke(UpdateCheckResult.OTHER_ERROR, string.Empty);
            return;
        }
        instance.DoTheUpdateCheck(onCheckComplete);
    }

    public static bool CheckVersionValidity () {
        try{
            new Version(Application.version);
            return true;
        }catch{
            return false;
        }
    }

    void DoTheUpdateCheck (System.Action<UpdateCheckResult, string> onCheckComplete) {
        StartCoroutine(VersionCheckCoroutine(onCheckComplete));
    }

    IEnumerator VersionCheckCoroutine (System.Action<UpdateCheckResult, string> onCheckComplete) {
        UnityWebRequest www = UnityWebRequest.Get("https://api.github.com/repos/maxlinke/CGBasics/releases/latest");
        yield return www.SendWebRequest();
        if((www.isNetworkError || www.isHttpError)){
            onCheckComplete.Invoke(UpdateCheckResult.WEB_ERROR, string.Empty);
        }else if(TryGetGitVersion(www.downloadHandler.text, out var gitVersionString)){
            try{
                var compareResult = new Version(Application.version).CompareTo(new Version(gitVersionString));
                switch(compareResult){
                    case Version.CompareResult.EQUAL:
                        onCheckComplete.Invoke(UpdateCheckResult.UP_TO_DATE, gitVersionString);
                        break;
                    case Version.CompareResult.NEWER:
                        onCheckComplete.Invoke(UpdateCheckResult.YOU_ARE_THE_UPDATE, gitVersionString);
                        break;
                    case Version.CompareResult.OLDER:
                        onCheckComplete.Invoke(UpdateCheckResult.UPDATE_AVAILABLE, gitVersionString);
                        break;
                    default: 
                        Debug.LogError($"Unknown thing: \"{compareResult}\"!");
                        onCheckComplete.Invoke(UpdateCheckResult.OTHER_ERROR, string.Empty);
                        break;
                }
            }catch{
                onCheckComplete.Invoke(UpdateCheckResult.OTHER_ERROR, string.Empty);
            }
            
        }else{
            onCheckComplete.Invoke(UpdateCheckResult.OTHER_ERROR, string.Empty);
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

    private class Version {

        public enum CompareResult {
            NEWER,
            EQUAL,
            OLDER
        }

        private List<int> numbers;
        private string letterSuffix;

        public Version (string versionString) {
            if(versionString == null || versionString.Length == 0){
                throw new System.Exception("Null or empty version strings aren't valid!");
            }
            versionString = versionString.ToLower();
            if(versionString[0] == 'v'){
                versionString = versionString.Substring(1);
            }else if(!IsNumeral(versionString[0])){
                throw new System.Exception($"Invalid first char! ({versionString[0]})");
            }
            this.numbers = new List<int>();
            this.letterSuffix = string.Empty;
            int i = 0;
            char c = versionString[0];
            while(i<versionString.Length){
                string numberPart = string.Empty;
                string letterPart = string.Empty;
                // first char of each element must be a numeral (don't advance here)
                if(!IsNumeral(c)){
                    throw GetFormatException();
                }
                // collect all 1 or more numerals
                while(IsNumeral(c)){
                    numberPart += c;
                    if(!Next()) break;
                }
                // next char must be letter or '.'
                // if the end is reached, c will still be a numeral and so this won't execute
                // similarly this won't executed if the c is '.'
                while(IsLetter(c)){
                    letterPart += c;
                    if(!Next()) break;
                }
                if(EndReached()){           // letters must be absolute end! ("1.0.3f" is okay but "1.0.3f.1" is not!)
                    ParseAndSaveNumber();
                    this.letterSuffix = letterPart;
                    break;
                }else if(letterPart.Length > 0){
                    throw GetFormatException();
                }
                // now it MUST be '.'
                if((c != '.')){
                    throw GetFormatException();
                }
                ParseAndSaveNumber();
                Next();
                if(EndReached()){
                    throw GetFormatException();
                }

                bool Next () {
                    i++;
                    if(EndReached()){
                        return false;
                    }
                    c = versionString[i];
                    return true;
                }

                bool EndReached () {
                    return (i >= versionString.Length);
                }

                void ParseAndSaveNumber () {
                    if(int.TryParse(numberPart, out int parsedNumber)){
                        numbers.Add(parsedNumber);
                    }else{
                        throw GetFormatException();
                    }
                }
            }

            bool IsNumeral (char inputChar) {
                return (inputChar >= '0' && inputChar <= '9');
            }

            bool IsLetter (char inputChar) {
                return ((inputChar >= 'a' && inputChar <= 'z') || (inputChar >= 'A' && inputChar <= 'Z'));
            }

            System.Exception GetFormatException () {
                return new System.Exception("Couldn't parse version string, check your formatting!");
            }
        }

        public CompareResult CompareTo (Version other) {
            int max = Mathf.Max(this.numbers.Count, other.numbers.Count);
            for(int i=0; i<max; i++){
                int thisNum = (i < this.numbers.Count) ? this.numbers[i] : 0;
                int otherNum = (i < other.numbers.Count) ? other.numbers[i] : 0;
                if(thisNum > otherNum){
                    return CompareResult.NEWER;
                }else if(thisNum < otherNum){
                    return CompareResult.OLDER;
                }
            }
            max = Mathf.Max(this.letterSuffix.Length, other.letterSuffix.Length);
            for(int i=0; i<max; i++){
                int thisLet = (i < this.letterSuffix.Length) ? this.letterSuffix[i] : 0;
                int otherLet = (i < other.letterSuffix.Length) ? other.letterSuffix[i] : 0;
                if(thisLet > otherLet){
                    return CompareResult.NEWER;
                }else if(thisLet < otherLet){
                    return CompareResult.OLDER;
                }
            }
            return CompareResult.EQUAL;
        }

        public bool IsNewerThan (Version other) {
            return this.CompareTo(other) == CompareResult.NEWER;
        }

        public bool IsOlderThan (Version other) {
            return this.CompareTo(other) == CompareResult.OLDER;
        }

        public override bool Equals (object obj) {
            if(obj is Version other){
                return this.CompareTo(other) == CompareResult.EQUAL;
            }
            return false;
        }

        public override int GetHashCode () {
            int output = 0;
            foreach(var number in numbers){
                output += number;
            }
            foreach(var ch in letterSuffix){
                output += ((int)ch);
            }
            return output;
        }

        public override string ToString () {
            var sb = new System.Text.StringBuilder();
            for(int i=0; i<numbers.Count; i++){
                sb.Append($"{numbers[i]}");
                if(i+1 < numbers.Count){
                    sb.Append(".");
                }
            }
            sb.Append(letterSuffix);
            return sb.ToString();
        }

    }
	
}
