using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Sonyakh.Reporting;

namespace Sonyakh.Lexing;

public sealed class Lexer(Stream _input, ICollection<ReportItem> _reports, string _inputName)
{
    private static readonly IReadOnlyDictionary<string, Keyword> _keywords = new Dictionary<string, Keyword>
    {
        { "public", Keyword.Public },
        { "private", Keyword.Private },
        { "protected", Keyword.Protected },
        { "internal", Keyword.Internal },
        { "namespace", Keyword.Namespace },
        { "enum", Keyword.Enum },
        { "class", Keyword.Class },
        { "struct", Keyword.Struct },
        { "void", Keyword.Void },
        { "static", Keyword.Static },
        { "abstract", Keyword.Abstract },
        { "virtual", Keyword.Virtual },
        { "override", Keyword.Override },
        { "if", Keyword.If },
        { "else", Keyword.Else },
        { "for", Keyword.For },
        { "while", Keyword.While },
        { "foreach", Keyword.Foreach },
        { "do", Keyword.Do },
        { "interface", Keyword.Interface },
        { "var", Keyword.Var },
        { "return", Keyword.Return }
    };

    private readonly Stack<ILexem> _accumulatedLexems = new(); 
    private readonly StreamReader _reader = new(_input);

    private int _row;
    private int _pos;
    private char _currentChar;
    private ILexem _currentToken = new BasicLexem(LexemType.Unknown);

    public ILexem Current
    { 
        get => _currentToken; 
        private set
        {
            ArgumentNullException.ThrowIfNull(value);

            _currentToken = value;
            
            if(value.Type == LexemType.EOF && 
               _accumulatedLexems.TryPeek(out ILexem? prev) && 
               prev.Type != LexemType.EOF)
            {
                return;
            }

            _accumulatedLexems.Push(value);
        }
    }

    public bool Advance()
    {
        if (!TryGetNextChar())
        {
            Current = new BasicLexem(LexemType.EOF);
            return false;
        }

        while(char.IsWhiteSpace(_currentChar))
        {
            if (!TryGetNextChar())
            {
                Current = new BasicLexem(LexemType.EOF);
                return false;
            }
        }

        Current = LexNext();

        return true;
    }

    internal ILexem LexNext()
    {
        char nextChar;
        switch(_currentChar)
        {
            case '+':
                return new BasicLexem(LexemType.Plus);
            case '-':
                return new BasicLexem(LexemType.Minus);
            case '*':
                return new BasicLexem(LexemType.Star);
            case '/':
                //TODO: handle comments
                return new BasicLexem(LexemType.Slash);
            case '(':
                return new BasicLexem(LexemType.LeftRoundPar);
            case ')':
                return new BasicLexem(LexemType.RightRoundPar);
            case '{':
                return new BasicLexem(LexemType.LeftCurvyPar);
            case '}':
                return new BasicLexem(LexemType.RightCurvyPar);
            case '[':
                return new BasicLexem(LexemType.LeftSquarePar);
            case ']':
                return new BasicLexem(LexemType.RightSquarePar);
            case '^':
                return new BasicLexem(LexemType.Xor);
            case ';':
                return new BasicLexem(LexemType.Semicolumn);
            case '.':
                return new BasicLexem(LexemType.Dot);
            case '\0':
                return new BasicLexem(LexemType.EOF);
            case ',':
                return new BasicLexem(LexemType.Comma);
            case '=':
                if(PeekChar(out nextChar) && nextChar == '=')
                {
                    if(!TryGetNextChar())
                    {
                        throw new SonyakhException("Peek was successfull, but could not advance");
                    }

                    return new BasicLexem(LexemType.Equal);
                }

                return new BasicLexem(LexemType.Assign);
            case '!':
                if(PeekChar(out nextChar) && nextChar == '=')
                {
                    if(!TryGetNextChar())
                    {
                        throw new SonyakhException("Peek was successfull, but could not advance");
                    }

                    return new BasicLexem(LexemType.NotEqual);
                }

                return new BasicLexem(LexemType.Not);

            case '<':
                if(PeekChar(out nextChar))
                {
                    if (nextChar == '=')
                    {
                        if(!TryGetNextChar())
                        {
                            throw new SonyakhException("Peek was successfull, but could not advance");
                        }

                        return new BasicLexem(LexemType.LessOrEqual);
                    }
                    else if (nextChar == '<')
                    {
                        if(!TryGetNextChar())
                        {
                            throw new SonyakhException("Peek was successfull, but could not advance");
                        }

                        return new BasicLexem(LexemType.LeftShift);
                    }
                }

                return new BasicLexem(LexemType.Less);

            case '>':
                if(PeekChar(out nextChar))
                {
                    if (nextChar == '=')
                    {
                        if(!TryGetNextChar())
                        {
                            throw new SonyakhException("Peek was successfull, but could not advance");
                        }

                        return new BasicLexem(LexemType.GreaterOrEqual);
                    }
                    else if (nextChar == '>')
                    {
                        if(!TryGetNextChar())
                        {
                            throw new SonyakhException("Peek was successfull, but could not advance");
                        }

                        return new BasicLexem(LexemType.RightShift);
                    }
                }

                return new BasicLexem(LexemType.Greater);

            case '&':
                if(PeekChar(out nextChar) && nextChar == '&')
                {
                    if(!TryGetNextChar())
                    {
                        throw new SonyakhException("Peek was successfull, but could not advance");
                    }

                    return new BasicLexem(LexemType.LogicalAnd);
                }

                return new BasicLexem(LexemType.Ampersand);

            case '|':
                if(PeekChar(out nextChar) && nextChar == '|')
                {
                    if(!TryGetNextChar())
                    {
                        throw new SonyakhException("Peek was successfull, but could not advance");
                    }

                    return new BasicLexem(LexemType.LogicalOr);
                }

                return new BasicLexem(LexemType.VerticalLine);
                
            default:
                if (char.IsNumber(_currentChar))
                {
                    return LexNumber();
                }

                if (_currentChar == '\'')
                {
                    return LexChar();
                }

                if (_currentChar == '\"')
                {
                    return LexString();
                }

                if(char.IsLetter(_currentChar) || _currentChar == '_')
                {
                    StringBuilder sb = new();
                    sb.Append(_currentChar);

                    while (PeekChar(out nextChar) && char.IsLetterOrDigit(nextChar) || nextChar == '_')
                    {
                        sb.Append(nextChar);
                        if(!TryGetNextChar())
                        {
                            throw new SonyakhException("Peek was successfull, but could not advance");
                        }
                    }

                    string value = sb.ToString();

                    if(value == "true")
                    {
                        return new ValueLexem<bool>(LexemType.Logical, true);
                    }
                    else if (value == "false")
                    {
                        return new ValueLexem<bool>(LexemType.Logical, false);
                    }

                    if(_keywords.TryGetValue(value, out Keyword keyword))
                    {
                        return new ValueLexem<Keyword>(LexemType.Keyword, keyword);
                    }

                    return new ValueLexem<string>(LexemType.Id, value);
                }

                _reports.ReportError("Unknown lexem", _inputName, _row, _pos);
                return new BasicLexem(LexemType.Unknown);
        }
    }

    private ValueLexem<char> LexChar()
    {
        if (!TryGetNextChar())
        {
            _reports.ReportError("Unexpected EOF. Char literal is incomplete.", _inputName, _row, _pos);
            return new (LexemType.Char, '\0');
        }

        if (_currentChar == '\'')
        {
            _reports.ReportError("Empty char is not allowed.", _inputName, _row, _pos);
            return new (LexemType.Char, '\0');
        }

        char value = _currentChar;

        if (!TryGetNextChar())
        {
            _reports.ReportError("Unexpected EOF. Char literal is incomplete.", _inputName, _row, _pos);
            return new (LexemType.Char, value);
        }

        if (_currentChar != '\'')
        {
            _reports.ReportError("Unexpected character. Char literal is incomplete.", _inputName, _row, _pos);
        }

        return new (LexemType.Char, value);
    }

    private ValueLexem<string> LexString()
    {
        if(!TryGetNextChar())
        {
            _reports.ReportError("String is not terminated.", _inputName, _row, _pos);
            return new ValueLexem<string>(LexemType.String, string.Empty);
        }

        StringBuilder sb = new();
        do
        {
            if(_currentChar == '\"')
            {
                return new (LexemType.String, sb.ToString());
            }

            sb.Append(_currentChar);

        } while(TryGetNextChar());

        _reports.ReportError("Unterminated string", _inputName, _row, _pos);
        return new (LexemType.String, sb.ToString());
    }

    private ILexem LexNumber()
    {
        StringBuilder sb = new();
        sb.Append(_currentChar);
        
        char nextChar;
        while (PeekChar(out nextChar) && char.IsNumber(nextChar))
        {
            sb.Append(nextChar);
            if(!TryGetNextChar())
            {
                throw new SonyakhException("Peek was successfull, but could not advance");
            }
        }

        if (PeekChar(out nextChar) && nextChar == '.')
        {
            if(!TryGetNextChar())
            {
                throw new SonyakhException("Peek was successfull, but could not advance");
            }
            sb.Append(_currentChar);

            while (PeekChar(out nextChar) && char.IsNumber(nextChar))
            {
                sb.Append(nextChar);
                if(!TryGetNextChar())
                {
                    throw new SonyakhException("Peek was successfull, but could not advance");
                }
            }

            double doubleValue = double.Parse(sb.ToString(), CultureInfo.InvariantCulture);
            return new ValueLexem<double>(LexemType.Float, doubleValue);
        }

        int value = int.Parse(sb.ToString());
        return new ValueLexem<int>(LexemType.Integer, value);
    }

    private bool TryGetNextChar()
    {
        Span<char> currChar = new char[1];
        int read = _reader.Read(currChar);
        if(read == 0)
        {
            _currentChar = '\0';
            return false;
        }

        _pos++;
        if(currChar[0] == '\n')
        {
            _pos = 0;
            _row++;
        }

        _currentChar = currChar[0];
        return true;
    }

    private bool PeekChar(out char nextChar)
    {
        int value = _reader.Peek();
        if(value == -1)
        {
            nextChar = '\0';
            return false;
        }

        nextChar = (char)value;
        return true;
    }
}