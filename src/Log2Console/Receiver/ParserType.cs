using System.ComponentModel;

namespace Log2Console.Receiver
{
    [Description("Parser")]
    public enum ParserType
    {
        Log4j2,
        Log4Net,
        NLog
    }
}