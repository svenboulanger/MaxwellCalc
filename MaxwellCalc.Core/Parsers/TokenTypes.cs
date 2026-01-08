namespace MaxwellCalc.Core.Parsers;

/// <summary>
/// The token types used for the lexer.
/// </summary>
public enum TokenTypes : int
{
    /// <summary>
    /// The end of a line.
    /// </summary>
    EndOfLine = 1 << 0,

    /// <summary>
    /// A word of letters.
    /// </summary>
    Word = 1 << 1,

    /// <summary>
    /// A decimal point number.
    /// </summary>
    Scalar = 1 << 2,

    /// <summary>
    /// Addition operator.
    /// </summary>
    Plus = 1 << 3,

    /// <summary>
    /// Subtraction operator.
    /// </summary>
    Minus = 1 << 4,
    
    /// <summary>
    /// Multiplication operator.
    /// </summary>
    Multiply = 1 << 5,
    
    /// <summary>
    /// Division operator.
    /// </summary>
    Divide = 1 << 6,
    
    /// <summary>
    /// Exponentiation/power operator.
    /// </summary>
    Power = 1 << 7,
    
    /// <summary>
    /// Modulo operator.
    /// </summary>
    Modulo = 1 << 8,

    /// <summary>
    /// Bitwise and.
    /// </summary>
    BitwiseAnd = 1 << 9,

    /// <summary>
    /// Logical and.
    /// </summary>
    LogicalAnd = 1 << 10,

    /// <summary>
    /// Bitwise or.
    /// </summary>
    BitwiseOr = 1 << 11,

    /// <summary>
    /// Logical or.
    /// </summary>
    LogicalOr = 1 << 12,

    /// <summary>
    /// Arithmetic shift.
    /// </summary>
    ArithmeticShift = 1 << 13,

    /// <summary>
    /// Integer division.
    /// </summary>
    IntegerDivision = 1 << 14,

    /// <summary>
    /// Exclamation mark.
    /// </summary>
    Exclamation = 1 << 15,

    /// <summary>
    /// Opening parenthesis.
    /// </summary>
    OpenParenthesis = 1 << 16,
    
    /// <summary>
    /// Closing parenthesis.
    /// </summary>
    CloseParenthesis = 1 << 17,
    
    /// <summary>
    /// Opening square bracket.
    /// </summary>
    OpenSquareBracket = 1 << 18,
    
    /// <summary>
    /// Closing square bracket.
    /// </summary>
    CloseSquareBracket = 1 << 19,

    /// <summary>
    /// An argument separator.
    /// </summary>
    Separator = 1 << 20,

    /// <summary>
    /// A greater than.
    /// </summary>
    GreaterThan = 1 << 21,

    /// <summary>
    /// A greater than or equal sign.
    /// </summary>
    GreaterThanOrEqual = 1 << 22,

    /// <summary>
    /// A less than.
    /// </summary>
    LessThan = 1 << 23,

    /// <summary>
    /// A less than or equal sign.
    /// </summary>
    LessThanOrEqual = 1 << 24,

    /// <summary>
    /// Equality.
    /// </summary>
    Equal = 1 << 25,

    /// <summary>
    /// Inequality.
    /// </summary>
    NotEqual = 1 << 26,

    /// <summary>
    /// Question mark.
    /// </summary>
    Question = 1 << 27,

    /// <summary>
    /// Colon.
    /// </summary>
    Colon = 1 << 28,

    /// <summary>
    /// Assignment sign.
    /// </summary>
    Assignment = 1 << 29,

    /// <summary>
    /// Quotation.
    /// </summary>
    Quote = 1 << 30,

    /// <summary>
    /// An unknown token.
    /// </summary>
    Unknown = 1 << 31,

    /// <summary>
    /// All tokens.
    /// </summary>
    All = -1
}
