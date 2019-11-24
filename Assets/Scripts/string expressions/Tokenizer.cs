using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StringExpressions {
    
    public static class Tokenizer {

        public static bool TryTokenize (string input, out Token[] tokens) {
            if(input == null){
                tokens = null;
                return false;
            }
            List<Token> tokenList = new List<Token>();
            var whiteSpaceSplit = input.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);      //TODO figure out why Regex.Replace(input, @"s", ""); didn't work...
            input = string.Empty;
            foreach(var subString in whiteSpaceSplit){
                input += subString;
            }
            bool isOperator = false;   //start with operAND!!!
            string currentTokenString = string.Empty;
            foreach(char c in input){
                
            }

            //start new char
            //if previous was operator then take this as an operand
            //always operand -> operator -> operand cycle!!!
            //brackets? functions?
            //only allow +-*/ as operators, exponents are pow(x,y) (so i can implement functions)

            tokens = null;
            return false;

            // use state machine for float values? idk...
            
            bool IsNumber (char c) {
                switch(c){
                    case '0': return true;
                    case '1': return true;
                    case '2': return true;
                    case '3': return true;
                    case '4': return true;
                    case '5': return true;
                    case '6': return true;
                    case '7': return true;
                    case '8': return true;
                    case '9': return true;
                    default: return false;
                }
            }

            bool IsOperator (char c) {
                switch(c){
                    case '+': return true;
                    case '-': return true;
                    case '*': return true;
                    case '/': return true;
                    default: return false;
                }
            }

        }

    }
	
}
