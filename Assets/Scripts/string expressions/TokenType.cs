namespace StringExpressions {

    public enum TokenType {
        NUMBER,     // followed by operator
        OPERATOR,   // followed by anything but an operator
        // FUNCTION,   // string followed by (                     // needs lookup
        // VARIABLE    // string followed by another operator      // needs lookup
    }
    
}