# cs-fmt

A configurable C# code formatter that respects your `.editorconfig` settings. Fork of [CSharpier](https://github.com/belav/csharpier).

Unlike CSharpier (which is deliberately opinionated with minimal options), **cs-fmt** reads your `.editorconfig` and enforces the style *you* choose.

Note: This was created via Claude 4.6 Opus as a test project to see how capable it is. None of the code has been written manually by me (Isak).

## Supported `.editorconfig` Settings

| Setting | Values | Effect |
|---------|--------|--------|
| `csharp_new_line_before_open_brace` | `none` | K&R brace style (same line) |
| `csharp_prefer_braces` | `true` | Wrap single-statement bodies in `{ }` |
| `dotnet_style_require_accessibility_modifiers` | `omit_if_default` | Remove redundant `private`, `internal`, etc. |
| `csharp_style_namespace_declarations` | `file_scoped` | Convert `namespace N { }` → `namespace N;` |
| `csharpier_chain_first_expression_on_same_line` | `true` | Keep first expression of a method chain on the same line as `=` |

Plus all standard CSharpier settings: `indent_size`, `indent_style`, `max_line_length`, `end_of_line`.

## Quick Start

Install globally:
```bash
dotnet tool install cs-fmt -g --prerelease
# After release: dotnet tool install cs-fmt -g
```

Format a directory:
```bash
cs-fmt format .
```

Format a single file:
```bash
cs-fmt format MyFile.cs
```

## Example `.editorconfig`

```ini
root = true

[*.cs]
indent_size = 4
end_of_line = lf
csharp_new_line_before_open_brace = none
csharp_prefer_braces = true:error
dotnet_style_require_accessibility_modifiers = omit_if_default:error
csharp_style_namespace_declarations = file_scoped:error
csharpier_chain_first_expression_on_same_line = true
```

### Method Chain Formatting

With `csharpier_chain_first_expression_on_same_line = true`:
```csharp
// Before (default)
var foo =
    myList
        .Where(c => c.Foo > 10)
        .Select(c => c.Bar);

// After (with option enabled)
var foo = myList
    .Where(c => c.Foo > 10)
    .Select(c => c.Bar);
```

## Before / After

### Before (default CSharpier style)
```csharp
namespace MyApp
{
    internal class Program
    {
        private void DoWork()
        {
            if (condition)
                Execute();
        }
    }
}
```

### After (with the `.editorconfig` above)
```csharp
namespace MyApp;

class Program {
    void DoWork() {
        if (condition) {
            Execute();
        }
    }
}
```

## Attribution

cs-fmt is a fork of [CSharpier](https://github.com/belav/csharpier) by Bela VanderVoort, licensed under the MIT License. The core formatting engine (Roslyn parsing + Prettier-style document printing) comes from CSharpier.

## License

MIT