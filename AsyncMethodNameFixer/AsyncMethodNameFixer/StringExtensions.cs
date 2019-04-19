namespace AsyncMethodNameFixer
{
    public static class StringExtensions
    {
        public static string AppendAsyncToString(this string input) => $"{input}Async";

        public static string RemoveAsyncFromString(this string input) => input.Substring(0, input.Length - 5);
    }
}