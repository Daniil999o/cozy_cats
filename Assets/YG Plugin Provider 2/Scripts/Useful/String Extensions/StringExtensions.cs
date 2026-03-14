using System;

public static class StringExtensions
{
    public static bool IsEmpty(this string text) => String.IsNullOrEmpty(text);

    public static bool IsEmptyOrWhiteSpaces(this string text) => String.IsNullOrWhiteSpace(text) || String.IsNullOrEmpty(text);
    

    public static object Format(this object text, string style) => text.ToString().Format(style);
    public static string Format(this string text, string style) => String.Format(text, style);
    public static string Format(this float value, string style) => string.Format(style, value);
    public static string Format(this int value, string style) => String.Format(style, value);

    public static string[] SplitLines(this string text) => text.Split('\n');
}