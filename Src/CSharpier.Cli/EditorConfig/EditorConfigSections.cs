using CSharpier.Core;

namespace CSharpier.Cli.EditorConfig;

/// <summary>
/// This is a representation of the editorconfig for the given directory along with
/// sections from any parent files until a root file is found
/// </summary>
internal class EditorConfigSections
{
    public required string DirectoryName { get; init; }
    public required IReadOnlyCollection<Section> SectionsIncludingParentFiles { get; init; }

    public PrinterOptions? ConvertToPrinterOptions(string filePath, bool ignoreDirectory)
    {
        var sections = this
            .SectionsIncludingParentFiles.Where(o => o.IsMatch(filePath, ignoreDirectory))
            .ToList();
        var resolvedConfiguration = new ResolvedConfiguration(sections);

        var formatter =
            resolvedConfiguration.Formatter ?? PrinterOptions.GetFormatter(filePath).ToString();

        if (!Enum.TryParse<Formatter>(formatter, ignoreCase: true, out var parsedFormatter))
        {
            return null;
        }

        var printerOptions = new PrinterOptions(parsedFormatter);

        if (resolvedConfiguration.MaxLineLength is { } maxLineLength)
        {
            printerOptions.Width = maxLineLength;
        }

        if (resolvedConfiguration.IndentStyle is "tab")
        {
            printerOptions.UseTabs = true;
        }

        if (printerOptions.UseTabs)
        {
            printerOptions.IndentSize = resolvedConfiguration.TabWidth ?? printerOptions.IndentSize;
        }
        else
        {
            printerOptions.IndentSize =
                resolvedConfiguration.IndentSize ?? printerOptions.IndentSize;
        }

        if (resolvedConfiguration.EndOfLine is { } endOfLine)
        {
            printerOptions.EndOfLine = endOfLine;
        }

        if (resolvedConfiguration.BraceNewLine is { } braceNewLine)
        {
            printerOptions.BraceNewLine = braceNewLine;
        }

        if (resolvedConfiguration.PreferBraces is { } preferBraces)
        {
            printerOptions.PreferBraces = preferBraces;
        }

        if (resolvedConfiguration.OmitDefaultAccessibilityModifiers is { } omitModifiers)
        {
            printerOptions.OmitDefaultAccessibilityModifiers = omitModifiers;
        }

        if (resolvedConfiguration.PreferFileScopedNamespace is { } preferFileScoped)
        {
            printerOptions.PreferFileScopedNamespace = preferFileScoped;
        }

        if (resolvedConfiguration.ChainFirstExpressionOnSameLine is { } chainFirst)
        {
            printerOptions.ChainFirstExpressionOnSameLine = chainFirst;
        }

        return printerOptions;
    }

    private class ResolvedConfiguration
    {
        public string? IndentStyle { get; }
        public int? IndentSize { get; }
        public int? TabWidth { get; }
        public int? MaxLineLength { get; }
        public EndOfLine? EndOfLine { get; }
        public string? Formatter { get; }
        public bool? BraceNewLine { get; }
        public bool? PreferBraces { get; }
        public bool? OmitDefaultAccessibilityModifiers { get; }
        public bool? PreferFileScopedNamespace { get; }
        public bool? ChainFirstExpressionOnSameLine { get; }

        public ResolvedConfiguration(List<Section> sections)
        {
            var indentStyle = sections.LastOrDefault(o => o.IndentStyle != null)?.IndentStyle;
            if (indentStyle is "space" or "tab")
            {
                this.IndentStyle = indentStyle;
            }

            var maxLineLength = sections.LastOrDefault(o => o.MaxLineLength != null)?.MaxLineLength;
            if (int.TryParse(maxLineLength, out var maxLineLengthValue) && maxLineLengthValue > 0)
            {
                this.MaxLineLength = maxLineLengthValue;
            }

            var indentSize = sections.LastOrDefault(o => o.IndentSize != null)?.IndentSize;
            var tabWidth = sections.LastOrDefault(o => o.TabWidth != null)?.TabWidth;

            if (indentSize == "tab")
            {
                if (int.TryParse(tabWidth, out var tabWidthValue))
                {
                    this.TabWidth = tabWidthValue;
                }

                this.IndentSize = this.TabWidth;
            }
            else
            {
                if (int.TryParse(indentSize, out var indentSizeValue))
                {
                    this.IndentSize = indentSizeValue;
                }

                this.TabWidth = int.TryParse(tabWidth, out var tabWidthValue)
                    ? tabWidthValue
                    : this.IndentSize;
            }

            var endOfLine = sections.LastOrDefault(o => o.EndOfLine != null)?.EndOfLine;
            if (Enum.TryParse(endOfLine, true, out EndOfLine result))
            {
                this.EndOfLine = result;
            }

            this.Formatter = sections.LastOrDefault(o => o.Formatter is not null)?.Formatter;

            // csharp_new_line_before_open_brace = none => K&R style
            var newLineBeforeOpenBrace = sections.LastOrDefault(o => o.NewLineBeforeOpenBrace != null)?.NewLineBeforeOpenBrace;
            if (newLineBeforeOpenBrace != null)
            {
                this.BraceNewLine = !newLineBeforeOpenBrace.Equals("none", StringComparison.OrdinalIgnoreCase);
            }

            // csharp_prefer_braces = true[:severity] => mandatory braces
            var preferBraces = sections.LastOrDefault(o => o.PreferBraces != null)?.PreferBraces;
            if (preferBraces != null)
            {
                var value = preferBraces.Split(':')[0].Trim();
                this.PreferBraces = value.Equals("true", StringComparison.OrdinalIgnoreCase);
            }

            // dotnet_style_require_accessibility_modifiers = omit_if_default[:severity]
            var requireModifiers = sections.LastOrDefault(o => o.RequireAccessibilityModifiers != null)?.RequireAccessibilityModifiers;
            if (requireModifiers != null)
            {
                var value = requireModifiers.Split(':')[0].Trim();
                this.OmitDefaultAccessibilityModifiers = value.Equals("omit_if_default", StringComparison.OrdinalIgnoreCase);
            }

            // csharp_style_namespace_declarations = file_scoped[:severity]
            var nsDeclarations = sections.LastOrDefault(o => o.NamespaceDeclarations != null)?.NamespaceDeclarations;
            if (nsDeclarations != null)
            {
                var value = nsDeclarations.Split(':')[0].Trim();
                this.PreferFileScopedNamespace = value.Equals("file_scoped", StringComparison.OrdinalIgnoreCase);
            }

            // csharpier_chain_first_expression_on_same_line = true
            var chainFirst = sections.LastOrDefault(o => o.ChainFirstExpressionOnSameLine != null)?.ChainFirstExpressionOnSameLine;
            if (chainFirst != null)
            {
                this.ChainFirstExpressionOnSameLine = chainFirst.Equals("true", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
