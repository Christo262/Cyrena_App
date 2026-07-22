using System.Text;
using System.Text.RegularExpressions;

public sealed class StreamingSpeechFormatter
{
    private readonly StringBuilder _pendingLine = new();
    private readonly StringBuilder _speechBuffer = new();

    private bool _inCodeBlock;

    // Common abbreviations and honorifics whose internal periods must not be
    // treated as sentence terminators. Stored with the trailing period; lookups
    // walk leftward from a candidate period to see if the letters before it
    // form a known abbreviation.
    private static readonly string[] Abbreviations = new[]
    {
        "mr", "mrs", "ms", "dr", "prof", "sr", "jr", "st",
        "vs", "etc", "approx", "dept", "est", "vol",
        "inc", "ltd", "co", "corp",
        "gen", "gov", "sgt", "capt", "col", "lt", "cmdr",
        "fig", "no", "vol", "rev", "hon",
    };

    // Soft cap on how much text can accumulate without finding a sentence
    // terminator. If the buffer exceeds this, ProcessLine will flush the
    // accumulated text as a single chunk rather than let it grow unbounded.
    private const int MaxBufferChars = 500;

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

        // If the buffer has grown too large without finding a terminator, flush
        // it as a single chunk so it cannot grow unbounded.
        if (_speechBuffer.Length > MaxBufferChars)
        {
            var overflow = _speechBuffer.ToString().Trim();
            _speechBuffer.Clear();
            if (!string.IsNullOrWhiteSpace(overflow))
                yield return overflow;
        }
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

            // Decimal numbers like 1.5, 3.14: digits on both sides of the period.
            if (c == '.' &&
                i > 0 &&
                i < text.Length - 1 &&
                char.IsDigit(text[i - 1]) &&
                char.IsDigit(text[i + 1]))
                continue;

            // Ellipsis: "..." should be treated as a single terminator at the
            // third period, not at the first. Skip if we are inside a run of
            // periods.
            if (c == '.')
            {
                if (i + 1 < text.Length && text[i + 1] == '.')
                    continue; // middle dot of an ellipsis, keep scanning
                if (i > 0 && text[i - 1] == '.')
                    continue; // trailing dot of an ellipsis, also keep scanning
            }

            // Known abbreviations: walk leftward from the period over letters
            // to see if the run of alphabetic characters immediately preceding
            // the period is a known abbreviation. If so, do not treat the
            // period as a terminator.
            if (c == '.' && IsAbbreviation(text, i))
                continue;

            // Trailing closing quote / bracket / parenthesis: the punctuation
            // immediately after the terminator (e.g. `"`.`, `]`.`, `)`.`) is
            // considered part of the sentence, not the start of the next one.
            var end = i + 1;
            if (end < text.Length)
            {
                var next = text[end];
                if (next == '"' || next == '\'' || next == ')' || next == ']' || next == '»' || next == '”')
                    end++;
            }

            var result = text[..end].Trim();
            _speechBuffer.Remove(0, end);

            if (string.IsNullOrWhiteSpace(result))
                return false;

            sentence = result;
            return true;
        }

        return false;
    }

    private static bool IsAbbreviation(string text, int periodIndex)
    {
        // Walk leftward from the period collecting letters.
        var start = periodIndex - 1;
        while (start >= 0 && char.IsLetter(text[start]))
            start--;
        start++; // step back to the first letter

        if (start > periodIndex - 1)
            return false; // no letters before the period

        var wordLen = periodIndex - start;
        if (wordLen < 2 || wordLen > 8)
            return false; // abbreviations are 2..8 letters

        // Build the word and check.
        Span<char> buf = stackalloc char[wordLen];
        for (var j = 0; j < wordLen; j++)
            buf[j] = char.ToLowerInvariant(text[start + j]);

        var word = new string(buf);
        foreach (var abbr in Abbreviations)
            if (abbr == word)
                return true;

        // Initialisms: single capital letters separated by periods, e.g.
        // "U.S.A." or "U.K." Each letter must be uppercase and the period
        // must be followed by a space or end-of-text to be a terminator.
        // Here we are at a period — check whether the pattern is a single
        // uppercase letter immediately before it and another single uppercase
        // letter immediately after it.
        if (wordLen == 1 && char.IsUpper(text[start]))
        {
            // Look for another "<uppercase>." pattern after this one.
            var after = periodIndex + 1;
            if (after + 1 < text.Length &&
                char.IsUpper(text[after]) &&
                text[after + 1] == '.')
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
