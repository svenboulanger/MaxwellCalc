﻿using MaxwellCalc.Parsers;
using MaxwellCalc.Workspaces;
using System;
using System.Collections.Generic;

namespace MaxwellCalc.Resolvers
{
    /// <summary>
    /// A resolver that allows resolving nodes to a result.
    /// </summary>
    /// <typeparam name="T">The result type.</typeparam>
    public interface IResolver<T>
    {
        /// <summary>
        /// Gets a default value to return.
        /// </summary>
        public T Default { get; }

        /// <summary>
        /// Evaluates a scalar value.
        /// </summary>
        /// <param name="scalar">The scalar value.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryScalar(string scalar, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates a unit.
        /// </summary>
        /// <param name="unit">The unit name.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryUnit(string unit, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates a variable.
        /// </summary>
        /// <param name="variable">The variable name.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryVariable(string variable, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the unary plus operator.
        /// </summary>
        /// <param name="a">The argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryPlus(T a, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the unary minus operator.
        /// </summary>
        /// <param name="a">The argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryMinus(T a, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the unary factorial operator.
        /// </summary>
        /// <param name="a">The argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryFactorial(T a, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary addition operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryAdd(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary subtraction operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TrySubtract(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary multiplication operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryMultiply(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary division operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryDivide(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary modulo operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryModulo(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary integer division operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryIntDivide(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary exponentiation operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryExp(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary in-unit operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryInUnit(T a, T b, ReadOnlyMemory<char> unit, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary bitwise OR operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryBitwiseOr(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary bitwise AND operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryBitwiseAnd(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary left shift operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryLeftShift(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary right shift operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryRightShift(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary greater than operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryGreaterThan(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary greater than or equals operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryGreaterThanOrEqual(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary less than operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryLessThan(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary less or equals operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryLessThanOrEqual(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary equality operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryEquals(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary inequality operator.
        /// </summary>
        /// <param name="a">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryNotEquals(T a, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the binary assignment operator.
        /// </summary>
        /// <param name="name">The left argument.</param>
        /// <param name="b">The right argument.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryAssign(string name, T b, IWorkspace<T> workspace, out T result);

        /// <summary>
        /// Evaluates the function.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="arguments">The arguments.</param>
        /// <param name="workspace">The workspace.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the evaluation happened; otherwise, <c>false</c>.</returns>
        public bool TryFunction(string name, IReadOnlyList<T> arguments, IWorkspace<T> workspace, out T result);
    }
}
