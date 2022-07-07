using System.Text;

namespace CustomLogger;

internal static class LoggerHelper
{
    /// <summary>
    /// Make sure we do not use BOM when encoding.
    /// Also it's better to log junk data than throwing exceptions when logging.
    /// </summary>
    public static readonly Encoding Utf8NoBomNoThrow = new UTF8Encoding(false, false);
}
