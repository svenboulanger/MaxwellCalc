using System;

namespace MaxwellCalc.Parsers
{
    /// <summary>
    /// A lexer.
    /// </summary>
    public class Lexer
    {
        private readonly string _input;
        private int _index = 0;

        /// <summary>
        /// Gets the content of the current token.
        /// </summary>
        public ReadOnlyMemory<char> Content => _input.AsMemory(Column, _index - Column);

        /// <summary>
        /// Gets the start of the current token.
        /// </summary>
        public int Column { get; private set; }

        /// <summary>
        /// Gets the curren type.
        /// </summary>
        public TokenTypes Type { get; private set; }

        /// <summary>
        /// Gets or sets the decimal character.
        /// </summary>
        public char Decimal { get; set; } = '.';

        /// <summary>
        /// Gets or sets the separator character.
        /// </summary>
        public char Separator { get; set; } = ',';

        /// <summary>
        /// Gets the character at the current index.
        /// </summary>
        protected char Char { get; private set; }

        /// <summary>
        /// Gets whether the current token contained trivia (leading whitespaces).
        /// </summary>
        public bool ContainsTrivia { get; private set; }

        /// <summary>
        /// Creates a new <see cref="Lexer"/>.
        /// </summary>
        /// <param name="content">The content.</param>
        public Lexer(string content)
        {
            _input = content;
            if (content.Length > 0)
                Char = _input[0];
            else
                Char = '\0';
            Next();
        }

        /// <summary>
        /// Continues to the next character
        /// </summary>
        protected void Continue()
        {
            if (_index < _input.Length)
            {
                _index++;
                if (_index >= _input.Length)
                    Char = '\0';
                else
                    Char = _input[_index];
            }
        }

        /// <summary>
        /// Looks a number of characters ahead.
        /// </summary>
        /// <param name="offset">The offset from the current character (<seealso cref="Char"/>).</param>
        /// <returns>Returns the character.</returns>
        protected char LookAhead(int offset)
        {
            int index = _index + offset;
            if (index >= 0 && index < _input.Length)
                return _input[index];
            return '\0';
        }

        /// <summary>
        /// Reads the next token.
        /// </summary>
        public void Next()
        {
            // Skip any trivia
            ContainsTrivia = false;
            while (Char == ' ' || Char == '\t')
            {
                ContainsTrivia = true;
                Continue();
            }

            // Deal with the actual content
            Column = _index;
            switch (Char)
            {
                case '\0':
                    Type = TokenTypes.EndOfLine;
                    break;

                case '+':
                    Type = TokenTypes.Plus;
                    Continue();
                    break;

                case '-':
                    Type = TokenTypes.Minus;
                    Continue();
                    break;

                case '/':
                    Type = TokenTypes.Divide;
                    Continue();
                    break;

                case '*':
                    Type = TokenTypes.Multiply;
                    Continue();
                    if (Char == '*')
                    {
                        Type = TokenTypes.Power;
                        Continue();
                    }
                    break;

                case '^':
                    Type = TokenTypes.Power;
                    Continue();
                    break;

                case '%':
                    Type = TokenTypes.Modulo;
                    Continue();
                    break;

                case '<':
                    Type = TokenTypes.LessThan;
                    Continue();
                    if (Char == '=')
                    {
                        Type = TokenTypes.LessThanOrEqual;
                        Continue();
                    }
                    else if (Char == '<')
                    {
                        Type = TokenTypes.ArithmeticShift;
                        Continue();
                    }
                    break;

                case '>':
                    Type = TokenTypes.GreaterThan;
                    Continue();
                    if (Char == '=')
                    {
                        Type = TokenTypes.GreaterThanOrEqual;
                        Continue();
                    }
                    else if (Char == '>')
                    {
                        Type = TokenTypes.ArithmeticShift;
                        Continue();
                    }
                    break;

                case '=':
                    Type = TokenTypes.Assignment;
                    Continue();
                    if (Char == '=')
                    {
                        Type = TokenTypes.Equal;
                        Continue();
                    }
                    break;

                case '!':
                    Type = TokenTypes.Exclamation;
                    Continue();
                    if (Char == '=')
                    {
                        Type = TokenTypes.NotEqual;
                        Continue();
                    }
                    break;

                case '(':
                    Type = TokenTypes.OpenParenthesis;
                    Continue();
                    break;

                case ')':
                    Type = TokenTypes.CloseParenthesis;
                    Continue();
                    break;

                case '\'':
                case '"':
                    Type = TokenTypes.Quote;
                    Continue();
                    break;

                case char s when s == Separator:
                    Type = TokenTypes.Separator;
                    Continue();
                    break;

                case char w when char.IsLetter(w):
                    Type = TokenTypes.Word;
                    Continue();
                    while (char.IsLetterOrDigit(Char) || Char == '_')
                        Continue();
                    break;

                case char n when char.IsDigit(n):

                    // We start by reading a number
                    Type = TokenTypes.Scalar;
                    Continue();
                    while (char.IsDigit(Char))
                        Continue();

                    // If a decimal follows, it becomes a real number
                    if (Char == Decimal)
                    {
                        Continue();
                        while (char.IsDigit(Char))
                            Continue();
                    }

                    // We might have an exponent after it.
                    if (Char == 'e' || Char == 'E')
                    {
                        char la = LookAhead(1);
                        if ((la == '+' || la == '-') &&
                            char.IsDigit(LookAhead(2)))
                        {
                            Continue(); // Consume 'e'
                            Continue(); // Consume '+' or '-'
                            Continue(); // Consume the digit
                            while (char.IsDigit(Char))
                                Continue();
                        }
                        else if (char.IsDigit(la))
                        {
                            Continue(); // Consume "e"
                            Continue(); // Consume the digit
                            while (char.IsDigit(Char))
                                Continue();
                        }
                    }
                    break;

                default:
                    Type = TokenTypes.Unknown;
                    Continue();
                    break;
            }
        }

        /// <summary>
        /// Gets a part of the input until this point (without the current token).
        /// </summary>
        /// <param name="from">The starting index.</param>
        /// <returns>Returns the tracked content.</returns>
        public ReadOnlyMemory<char> Track(int from) => _input.AsMemory(from, Column - from);
    }
}
