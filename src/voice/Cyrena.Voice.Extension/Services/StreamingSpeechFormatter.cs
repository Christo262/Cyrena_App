using System.Text;
using System.Text.RegularExpressions;

public sealed class StreamingSpeechFormatter
{
    private readonly StringBuilder _pendingLine = new();
    private readonly StringBuilder _speechBuffer = new();

    private bool _inCodeBlock;

    public IEnumerable<string> Push(string? token)
    {
        if (string.IsNullOrEmpty(token))
            yield break;

        _pendingLine.Append(token);

        while (true)
        {
            var text = _pendingLine.ToString();
            var newlineIndex = text.IndexOf('\n');

            if (newlineIndex < 0)
                yield break;

            var line = text[..newlineIndex];
            _pendingLine.Remove(0, newlineIndex + 1);

            foreach (var spoken in ProcessLine(line))
                yield return spoken;
        }
    }

    public IEnumerable<string> Flush()
    {
        if (_pendingLine.Length > 0)
        {
            foreach (var spoken in ProcessLine(_pendingLine.ToString()))
                yield return spoken;

            _pendingLine.Clear();
        }

        var rest = Clean(_speechBuffer.ToString());
        _speechBuffer.Clear();

        if (!string.IsNullOrWhiteSpace(rest))
            yield return rest;
    }

    private IEnumerable<string> ProcessLine(string line)
    {
        var trimmed = line.Trim();

        if (trimmed.StartsWith("```"))
        {
            _inCodeBlock = !_inCodeBlock;
            yield break;
        }

        if (_inCodeBlock)
            yield break;

        if (IsTableLine(trimmed))
            yield break;

        var clean = Clean(line);

        if (string.IsNullOrWhiteSpace(clean))
            yield break;

        if (_speechBuffer.Length > 0)
            _speechBuffer.Append(' ');

        _speechBuffer.Append(clean);

        while (TryTakeSentence(out var sentence))
            yield return sentence;
    }

    private bool TryTakeSentence(out string sentence)
    {
        sentence = "";

        var text = _speechBuffer.ToString();

        for (var i = 0; i < text.Length; i++)
        {
            var c = text[i];

            if (c != '.' && c != '?' && c != '!')
                continue;

            if (c == '.' &&
                i > 0 &&
                i < text.Length - 1 &&
                char.IsDigit(text[i - 1]) &&
                char.IsDigit(text[i + 1]))
                continue;

            var result = text[..(i + 1)].Trim();
            _speechBuffer.Remove(0, i + 1);

            if (string.IsNullOrWhiteSpace(result))
                return false;

            sentence = result;
            return true;
        }

        return false;
    }

    private static bool IsTableLine(string line)
    {
        return line.StartsWith("|") || Regex.IsMatch(line, @"^\s*\|?[\s:\-]+\|[\s:\-\|]+\s*$");
    }

    private static string Clean(string text)
    {
        text = Regex.Replace(text, @"`[^`]*`", "");
        text = Regex.Replace(text, @"\[(.*?)\]\((.*?)\)", "$1");
        text = Regex.Replace(text, @"https?:\/\/\S+", "");
        text = Regex.Replace(text, @"^\s{0,3}#{1,6}\s*", "", RegexOptions.Multiline);
        text = Regex.Replace(text, @"^\s*[-*+]\s+", "", RegexOptions.Multiline);
        text = Regex.Replace(text, @"^\s*\d+\.\s+", "", RegexOptions.Multiline);
        text = Regex.Replace(text, @"\s+", " ");
        return text.Trim();
    }
}