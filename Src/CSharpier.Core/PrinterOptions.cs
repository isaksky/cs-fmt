using System.Globalization;

namespace CSharpier.Core;

internal class PrinterOptions(Formatter formatter)
{
    public bool IncludeAST { get; init; }
    public bool IncludeDocTree { get; init; }
    public bool UseTabs { get; set; }

    private int indentSize = formatter == Formatter.XML ? 2 : 4;
    public int IndentSize
    {
        get => this.indentSize;
        set
        {
            if (value <= 0)
            {
                throw new ArgumentException("An indent size of 0 is not valid");
            }
            this.indentSize = value;
        }
    }

    public int Width { get; set; } = 100;
    public EndOfLine EndOfLine { get; set; } = EndOfLine.Auto;
    public bool TrimInitialLines { get; init; } = true;
    public bool IncludeGenerated { get; set; }
    public Formatter Formatter { get; set; } = formatter;

    /// <summary>
    /// When false, opening braces are placed on the same line (K&R style).
    /// When true (default), opening braces go on a new line (Allman style).
    /// Corresponds to csharp_new_line_before_open_brace.
    /// </summary>
    public bool BraceNewLine { get; set; } = true;

    /// <summary>
    /// When true, always wrap single-statement bodies in braces.
    /// Corresponds to csharp_prefer_braces = true.
    /// </summary>
    public bool PreferBraces { get; set; }

    /// <summary>
    /// When true, omit default accessibility modifiers (private for members, internal for top-level types).
    /// Corresponds to dotnet_style_require_accessibility_modifiers = omit_if_default.
    /// </summary>
    public bool OmitDefaultAccessibilityModifiers { get; set; }

    /// <summary>
    /// When true, convert block-scoped namespaces to file-scoped namespaces.
    /// Corresponds to csharp_style_namespace_declarations = file_scoped.
    /// </summary>
    public bool PreferFileScopedNamespace { get; set; }

    /// <summary>
    /// When true, the first expression in a method chain stays on the same line
    /// as the variable declaration or assignment operator.
    /// Corresponds to csharpier_chain_first_expression_on_same_line = true.
    /// </summary>
    public bool ChainFirstExpressionOnSameLine { get; set; }

    public const int WidthUsedByTests = 100;

    internal static string GetLineEnding(string code, PrinterOptions printerOptions)
    {
        if (printerOptions.EndOfLine != EndOfLine.Auto)
        {
            return printerOptions.EndOfLine == EndOfLine.CRLF ? "\r\n" : "\n";
        }

        var lineIndex = code.IndexOf('\n');
        if (lineIndex <= 0)
        {
            return "\n";
        }
        if (code[lineIndex - 1] == '\r')
        {
            return "\r\n";
        }

        return "\n";
    }

    public static Formatter GetFormatter(string filePath)
    {
        var possibleExtension = Path.GetExtension(filePath);
        if (possibleExtension == string.Empty)
        {
            return Formatter.Unknown;
        }

        var extension = possibleExtension[1..].ToLower(CultureInfo.InvariantCulture);

        var formatter = extension switch
        {
            "cs" => Formatter.CSharp,
            "csx" => Formatter.CSharpScript,
            "config" or "csproj" or "props" or "slnx" or "targets" or "xaml" or "xml" =>
                Formatter.XML,
            _ => Formatter.Unknown,
        };
        return formatter;
    }
}

internal enum Formatter
{
    Unknown,
    CSharp,
    CSharpScript,
    XML,
}
