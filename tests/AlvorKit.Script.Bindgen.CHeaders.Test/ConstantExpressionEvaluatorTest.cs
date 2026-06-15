namespace AlvorKit.Script.Bindgen.CHeaders.Test;

[TestClass]
public sealed class ConstantExpressionEvaluatorTest
{
    [TestMethod]
    public void Evaluate_HonorsOperatorPrecedenceAndParentheses()
    {
        Assert.AreEqual(7, ConstantExpressionEvaluator.Evaluate(["1", "+", "2", "*", "3"], []));
        Assert.AreEqual(9, ConstantExpressionEvaluator.Evaluate(["(", "1", "+", "2", ")", "*", "3"], []));
    }

    [TestMethod]
    public void Evaluate_ResolvesIdentifiersAndBitwiseOperators()
    {
        var known = new Dictionary<string, long> { ["A"] = 0b0011, ["B"] = 0b0101 };

        Assert.AreEqual(0b0111, ConstantExpressionEvaluator.Evaluate(["A", "|", "B"], known));
        Assert.AreEqual(0b0001, ConstantExpressionEvaluator.Evaluate(["A", "&", "B"], known));
    }

    [TestMethod]
    public void Evaluate_HandlesUnaryShiftAndHexLiterals()
    {
        Assert.AreEqual(16, ConstantExpressionEvaluator.Evaluate(["1", "<<", "4"], []));
        Assert.AreEqual(~0x10L, ConstantExpressionEvaluator.Evaluate(["~", "0x10UL"], []));
        Assert.AreEqual(-12, ConstantExpressionEvaluator.Evaluate(["-", "12L"], []));
    }

    [TestMethod]
    public void Evaluate_ReturnsNullForUnsupportedOrUnsafeExpressions()
    {
        Assert.IsNull(ConstantExpressionEvaluator.Evaluate(["1", "/", "0"], []));
        Assert.IsNull(ConstantExpressionEvaluator.Evaluate(["UNKNOWN"], []));
        Assert.IsNull(ConstantExpressionEvaluator.Evaluate(["1", "+"], []));
        Assert.IsNull(ConstantExpressionEvaluator.Evaluate(["(", "1", "+", "2"], []));
        Assert.IsNull(ConstantExpressionEvaluator.Evaluate(["1", "2"], []));
    }
}
