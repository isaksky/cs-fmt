using CSharpier.Core.DocTypes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.Core.CSharp.SyntaxPrinter;

internal static class Modifiers
{
    private class DefaultOrder : IComparer<SyntaxToken>
    {
        // use the default order from https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/style-rules/ide0036
        private static readonly string[] DefaultOrdered =
        [
            "public",
            "private",
            "protected",
            "internal",
            "file",
            "static",
            "extern",
            "new",
            "virtual",
            "abstract",
            "sealed",
            "override",
            "readonly",
            "unsafe",
            "required",
            "volatile",
            "async",
        ];

        public int Compare(SyntaxToken x, SyntaxToken y)
        {
            return GetIndex(x.Text) - GetIndex(y.Text);
        }

        private static int GetIndex(string? value)
        {
            var result = Array.IndexOf(DefaultOrdered, value);
            return result == -1 ? int.MaxValue : result;
        }
    }

    private static readonly DefaultOrder Comparer = new();

    public static Doc Print(SyntaxTokenList modifiers, PrintingContext context)
    {
        if (modifiers.Count == 0)
        {
            return Doc.Null;
        }

        var filtered = FilterDefaultModifiers(modifiers, context);
        if (filtered.Count == 0)
        {
            return Doc.Null;
        }

        return Doc.Group(Doc.Join(" ", filtered.Select(o => Token.Print(o, context))), " ");
    }

    public static Doc PrintSorted(SyntaxTokenList modifiers, PrintingContext context)
    {
        var filtered = FilterDefaultModifiers(modifiers, context);
        return PrintWithSortedModifiers(
            filtered,
            modifiers,
            context,
            sortedModifiers =>
                Doc.Group(Doc.Join(" ", sortedModifiers.Select(o => Token.Print(o, context))), " ")
        );
    }

    public static Doc PrintSorterWithoutLeadingTrivia(
        SyntaxTokenList modifiers,
        PrintingContext context
    )
    {
        var filtered = FilterDefaultModifiers(modifiers, context);
        return PrintWithSortedModifiers(
            filtered,
            modifiers,
            context,
            sortedModifiers =>
                Doc.Group(
                    Token.PrintWithoutLeadingTrivia(sortedModifiers[0], context),
                    " ",
                    sortedModifiers.Count > 1
                        ? Doc.Concat(
                            sortedModifiers
                                .Skip(1)
                                .Select(o => Token.PrintWithSuffix(o, " ", context))
                                .ToArray()
                        )
                        : Doc.Null
                )
        );
    }

    /// <summary>
    /// Filters out default accessibility modifiers when OmitDefaultAccessibilityModifiers is enabled.
    /// - private is default for class/struct members
    /// - internal is default for top-level type declarations
    /// </summary>
    private static SyntaxTokenList FilterDefaultModifiers(
        SyntaxTokenList modifiers,
        PrintingContext context
    )
    {
        if (!context.Options.OmitDefaultAccessibilityModifiers)
        {
            return modifiers;
        }

        var parent = modifiers.Count > 0 ? modifiers[0].Parent : null;
        if (parent == null)
        {
            return modifiers;
        }

        // Determine which modifier kind is the default
        SyntaxKind? defaultModifierKind = null;

        if (IsTopLevelTypeDeclaration(parent))
        {
            // Top-level types default to internal
            defaultModifierKind = SyntaxKind.InternalKeyword;
        }
        else if (IsMemberInTypeDeclaration(parent))
        {
            // Members of class/struct/record default to private
            defaultModifierKind = SyntaxKind.PrivateKeyword;
        }

        if (defaultModifierKind == null)
        {
            return modifiers;
        }

        // Only remove the default modifier if it's the only accessibility modifier present
        // (i.e., don't remove "private" from "private protected")
        var accessibilityModifiers = modifiers.Where(IsAccessibilityModifier).ToList();
        if (accessibilityModifiers.Count == 1 && accessibilityModifiers[0].IsKind(defaultModifierKind.Value))
        {
            var tokenToRemove = accessibilityModifiers[0];
            var remaining = modifiers.Where(m => m != tokenToRemove).ToList();

            // Transfer any leading trivia from the removed token to the next token
            if (remaining.Count > 0 && tokenToRemove.HasLeadingTrivia)
            {
                var existingTrivia = remaining[0].LeadingTrivia;
                var combinedTrivia = tokenToRemove.LeadingTrivia.AddRange(existingTrivia);
                remaining[0] = remaining[0].WithLeadingTrivia(combinedTrivia);
            }

            return new SyntaxTokenList(remaining);
        }

        return modifiers;
    }

    private static bool IsAccessibilityModifier(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.PublicKeyword)
            || token.IsKind(SyntaxKind.PrivateKeyword)
            || token.IsKind(SyntaxKind.ProtectedKeyword)
            || token.IsKind(SyntaxKind.InternalKeyword);
    }

    private static bool IsTopLevelTypeDeclaration(SyntaxNode node)
    {
        // A type declaration whose parent is a namespace or compilation unit
        return node is BaseTypeDeclarationSyntax or DelegateDeclarationSyntax
            && node.Parent is BaseNamespaceDeclarationSyntax
                or CompilationUnitSyntax;
    }

    private static bool IsMemberInTypeDeclaration(SyntaxNode node)
    {
        // A member whose parent is a type declaration (class, struct, record, interface)
        return node.Parent is TypeDeclarationSyntax;
    }

    private static Doc PrintWithSortedModifiers(
        in SyntaxTokenList modifiers,
        in SyntaxTokenList originalModifiers,
        PrintingContext context,
        Func<IReadOnlyList<SyntaxToken>, Doc> print
    )
    {
        if (modifiers.Count == 0)
        {
            return Doc.Null;
        }

        // reordering modifiers inside of #ifs can lead to code that doesn't compile
        var willReorderModifiers =
            modifiers.Count > 1
            && !modifiers.Skip(1).Any(o => o.LeadingTrivia.Any(p => p.IsDirective || p.IsComment()))
            && !modifiers[0].LeadingTrivia.Any(p => p.IsDirective);

        var sortedModifiers = modifiers.ToArray();
        var leadingToken = sortedModifiers.FirstOrDefault();
        if (willReorderModifiers)
        {
            Array.Sort(sortedModifiers, Comparer);
        }

        if (willReorderModifiers && !sortedModifiers.SequenceEqual(modifiers))
        {
            context.State.ReorderedModifiers = true;

            var leadingTrivia = leadingToken.LeadingTrivia;
            var leadingTokenIndex = Array.FindIndex(
                sortedModifiers,
                token => token == leadingToken
            );
            sortedModifiers[leadingTokenIndex] = sortedModifiers[leadingTokenIndex]
                .WithLeadingTrivia(new SyntaxTriviaList());
            sortedModifiers[0] = sortedModifiers[0].WithLeadingTrivia(leadingTrivia);
        }

        return print(sortedModifiers);
    }
}

