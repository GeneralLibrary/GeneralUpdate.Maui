# GeneralUpdate.Maui

This project is a subproject of GeneralUpdate, designed to be compatible with .NET MAUI.

| Platform | Support | Framework version |
| -------- | ------- | ----------------- |
| Android  | Yes     | .NET10            |
| Windows  | -       | -                 |
| iOS      | -       | -                 |
| Mac      | -       | -                 |

## Projects

- `GeneralUpdate.Maui.Android`: UI-less Android auto-update core for .NET MAUI, including:
  - Update discovery from external metadata
  - Resumable APK download via `HttpClient` and HTTP range requests
  - SHA256 integrity verification
  - Android package installation triggering via `FileProvider` and system installer intent
  - Workflow state and event notifications for update lifecycle and download statistics
