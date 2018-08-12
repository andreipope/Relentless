using System.Text;
using UnityEngine;

public class CustomDebug
{
    private const string LogColorStart = "<color=green>";
    private const string LogErrorColorStart = "<color=red>";
    private const string ColorClose = "</color>";

    private const string LogSize = "<size=18>";
    private const string LogErrorSize = "<size=18>";
    private const string SizeClose = "</size>";

    public static void Log(object message)
    {
        var strBuilder = new StringBuilder();
        strBuilder.Append(LogSize);
        strBuilder.Append(LogColorStart);
        strBuilder.Append(message);
        strBuilder.Append(ColorClose);
        strBuilder.Append(SizeClose);
        Debug.Log(strBuilder);
    }

    public static void LogError(object message)
    {
        var strBuilder = new StringBuilder();
        strBuilder.Append(LogErrorSize);
        strBuilder.Append(LogErrorColorStart);
        strBuilder.Append(message);
        strBuilder.Append(ColorClose);
        strBuilder.Append(SizeClose);
        Debug.LogError(strBuilder);
    }
}
