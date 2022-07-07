using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CustomLogger.Test;

[TestClass]
public class Utf8ConsoleLambdaLogForwarderTest
{
    [TestMethod]
    public void LogBytes_JunkData_ShouldNotThrow()
    {
        // these invalid UTF8 bytes are pre-generated
        var invalidBytes = Convert.FromBase64String(
            "f9Y9xM+5FjoMPWd48BQpOZpR/53WRasOQH+mjoNnTU5NG3a7BrMDie8/Gw2ejMv9+tvBnQrrtvicR4M6j7+bK7lf1DHTfcRyauKqYxq6UVVsAzKCAkO2NPy2JxO92ldP7+EYWXmBqJiSDz9N+pRIbORO5iAgkt998fKM");
        var forwarder = new Utf8ConsoleLambdaLogForwarder(_ => { });

        // just make sures that this doesn't throw
        forwarder.Forward(invalidBytes);
    }

    [TestMethod]
    public void LogString_ShouldCallWriteLine()
    {
        const string testMsg = "test";
        var mockAction = new MockWriteLineAction();
        var forwarder = new Utf8ConsoleLambdaLogForwarder(mockAction.WriteLine);
        forwarder.Forward(testMsg);

        Assert.AreEqual(testMsg, mockAction.Line);
        Assert.AreEqual(1, mockAction.CallCount);
    }

    [TestMethod]
    [DataRow("Lorem ipsum dolor sit amet")]
    [DataRow("商再新書投材高財寝証金葉怒治社森不善表変")]
    [DataRow("Λορεμ ιπσθμ δολορ σιτ αμετ")]
    public void LogBytes_ValidUtf8_ShouldCallWriteLine(string msg)
    {
        var testData = LoggerHelper.Utf8NoBomNoThrow.GetBytes(msg);
        var mockAction = new MockWriteLineAction();
        var forwarder = new Utf8ConsoleLambdaLogForwarder(mockAction.WriteLine);

        forwarder.Forward(testData);
        Assert.AreEqual(msg, mockAction.Line);
        Assert.AreEqual(1, mockAction.CallCount);
    }

    private class MockWriteLineAction
    {
        private int _callCount;

        public string? Line { get; private set; }

        public int CallCount => _callCount;

        public void WriteLine(string? str)
        {
            Interlocked.Increment(ref _callCount);
            Line = str;
        }
    }
}
