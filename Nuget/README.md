cs-fmt is a configurable C# code formatter that respects your `.editorconfig` settings. Fork of [CSharpier](https://github.com/belav/csharpier).

### Supported `.editorconfig` Settings

| Setting | Values | Effect |
|---------|--------|--------|
| `csharp_new_line_before_open_brace` | `none` | K&R brace style |
| `csharp_prefer_braces` | `true` | Mandatory braces |
| `dotnet_style_require_accessibility_modifiers` | `omit_if_default` | Remove redundant modifiers |
| `csharp_style_namespace_declarations` | `file_scoped` | File-scoped namespaces |

### Quick Start
```bash
dotnet tool install cs-fmt -g
```
```bash
cs-fmt format .
```

### Before
```c#
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

### After (with .editorconfig above)
```c#
namespace MyApp;

class Program {
    void DoWork() {
        if (condition) {
            Execute();
        }
    }
}
```
