namespace AlvorKit.Script.Bindgen;

/// <summary>Evaluates the limited integer macro expressions accepted as generated constants.</summary>
public static class ConstantExpressionEvaluator
{
    /// <summary>Returns the expression value when every token can be evaluated safely.</summary>
    public static long? Evaluate(List<string> tokens, Dictionary<string, long> knownConstants)
    {
        var parsed = ParseBinary(tokens, knownConstants, position: 0, minPrecedence: 0);
        return parsed.Value is not null && parsed.Position == tokens.Count ? parsed.Value : null;
    }

    /// <summary>Parses a precedence-climbing binary expression.</summary>
    private static (long? Value, int Position) ParseBinary(
        List<string> tokens,
        Dictionary<string, long> knownConstants,
        int position,
        int minPrecedence)
    {
        var left = ParseUnary(tokens, knownConstants, position);
        while (left.Value is not null && left.Position < tokens.Count)
        {
            var precedence = BinaryPrecedence(tokens[left.Position]);
            if (precedence == 0 || precedence < minPrecedence)
                return left;

            var op = tokens[left.Position];
            var right = ParseBinary(tokens, knownConstants, left.Position + 1, precedence + 1);
            left = right.Value is null ? (null, right.Position) : (Apply(op, left.Value.Value, right.Value.Value), right.Position);
        }
        return left;
    }

    /// <summary>Parses unary signs, bitwise-not, parentheses, identifiers, and literals.</summary>
    private static (long? Value, int Position) ParseUnary(List<string> tokens, Dictionary<string, long> knownConstants, int position)
    {
        if (position >= tokens.Count)
            return (null, position);

        return tokens[position] switch
        {
            "-" => MapUnary(ParseUnary(tokens, knownConstants, position + 1), value => -value),
            "+" => ParseUnary(tokens, knownConstants, position + 1),
            "~" => MapUnary(ParseUnary(tokens, knownConstants, position + 1), value => ~value),
            "(" => ParseParenthesized(tokens, knownConstants, position + 1),
            var token when knownConstants.TryGetValue(token, out var known) => (known, position + 1),
            var token => (ParseLiteral(token), position + 1)
        };
    }

    /// <summary>Applies a unary transform while preserving failed parse positions.</summary>
    private static (long? Value, int Position) MapUnary((long? Value, int Position) parsed, Func<long, long> map) =>
        parsed.Value is null ? parsed : (map(parsed.Value.Value), parsed.Position);

    /// <summary>Parses a parenthesized subexpression.</summary>
    private static (long? Value, int Position) ParseParenthesized(
        List<string> tokens,
        Dictionary<string, long> knownConstants,
        int position)
    {
        var inner = ParseBinary(tokens, knownConstants, position, minPrecedence: 0);
        return inner.Value is not null && inner.Position < tokens.Count && tokens[inner.Position] == ")"
            ? (inner.Value, inner.Position + 1)
            : (null, inner.Position);
    }

    /// <summary>Applies a safe binary operation, returning null for invalid division or remainder.</summary>
    private static long? Apply(string op, long left, long right) => op switch
    {
        "|" => left | right,
        "^" => left ^ right,
        "&" => left & right,
        "<<" => left << (int)right,
        ">>" => left >> (int)right,
        "+" => left + right,
        "-" => left - right,
        "*" => left * right,
        "/" => right == 0 ? null : left / right,
        "%" => right == 0 ? null : left % right,
        _ => null
    };

    /// <summary>Parses decimal or hexadecimal integer literals with unsigned and long suffixes.</summary>
    private static long? ParseLiteral(string token)
    {
        var literal = token.TrimEnd('u', 'U', 'l', 'L');
        return literal.StartsWith("0x") || literal.StartsWith("0X")
            ? long.TryParse(literal[2..], System.Globalization.NumberStyles.HexNumber, null, out var hex) ? hex : null
            : long.TryParse(literal, out var dec) ? dec : null;
    }

    /// <summary>Returns the precedence of a supported binary operator, or zero when unsupported.</summary>
    private static int BinaryPrecedence(string token) => token switch
    {
        "|" => 1,
        "^" => 2,
        "&" => 3,
        "<<" or ">>" => 4,
        "+" or "-" => 5,
        "*" or "/" or "%" => 6,
        _ => 0
    };
}
