namespace MaxwellCalc.Core.Parsers.Nodes;

/// <summary>
/// Possible binary operator types.
/// </summary>
public enum BinaryOperatorTypes
{
    /// <summary>
    /// Addition operator (a + b).
    /// </summary>
    Add,

    /// <summary>
    /// Subtraction operator (a - b).
    /// </summary>
    Subtract,

    /// <summary>
    /// Multiplication operator (a * b).
    /// </summary>
    Multiply,

    /// <summary>
    /// Division operator (a / b).
    /// </summary>
    Divide,

    /// <summary>
    /// Modulo operator (a % b).
    /// </summary>
    Modulo,

    /// <summary>
    /// Integer division (a \ b)
    /// </summary>
    IntDivide,

    /// <summary>
    /// Exponentiation (a ** b).
    /// </summary>
    Exponent,

    /// <summary>
    /// Left shift (a << b).
    /// </summary>
    LeftShift,

    /// <summary>
    /// Right shift (a >> b).
    /// </summary>
    RightShift,

    /// <summary>
    /// In unit (a in b).
    /// </summary>
    InUnit,

    /// <summary>
    /// Bitwise AND (a & b).
    /// </summary>
    BitwiseAnd,

    /// <summary>
    /// Logical AND (a && b).
    /// </summary>
    LogicalAnd,

    /// <summary>
    /// Bitwise OR (a | b).
    /// </summary>
    BitwiseOr,

    /// <summary>
    /// Logical OR (a || b).
    /// </summary>
    LogicalOr,

    /// <summary>
    /// Greater than (a > b).
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Greater than or equal (a >= b).
    /// </summary>
    GreaterThanOrEqual,

    /// <summary>
    /// Less than (a < b).
    /// </summary>
    LessThan,

    /// <summary>
    /// Less than or equal (a <= b).
    /// </summary>
    LessThanOrEqual,

    /// <summary>
    /// Equality (a == b).
    /// </summary>
    Equal,

    /// <summary>
    /// Inequality (a != b).
    /// </summary>
    NotEqual,

    /// <summary>
    /// Assignment.
    /// </summary>
    Assign,
}
