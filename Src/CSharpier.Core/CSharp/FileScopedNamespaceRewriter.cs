using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.Core.CSharp;

/// <summary>
/// Rewrites block-scoped namespace declarations (namespace N { ... }) to
/// file-scoped namespace declarations (namespace N;) when the file contains
/// exactly one top-level namespace with no nested namespaces.
/// </summary>
internal class FileScopedNamespaceRewriter : CSharpSyntaxRewriter
{
    public static SyntaxNode Rewrite(CompilationUnitSyntax root)
    {
        // Only convert when there's exactly one top-level namespace member
        var namespaceDeclarations = root.Members.OfType<NamespaceDeclarationSyntax>().ToList();
        if (namespaceDeclarations.Count != 1)
        {
            return root;
        }

        var ns = namespaceDeclarations[0];

        // Don't convert if there are nested namespaces
        if (ns.Members.OfType<NamespaceDeclarationSyntax>().Any())
        {
            return root;
        }

        // Create a file-scoped namespace declaration
        var fileScopedNs = SyntaxFactory
            .FileScopedNamespaceDeclaration(ns.Name)
            .WithNamespaceKeyword(ns.NamespaceKeyword)
            .WithSemicolonToken(
                SyntaxFactory
                    .Token(SyntaxKind.SemicolonToken)
                    .WithTrailingTrivia(SyntaxFactory.ElasticLineFeed)
            )
            .WithExterns(ns.Externs)
            .WithUsings(ns.Usings)
            .WithMembers(UnindentMembers(ns.Members))
            .WithAttributeLists(ns.AttributeLists)
            .WithModifiers(ns.Modifiers);

        // Preserve leading trivia from the opening brace (comments between namespace line and brace)
        var leadingTrivia = ns.OpenBraceToken.LeadingTrivia;
        if (leadingTrivia.Any())
        {
            fileScopedNs = fileScopedNs.WithLeadingTrivia(
                ns.GetLeadingTrivia().AddRange(leadingTrivia)
            );
        }
        else
        {
            fileScopedNs = fileScopedNs.WithLeadingTrivia(ns.GetLeadingTrivia());
        }

        // Preserve trailing trivia from the closing brace
        var trailingTrivia = ns.CloseBraceToken.TrailingTrivia;
        if (trailingTrivia.Any())
        {
            var lastMember = fileScopedNs.Members.LastOrDefault();
            if (lastMember != null)
            {
                var updatedLastMember = lastMember.WithTrailingTrivia(
                    lastMember.GetTrailingTrivia().AddRange(trailingTrivia)
                );
                fileScopedNs = fileScopedNs.WithMembers(
                    fileScopedNs.Members.Replace(lastMember, updatedLastMember)
                );
            }
        }

        return root.WithMembers(root.Members.Replace(ns, fileScopedNs));
    }

    private static SyntaxList<MemberDeclarationSyntax> UnindentMembers(
        SyntaxList<MemberDeclarationSyntax> members
    )
    {
        // Remove one level of indentation from each member's leading trivia.
        // CSharpier will re-format with proper indentation anyway,
        // so we just need to make the syntax tree valid.
        return SyntaxFactory.List(members.Select(m => UnindentMember(m)));
    }

    private static MemberDeclarationSyntax UnindentMember(MemberDeclarationSyntax member)
    {
        var leadingTrivia = member.GetLeadingTrivia();
        var newTrivia = SyntaxFactory.TriviaList(
            leadingTrivia.Select(t =>
            {
                if (t.IsKind(SyntaxKind.WhitespaceTrivia))
                {
                    var text = t.ToString();
                    // Remove one level of indentation (4 spaces or 1 tab)
                    if (text.StartsWith("    "))
                    {
                        return SyntaxFactory.Whitespace(text[4..]);
                    }
                    if (text.StartsWith("\t"))
                    {
                        return SyntaxFactory.Whitespace(text[1..]);
                    }
                }
                return t;
            })
        );
        return member.WithLeadingTrivia(newTrivia);
    }
}
