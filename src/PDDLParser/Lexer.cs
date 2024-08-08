using System;
using System.Collections.Generic;

public enum TokenType {
    Keyword, Symbol, Identifier, Parameter, EndOfFile
}

public class Token {
    public TokenType Type { get; }
    public string Value { get; }

    public Token(TokenType type, string value) {
        Type = type;
        Value = value;
    }
}

public class Lexer {
    private readonly string _input;
    private int _position;
    private static readonly HashSet<string> Keywords = new HashSet<string> 
    { 
        "define", "domain", "problem", "action", "parameters", "precondition", "effect", "init", "goal" 
    };

    public Lexer(string input) {
        _input = input;
        _position = 0;
    }

    public Token GetNextToken() {
        // Skip whitespace
        while (_position < _input.Length && char.IsWhiteSpace(_input[_position])) {
            _position++;
        }

        if (_position >= _input.Length) {
            return new Token(TokenType.EndOfFile, "");
        }

        char currentChar = _input[_position];

        // Handle symbols
        if (currentChar == '(' || currentChar == ')' || currentChar == ':') {
            _position++;
            return new Token(TokenType.Symbol, currentChar.ToString());
        }

        // Handle parameters
        if (currentChar == '?') {
            _position++;
            string param = ReadWhile(c => char.IsLetterOrDigit(c));
            return new Token(TokenType.Parameter, "?" + param);
        }

        // Handle identifiers and keywords
        if (char.IsLetter(currentChar)) {
            string identifier = ReadWhile(c => char.IsLetterOrDigit(c));
            if (IsKeyword(identifier)) {
                return new Token(TokenType.Keyword, identifier);
            } else {
                return new Token(TokenType.Identifier, identifier);
            }
        }

        throw new InvalidOperationException("Unexpected character: " + currentChar);
    }

    private string ReadWhile(Func<char, bool> predicate) {
        int startPos = _position;
        while (_position < _input.Length && predicate(_input[_position])) {
            _position++;
        }
        return _input.Substring(startPos, _position - startPos);
    }

    private bool IsKeyword(string identifier) {
        return Keywords.Contains(identifier);
    }
}
