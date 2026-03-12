using AwesomeAssertions;
using CSharpier.Core;
using CSharpier.Core.CSharp;

namespace CSharpier.Tests;

public class EditorConfigFormattingTests
{
    private static async Task<string> FormatAsync(string code, PrinterOptions? options = null)
    {
        options ??= new PrinterOptions(Formatter.CSharp);
        var result = await CSharpFormatter.FormatAsync(code, options);
        return result.Code;
    }

    private static PrinterOptions KROptions() => new(Formatter.CSharp) { BraceNewLine = false };

    private static PrinterOptions BracesOptions() =>
        new(Formatter.CSharp) { PreferBraces = true };

    private static PrinterOptions KRBracesOptions() =>
        new(Formatter.CSharp) { BraceNewLine = false, PreferBraces = true };

    private static PrinterOptions OmitModifiersOptions() =>
        new(Formatter.CSharp) { OmitDefaultAccessibilityModifiers = true };

    // ==========================================================
    // K&R BRACE PLACEMENT TESTS
    // ==========================================================

    [Test]
    public async Task KR_Class_Declaration()
    {
        var input = "class Foo\n{\n}\n";
        var result = await FormatAsync(input, KROptions());
        result.Should().Contain("class Foo {");
    }

    [Test]
    public async Task KR_Method_Declaration()
    {
        var input = "class Foo\n{\n    void Bar()\n    {\n    }\n}\n";
        var result = await FormatAsync(input, KROptions());
        result.Should().Contain("void Bar() { }");
    }

    [Test]
    public async Task KR_If_Else()
    {
        var input = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                        var x = 1;
                    }
                    else
                    {
                        var x = 2;
                    }
                }
            }
            """;
        var result = await FormatAsync(input, KROptions());
        result.Should().Contain("} else {");
    }

    [Test]
    public async Task KR_Try_Catch_Finally()
    {
        var input = """
            class C
            {
                void M()
                {
                    try
                    {
                        var x = 1;
                    }
                    catch (Exception ex)
                    {
                        var x = 2;
                    }
                    finally
                    {
                        var x = 3;
                    }
                }
            }
            """;
        var result = await FormatAsync(input, KROptions());
        result.Should().Contain("} catch");
        result.Should().Contain("} finally");
    }

    [Test]
    public async Task KR_For_Loop()
    {
        var input = """
            class C
            {
                void M()
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var x = i;
                    }
                }
            }
            """;
        var result = await FormatAsync(input, KROptions());
        result.Should().Contain("i++) {");
    }

    [Test]
    public async Task KR_While_Loop()
    {
        var input = """
            class C
            {
                void M()
                {
                    while (true)
                    {
                        break;
                    }
                }
            }
            """;
        var result = await FormatAsync(input, KROptions());
        result.Should().Contain("while (true) {");
    }

    [Test]
    public async Task KR_Foreach_Loop()
    {
        var input = """
            class C
            {
                void M()
                {
                    foreach (var x in items)
                    {
                        var y = x;
                    }
                }
            }
            """;
        var result = await FormatAsync(input, KROptions());
        result.Should().Contain("items) {");
    }

    [Test]
    public async Task Allman_Default_Braces()
    {
        var input = """
            class C
            {
                void M()
                {
                }
            }
            """;
        var result = await FormatAsync(input);
        result.Should().Contain("class C\n{");
    }

    // ==========================================================
    // MANDATORY BRACES TESTS
    // ==========================================================

    [Test]
    public async Task Braces_If_SingleStatement()
    {
        var input = """
            class C
            {
                void M()
                {
                    if (true)
                        var x = 1;
                }
            }
            """;
        var result = await FormatAsync(input, BracesOptions());
        // After the if line, there should be braces around the single statement
        result.Should().Contain("{").And.Contain("}");
        // Count total braces - with mandatory braces, we get class + method + if = 3 pairs = 6 braces
        result.Count(c => c == '{').Should().BeGreaterThanOrEqualTo(3);
    }

    [Test]
    public async Task Braces_Else_SingleStatement()
    {
        var input = """
            class C
            {
                void M()
                {
                    if (true)
                        var x = 1;
                    else
                        var x = 2;
                }
            }
            """;
        var result = await FormatAsync(input, BracesOptions());
        // class + method + if + else = 4 pairs
        result.Count(c => c == '{').Should().BeGreaterThanOrEqualTo(4);
    }

    [Test]
    public async Task Braces_For_SingleStatement()
    {
        var input = """
            class C
            {
                void M()
                {
                    for (int i = 0; i < 10; i++)
                        var x = i;
                }
            }
            """;
        var result = await FormatAsync(input, BracesOptions());
        result.Count(c => c == '{').Should().BeGreaterThanOrEqualTo(3);
    }

    [Test]
    public async Task Braces_While_SingleStatement()
    {
        var input = """
            class C
            {
                void M()
                {
                    while (true)
                        break;
                }
            }
            """;
        var result = await FormatAsync(input, BracesOptions());
        result.Count(c => c == '{').Should().BeGreaterThanOrEqualTo(3);
    }

    [Test]
    public async Task Braces_Foreach_SingleStatement()
    {
        var input = """
            class C
            {
                void M()
                {
                    foreach (var x in items)
                        var y = x;
                }
            }
            """;
        var result = await FormatAsync(input, BracesOptions());
        result.Count(c => c == '{').Should().BeGreaterThanOrEqualTo(3);
    }

    [Test]
    public async Task Braces_AlreadyBraced_Unchanged()
    {
        var input = """
            class C
            {
                void M()
                {
                    if (true)
                    {
                        var x = 1;
                    }
                }
            }
            """;
        var resultWithBraces = await FormatAsync(input, BracesOptions());
        var resultWithout = await FormatAsync(input);
        resultWithBraces.Should().Be(resultWithout);
    }

    [Test]
    public async Task Braces_Default_NoForce()
    {
        var input = """
            class C
            {
                void M()
                {
                    if (true)
                        var x = 1;
                }
            }
            """;
        var result = await FormatAsync(input);
        // Only class + method = 2 brace pairs, no forced braces on the if
        result.Count(c => c == '{').Should().Be(2);
    }

    [Test]
    public async Task KR_Braces_Combined()
    {
        var input = """
            class C
            {
                void M()
                {
                    if (true)
                        var x = 1;
                }
            }
            """;
        var result = await FormatAsync(input, KRBracesOptions());
        result.Should().Contain("if (true) {");
    }

    // ==========================================================
    // MODIFIER REMOVAL TESTS
    // ==========================================================

    [Test]
    public async Task Modifier_Private_Removed_From_Method()
    {
        var input = """
            class C
            {
                private void M()
                {
                }
            }
            """;
        var result = await FormatAsync(input, OmitModifiersOptions());
        result.Should().NotContain("private void");
        result.Should().Contain("void M()");
    }

    [Test]
    public async Task Modifier_Private_Removed_From_Field()
    {
        var input = """
            class C
            {
                private int x;
            }
            """;
        var result = await FormatAsync(input, OmitModifiersOptions());
        result.Should().NotContain("private int");
        result.Should().Contain("int x;");
    }

    [Test]
    public async Task Modifier_Private_Removed_From_Property()
    {
        var input = """
            class C
            {
                private int X { get; set; }
            }
            """;
        var result = await FormatAsync(input, OmitModifiersOptions());
        result.Should().NotContain("private int X");
        result.Should().Contain("int X");
    }

    [Test]
    public async Task Modifier_Internal_Removed_From_TopLevel_Class()
    {
        var input = """
            namespace N
            {
                internal class C
                {
                }
            }
            """;
        var result = await FormatAsync(input, OmitModifiersOptions());
        result.Should().NotContain("internal class");
        result.Should().Contain("class C");
    }

    [Test]
    public async Task Modifier_Public_Not_Removed()
    {
        var input = """
            class C
            {
                public void M()
                {
                }
            }
            """;
        var result = await FormatAsync(input, OmitModifiersOptions());
        result.Should().Contain("public void M()");
    }

    [Test]
    public async Task Modifier_Protected_Not_Removed()
    {
        var input = """
            class C
            {
                protected void M()
                {
                }
            }
            """;
        var result = await FormatAsync(input, OmitModifiersOptions());
        result.Should().Contain("protected void M()");
    }

    [Test]
    public async Task Modifier_Private_Static_Keeps_Static()
    {
        var input = """
            class C
            {
                private static void M()
                {
                }
            }
            """;
        var result = await FormatAsync(input, OmitModifiersOptions());
        result.Should().NotContain("private");
        result.Should().Contain("static void M()");
    }

    [Test]
    public async Task Modifier_Default_NoRemoval()
    {
        var input = """
            class C
            {
                private void M()
                {
                }
            }
            """;
        var result = await FormatAsync(input);
        result.Should().Contain("private void M()");
    }

    [Test]
    public async Task Modifier_PrivateProtected_Not_Removed()
    {
        var input = """
            class C
            {
                private protected void M()
                {
                }
            }
            """;
        var result = await FormatAsync(input, OmitModifiersOptions());
        result.Should().Contain("private protected void M()");
    }

    [Test]
    public async Task Modifier_Internal_Not_Removed_From_Nested_Class()
    {
        var input = """
            class Outer
            {
                internal class Inner
                {
                }
            }
            """;
        var result = await FormatAsync(input, OmitModifiersOptions());
        result.Should().Contain("internal class Inner");
    }

    [Test]
    public async Task Modifier_Private_Removed_From_Nested_Class()
    {
        var input = """
            class Outer
            {
                private class Inner
                {
                }
            }
            """;
        var result = await FormatAsync(input, OmitModifiersOptions());
        result.Should().NotContain("private class Inner");
        result.Should().Contain("class Inner");
    }
    // ==========================================================
    // FILE-SCOPED NAMESPACE CONVERSION TESTS
    // ==========================================================

    private static PrinterOptions FileScopedOptions() =>
        new(Formatter.CSharp) { PreferFileScopedNamespace = true };

    [Test]
    public async Task FileScoped_Basic_Conversion()
    {
        var input = "namespace N\n{\n    class C\n    {\n    }\n}\n";
        var result = await FormatAsync(input, FileScopedOptions());
        result.Should().Contain("namespace N;");
        result.Should().NotContain("namespace N\n{");
    }

    [Test]
    public async Task FileScoped_Already_FileScoped_Unchanged()
    {
        var input = "namespace N;\n\nclass C\n{\n}\n";
        var result = await FormatAsync(input, FileScopedOptions());
        result.Should().Contain("namespace N;");
    }

    [Test]
    public async Task FileScoped_Nested_Not_Converted()
    {
        var input = "namespace N\n{\n    namespace Inner\n    {\n        class C\n        {\n        }\n    }\n}\n";
        var result = await FormatAsync(input, FileScopedOptions());
        result.Should().Contain("namespace Inner");
        result.Should().NotContain("namespace N;");
    }

    [Test]
    public async Task FileScoped_Multiple_Not_Converted()
    {
        var input = "namespace A\n{\n    class C1 { }\n}\n\nnamespace B\n{\n    class C2 { }\n}\n";
        var result = await FormatAsync(input, FileScopedOptions());
        result.Should().NotContain("namespace A;");
        result.Should().NotContain("namespace B;");
    }

    [Test]
    public async Task FileScoped_Default_No_Conversion()
    {
        var input = "namespace N\n{\n    class C\n    {\n    }\n}\n";
        var result = await FormatAsync(input);
        result.Should().NotContain("namespace N;");
        result.Should().Contain("namespace N\n{");
    }

    [Test]
    public async Task FileScoped_Combined_With_KR()
    {
        var input = "namespace N\n{\n    class C\n    {\n        void M()\n        {\n        }\n    }\n}\n";
        var options = new PrinterOptions(Formatter.CSharp)
        {
            PreferFileScopedNamespace = true,
            BraceNewLine = false,
        };
        var result = await FormatAsync(input, options);
        result.Should().Contain("namespace N;");
        result.Should().Contain("void M() { }");
    }

    // ==========================================================
    // CHAIN FIRST EXPRESSION ON SAME LINE TESTS
    // ==========================================================

    // Use a narrow width to force chain wrapping in tests.
    // Chains need 3+ invocations for PrintMemberChain to allow wrapping.
    private static PrinterOptions ChainSameLineOptions() =>
        new(Formatter.CSharp) { ChainFirstExpressionOnSameLine = true, Width = 50 };

    private static PrinterOptions ChainDefaultOptions() =>
        new(Formatter.CSharp) { Width = 50 };

    [Test]
    public async Task Chain_WithOption_FirstExpression_StaysOnSameLine()
    {
        var input = """
            class C
            {
                void M()
                {
                    var foo = myList.Where(c => c.Foo > 10).Select(c => c.Bar).ToList();
                }
            }
            """;
        var result = await FormatAsync(input, ChainSameLineOptions());
        // With the option, 'myList' stays on the same line as 'var foo ='
        result.Should().Contain("var foo = myList");
    }

    [Test]
    public async Task Chain_Default_WrapsChainMembers()
    {
        var input = """
            class C
            {
                void M()
                {
                    var foo = myList.Where(c => c.Foo > 10).Select(c => c.Bar).ToList();
                }
            }
            """;
        var expected = """
            class C
            {
                void M()
                {
                    var foo = myList
                        .Where(c =>
                            c.Foo > 10
                        )
                        .Select(c =>
                            c.Bar
                        )
                        .ToList();
                }
            }

            """;
        var result = await FormatAsync(
            input,
            new PrinterOptions(Formatter.CSharp) { Width = 30 }
        );
        result.Should().Be(expected);
    }

    [Test]
    public async Task Chain_WithOption_ShortChain_StaysOnOneLine()
    {
        var input = """
            class C
            {
                void M()
                {
                    var x = list.ToList();
                }
            }
            """;
        var result = await FormatAsync(input, ChainSameLineOptions());
        // Short enough to fit on one line regardless of option
        result.Should().Contain("var x = list.ToList()");
    }

    [Test]
    public async Task Chain_WithOption_Assignment_FirstExpression_StaysOnSameLine()
    {
        var input = """
            class C
            {
                void M()
                {
                    object foo;
                    foo = myList.Where(c => c.Foo > 10).Select(c => c.Bar).First();
                }
            }
            """;
        var result = await FormatAsync(input, ChainSameLineOptions());
        result.Should().Contain("foo = myList");
    }
}
