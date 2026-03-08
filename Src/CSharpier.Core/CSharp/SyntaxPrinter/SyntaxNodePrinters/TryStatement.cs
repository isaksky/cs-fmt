using CSharpier.Core.DocTypes;
using CSharpier.Core.Utilities;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CSharpier.Core.CSharp.SyntaxPrinter.SyntaxNodePrinters;

internal static class TryStatement
{
    public static Doc Print(TryStatementSyntax node, PrintingContext context)
    {
        var docs = new DocListBuilder(8);
        var separator = context.Options.BraceNewLine ? (Doc)Doc.HardLine : (Doc)" ";
        docs.Add(ExtraNewLines.Print(node));
        docs.Add(AttributeLists.Print(node, node.AttributeLists, context));
        docs.Add(Token.Print(node.TryKeyword, context));
        docs.Add(Block.Print(node.Block, context));
        docs.Add(node.Catches.Any() ? separator : Doc.Null);
        docs.Add(Doc.Join(separator, node.Catches.Select(o => CatchClause.Print(o, context))));

        if (node.Finally != null)
        {
            docs.Add(separator, FinallyClause.Print(node.Finally, context));
        }
        return Doc.Concat(ref docs);
    }
}
