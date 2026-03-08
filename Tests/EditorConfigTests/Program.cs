using CSharpier.Core;

int passed = 0;
int failed = 0;

async Task<string> FormatAsync(string code, CodeFormatterOptions? options = null)
{
    var result = await CodeFormatterOptions.FormatAsync(code, options);
    return result.Code;
}

CodeFormatterOptions KROptions() => new() { BraceNewLine = false };
CodeFormatterOptions BracesOptions() => new() { PreferBraces = true };
CodeFormatterOptions KRBracesOptions() => new() { BraceNewLine = false, PreferBraces = true };
CodeFormatterOptions OmitModifiersOptions() => new() { OmitDefaultAccessibilityModifiers = true };

void Assert(bool condition, string testName, string? detail = null)
{
    if (condition) { passed++; Console.WriteLine($"  PASS: {testName}"); }
    else { failed++; Console.WriteLine($"  FAIL: {testName}{(detail != null ? " - " + detail : "")}"); }
}

void AssertContains(string actual, string expected, string testName) =>
    Assert(actual.Contains(expected), testName, $"Expected to contain '{expected}' but got:\n{actual}");

void AssertNotContains(string actual, string unexpected, string testName) =>
    Assert(!actual.Contains(unexpected), testName, $"Should not contain '{unexpected}' but got:\n{actual}");

// ==========================================================
Console.WriteLine("\n=== K&R Brace Placement ===");

var r = await FormatAsync("class Foo\n{\n}\n", KROptions());
AssertContains(r, "class Foo {", "KR_Class_Declaration");

r = await FormatAsync("class Foo\n{\n    void Bar()\n    {\n    }\n}\n", KROptions());
AssertContains(r, "void Bar() { }", "KR_Method_Declaration");

r = await FormatAsync("class C\n{\n    void M()\n    {\n        if (true)\n        {\n            var x = 1;\n        }\n        else\n        {\n            var x = 2;\n        }\n    }\n}\n", KROptions());
AssertContains(r, "} else {", "KR_If_Else");

r = await FormatAsync("class C\n{\n    void M()\n    {\n        try\n        {\n            var x = 1;\n        }\n        catch (Exception ex)\n        {\n            var x = 2;\n        }\n        finally\n        {\n            var x = 3;\n        }\n    }\n}\n", KROptions());
AssertContains(r, "} catch", "KR_Try_Catch");
AssertContains(r, "} finally", "KR_Try_Finally");

r = await FormatAsync("class C\n{\n    void M()\n    {\n        for (int i = 0; i < 10; i++)\n        {\n            var x = i;\n        }\n    }\n}\n", KROptions());
AssertContains(r, "i++) {", "KR_For_Loop");

r = await FormatAsync("class C\n{\n    void M()\n    {\n        while (true)\n        {\n            break;\n        }\n    }\n}\n", KROptions());
AssertContains(r, "while (true) {", "KR_While_Loop");

r = await FormatAsync("class C\n{\n    void M()\n    {\n        foreach (var x in items)\n        {\n            var y = x;\n        }\n    }\n}\n", KROptions());
AssertContains(r, "items) {", "KR_Foreach_Loop");

r = await FormatAsync("class C\n{\n    void M()\n    {\n    }\n}\n");
AssertContains(r, "class C\n{", "Allman_Default");

// ==========================================================
Console.WriteLine("\n=== Mandatory Braces ===");

r = await FormatAsync("class C\n{\n    void M()\n    {\n        if (true)\n            var x = 1;\n    }\n}\n", BracesOptions());
Assert(r.Count(c => c == '{') >= 3, "Braces_If_SingleStatement", $"braces={r.Count(c => c == '{')}");

r = await FormatAsync("class C\n{\n    void M()\n    {\n        if (true)\n            var x = 1;\n        else\n            var x = 2;\n    }\n}\n", BracesOptions());
Assert(r.Count(c => c == '{') >= 4, "Braces_Else_SingleStatement", $"braces={r.Count(c => c == '{')}");

r = await FormatAsync("class C\n{\n    void M()\n    {\n        for (int i = 0; i < 10; i++)\n            var x = i;\n    }\n}\n", BracesOptions());
Assert(r.Count(c => c == '{') >= 3, "Braces_For_SingleStatement");

r = await FormatAsync("class C\n{\n    void M()\n    {\n        while (true)\n            break;\n    }\n}\n", BracesOptions());
Assert(r.Count(c => c == '{') >= 3, "Braces_While_SingleStatement");

r = await FormatAsync("class C\n{\n    void M()\n    {\n        foreach (var x in items)\n            var y = x;\n    }\n}\n", BracesOptions());
Assert(r.Count(c => c == '{') >= 3, "Braces_Foreach_SingleStatement");

var braced = "class C\n{\n    void M()\n    {\n        if (true)\n        {\n            var x = 1;\n        }\n    }\n}\n";
var r1 = await FormatAsync(braced, BracesOptions());
var r2 = await FormatAsync(braced);
Assert(r1 == r2, "Braces_AlreadyBraced_Unchanged");

r = await FormatAsync("class C\n{\n    void M()\n    {\n        if (true)\n            var x = 1;\n    }\n}\n");
Assert(r.Count(c => c == '{') == 2, "Braces_Default_NoForce");

r = await FormatAsync("class C\n{\n    void M()\n    {\n        if (true)\n            var x = 1;\n    }\n}\n", KRBracesOptions());
AssertContains(r, "if (true) {", "KR_Braces_Combined");

// ==========================================================
Console.WriteLine("\n=== Modifier Removal ===");

r = await FormatAsync("class C\n{\n    private void M()\n    {\n    }\n}\n", OmitModifiersOptions());
AssertNotContains(r, "private void", "Modifier_Private_Removed_From_Method");
AssertContains(r, "void M()", "Modifier_Private_Removed_Method_HasMethod");

r = await FormatAsync("class C\n{\n    private int x;\n}\n", OmitModifiersOptions());
AssertNotContains(r, "private int", "Modifier_Private_Removed_From_Field");
AssertContains(r, "int x;", "Modifier_Private_Removed_Field_HasField");

r = await FormatAsync("class C\n{\n    private int X { get; set; }\n}\n", OmitModifiersOptions());
AssertNotContains(r, "private int X", "Modifier_Private_Removed_From_Property");
AssertContains(r, "int X", "Modifier_Private_Removed_Prop_HasProp");

r = await FormatAsync("namespace N\n{\n    internal class C\n    {\n    }\n}\n", OmitModifiersOptions());
AssertNotContains(r, "internal class", "Modifier_Internal_Removed_TopLevel");
AssertContains(r, "class C", "Modifier_Internal_Removed_HasClass");

r = await FormatAsync("class C\n{\n    public void M()\n    {\n    }\n}\n", OmitModifiersOptions());
AssertContains(r, "public void M()", "Modifier_Public_Not_Removed");

r = await FormatAsync("class C\n{\n    protected void M()\n    {\n    }\n}\n", OmitModifiersOptions());
AssertContains(r, "protected void M()", "Modifier_Protected_Not_Removed");

r = await FormatAsync("class C\n{\n    private static void M()\n    {\n    }\n}\n", OmitModifiersOptions());
AssertNotContains(r, "private", "Modifier_Private_Static_NoPrivate");
AssertContains(r, "static void M()", "Modifier_Private_Static_KeepsStatic");

r = await FormatAsync("class C\n{\n    private void M()\n    {\n    }\n}\n");
AssertContains(r, "private void M()", "Modifier_Default_NoRemoval");

r = await FormatAsync("class C\n{\n    private protected void M()\n    {\n    }\n}\n", OmitModifiersOptions());
AssertContains(r, "private protected void M()", "Modifier_PrivateProtected_NotRemoved");

r = await FormatAsync("class Outer\n{\n    internal class Inner\n    {\n    }\n}\n", OmitModifiersOptions());
AssertContains(r, "internal class Inner", "Modifier_Internal_NestedNotRemoved");

r = await FormatAsync("class Outer\n{\n    private class Inner\n    {\n    }\n}\n", OmitModifiersOptions());
AssertNotContains(r, "private class Inner", "Modifier_Private_NestedRemoved");
AssertContains(r, "class Inner", "Modifier_Private_Nested_HasClass");

// ==========================================================
Console.WriteLine($"\n==================================================");
Console.WriteLine($"Results: {passed} passed, {failed} failed, {passed + failed} total");
Console.WriteLine($"==================================================");
return failed > 0 ? 1 : 0;
