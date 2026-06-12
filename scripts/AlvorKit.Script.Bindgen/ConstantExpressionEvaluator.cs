namespace AlvorKit.Script.Bindgen;

/// <summary>
/// Evaluates integer expressions from object-like C macros. Supported syntax is
/// intentionally small: literals, previously evaluated identifiers, parentheses,
/// unary +/-/~, and the common arithmetic/bitwise binary operators.
/// </summary>
public static class ConstantExpressionEvaluator
{
    public static long? Evaluate(List<string> tokens, Dictionary<string, long> knownConstants)
    {
        var position = 0;
        var value = ParseBinary(minPrecedence: 0);
        return value is not null && position == tokens.Count ? value : null;

        long? ParseBinary(int minPrecedence)
        {
            var left = ParseUnary();
            while (left is not null && position < tokens.Count)
            {
                var precedence = BinaryPrecedence(tokens[position]);
                if (precedence == 0 || precedence < minPrecedence)
                    return left;

                var op = tokens[position++];
                var right = ParseBinary(precedence + 1);
                if (right is null)
                    return null;

                left = op switch
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
            }
            return left;
        }

        long? ParseUnary()
        {
            if (position >= tokens.Count)
                return null;

            switch (tokens[position])
            {
                case "-":
                    position++;
                    return -ParseUnary();
                case "+":
                    position++;
                    return ParseUnary();
                case "~":
                    position++;
                    return ~ParseUnary();
                case "(":
                    position++;
                    var inner = ParseBinary(minPrecedence: 0);
                    if (inner is null || position >= tokens.Count || tokens[position] != ")")
                        return null;
                    position++;
                    return inner;
            }

            var token = tokens[position];
            if (knownConstants.TryGetValue(token, out var referencedValue))
            {
                position++;
                return referencedValue;
            }

            var literal = token.TrimEnd('u', 'U', 'l', 'L');
            var parsed = literal.StartsWith("0x") || literal.StartsWith("0X")
                ? long.TryParse(literal[2..], System.Globalization.NumberStyles.HexNumber, null, out var hex) ? hex : (long?)null
                : long.TryParse(literal, out var dec) ? dec : null;
            if (parsed is null)
                return null;

            position++;
            return parsed;
        }
    }

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
