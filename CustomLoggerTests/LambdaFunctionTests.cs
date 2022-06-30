using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CustomLoggerTests;

[TestClass]
public class LambdaFunctionTests
{
    private readonly AsyncLocal<string> _asyncLocalString = new AsyncLocal<string>();

    [TestMethod]
    public void Outer()
    {
        _asyncLocalString.Value = "Outer";

        InnerCall1().GetAwaiter().GetResult();

        Assert.AreEqual("Outer", _asyncLocalString.Value);
    }

    private async Task InnerCall1()
    {
        Assert.AreEqual("Outer", _asyncLocalString.Value);
        _asyncLocalString.Value = "InnerCall1";
        await Task.Delay(100);
        await InnerCall2();

        Assert.AreEqual("InnerCall1", _asyncLocalString.Value);
    }

    private async Task InnerCall2()
    {
        Assert.AreEqual("InnerCall1", _asyncLocalString.Value);
        _asyncLocalString.Value = "InnerCall2";
        await Task.Delay(100);
    }
}
