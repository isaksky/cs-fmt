namespace CSharpier.Core;

public class CodeFormatterOptions
{
    public int Width { get; init; } = 100;
    public IndentStyle IndentStyle { get; init; } = IndentStyle.Spaces;
    public int IndentSize { get; init; } = 4;
    public EndOfLine EndOfLine { get; init; } = EndOfLine.Auto;
    public bool IncludeGenerated { get; init; }
    public bool BraceNewLine { get; init; } = true;
    public bool PreferBraces { get; init; }
    public bool OmitDefaultAccessibilityModifiers { get; init; }
    public bool PreferFileScopedNamespace { get; init; }

    internal PrinterOptions ToPrinterOptions()
    {
        return new(Formatter.CSharp)
        {
            Width = this.Width,
            UseTabs = this.IndentStyle == IndentStyle.Tabs,
            IndentSize = this.IndentSize,
            EndOfLine = this.EndOfLine,
            IncludeGenerated = this.IncludeGenerated,
            BraceNewLine = this.BraceNewLine,
            PreferBraces = this.PreferBraces,
            OmitDefaultAccessibilityModifiers = this.OmitDefaultAccessibilityModifiers,
            PreferFileScopedNamespace = this.PreferFileScopedNamespace,
        };
    }

    public static async Task<CodeFormatterResult> FormatAsync(
        string code,
        CodeFormatterOptions? options = null,
        CancellationToken cancellationToken = default
    )
    {
        options ??= new CodeFormatterOptions();
        return await CodeFormatter.FormatAsync(
            code,
            options.ToPrinterOptions(),
            cancellationToken
        );
    }
}

public enum IndentStyle
{
    Spaces,
    Tabs,
}
