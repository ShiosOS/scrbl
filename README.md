# scrbl

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)

A elegant command-line tool for managing your daily notes with optional cloud synchronization.

## Why scrbl?

After trying numerous note-taking applications without finding the perfect fit, scrbl was born from the idea of keeping notes in a single, long Markdown file. However, traditional file-based note-taking had some pain points:

- 💾 **Data Loss Risk**: Forgetting to save could lose an entire day's worth of notes
- ⚡ **Slow Workflow**: No quick way to append notes directly from the terminal
- 🔄 **No Sync**: Difficulty accessing notes across multiple devices

scrbl solves these problems with automatic syncing, structured daily templates, and terminal integration.

## Features

### 🚀 Quick Note Management
```bash
scrbl append "I love neovim"                   # Add content to today's section
scrbl a "Quick note"                           # Short alias for append
```

### 📝 Daily Templates
```bash
scrbl create --daily                           # Create structured daily entry
scrbl create -d                                # Short alias for daily creation
```

### 🛠️ Editor Integration
```bash
scrbl edit                                     # Open notes in your default editor
scrbl edit --editor code                      # Use specific editor (VS Code, vim, etc.)
```

### 📊 Comprehensive Dashboard
```bash
scrbl status                                   # View file stats, sync status, and activity metrics
scrbl config                                   # Check current configuration with helpful tips
scrbl show                                     # Display recent notes with elegant formatting
```

### ☁️ Cloud Sync (Optional)
```bash
scrbl setup                                    # Configure sync during initial setup
scrbl sync                                     # Manual sync with remote server
```

The sync feature works with a custom Web API that you can deploy on any server or cloud platform. The sync functionality is entirely optional - scrbl works perfectly as a local-only tool.

#### API Requirements

If you want to implement sync functionality, your API needs these endpoints:

**Authentication**: All endpoints expect an `X-Api-Key` header for authentication.

| Method | Endpoint | Description | Request/Response |
|--------|----------|-------------|-----------------|
| `GET` | `/api/notes/metadata` | Get file metadata | Returns: `{ "lastModified": "2023-12-01T10:30:00Z" }` |
| `GET` | `/api/notes/content` | Download notes file | Returns: Raw file content as string |
| `POST` | `/api/notes/content` | Upload notes file | Body: `{ "content": "file content here" }` |

**Error Handling**:
- Return `401 Unauthorized` for invalid API keys
- Return appropriate HTTP status codes for other errors

## Installation

### Prerequisites
- [.NET 9.0 SDK or later](https://dotnet.microsoft.com/download)

### Install as .NET Global Tool

```bash
# Clone the repository
git clone https://github.com/ShiosOS/scrbl.git
cd scrbl

# Build and pack the tool
dotnet pack --configuration Release

# Install globally
dotnet tool install --global --add-source ./scrbl scrbl
```

### Verification

```bash
# Verify installation
scrbl --version
scrbl --help
```

## Quick Start

### 1. Setup
```bash
scrbl setup /path/to/notes/directory
```

### 2. Create Daily Entry
```bash
scrbl create --daily
```

### 3. Add Content
```bash
scrbl append "Today I learned something new"   # Add to today's section
scrbl a "Review PR #123"                       # Short alias for append
```

### 4. Check Your Progress
```bash
scrbl status                                   # View file stats and activity
scrbl show                                     # See recent notes
```

### 5. Edit When Needed
```bash
scrbl edit
```

## Commands

| Command | Description | Example |
|---------|-------------|---------|
| `setup <path>` | Configure notes directory and optional sync | `scrbl setup ~/notes` |
| `create --daily` | Create daily entry template | `scrbl create -d` |
| `append <content>` or `a <content>` | Add to today's section | `scrbl append "Meeting notes"` |
| `show [--lines N]` | Display recent notes with formatting | `scrbl show --lines 20` |
| `status` | View comprehensive file and sync dashboard | `scrbl status` |
| `config` | Show current configuration settings | `scrbl config` |
| `edit [--editor <name>]` | Open notes in text editor | `scrbl edit --editor code` |
| `sync` | Manually sync with remote server | `scrbl sync` |

## Templates

### Daily Template Structure

When you run `scrbl create --daily`, scrbl generates:

```markdown
## 2024.08.28
### Daily Summary

```

This creates a consistent, date-organized structure that makes it easy to navigate your notes chronologically.

### File Organization

```
your-notes-directory/
└── scrbl.md                    # Your main notes file (auto-created)
```

The notes file uses a simple Markdown format that's readable in any text editor and works great with version control systems like Git.

## Configuration

### Configuration File Locations

| Platform | Configuration Path |
|----------|-------------------|
| **Windows** | `%APPDATA%\scrbl\config.json` |
| **Linux/Mac** | `~/.config/scrbl/config.json` |

### Configuration Options

| Setting | Description | Example |
|---------|-------------|----------|
| **Notes File Path** | Location of your main notes file | `~/Documents/scrbl.md` |
| **Server URL** | Remote server endpoint for sync | `https://api.example.com` |
| **API Key** | Authentication key for sync service | `your-secret-api-key` |
| **Editor** | Preferred text editor for `edit` command | `code`, `vim`, `notepad` |

### View Configuration

```bash
scrbl config                                   # View current settings with helpful tips
scrbl status                                   # Complete dashboard with file stats and sync status
```

## Development

### Building from Source

```bash
# Clone and build
git clone https://github.com/ShiosOS/scrbl.git
cd scrbl
dotnet build --configuration Release
```

### Running Tests

The project includes comprehensive tests using xUnit:

```bash
dotnet test                                    # Run all tests
dotnet test --verbosity normal                 # Run with detailed output
```

### Project Structure

```
scrbl/
├── Commands/           # CLI command implementations
├── Managers/           # Configuration and file management
├── Services/           # Sync and external services
├── Tests/              # Unit tests
├── scrbl.csproj        # Project configuration
├── update-scrbl.ps1    # PowerShell update script
└── README.md           # Documentation
```

## Contributing

Contributions are welcome! Here's how you can help:

### Reporting Issues

- Use the [GitHub Issues](https://github.com/ShiosOS/scrbl/issues) page
- Include your OS, .NET version, and scrbl version
- Provide steps to reproduce the issue
- Include relevant error messages or logs

### Development Setup

1. Fork the repository
2. Clone your fork: `git clone https://github.com/YOUR_USERNAME/scrbl.git`
3. Create a feature branch: `git checkout -b feature/amazing-feature`
4. Make your changes and add tests
5. Run tests: `dotnet test`
6. Commit your changes: `git commit -m 'Add amazing feature'`
7. Push to your branch: `git push origin feature/amazing-feature`
8. Open a Pull Request

### Code Style

- Follow C# coding conventions
- Add unit tests for new features
- Update documentation as needed
- Use meaningful commit messages


## Acknowledgments

- Built with [.NET 9.0](https://dotnet.microsoft.com/)
- CLI styling powered by [Spectre.Console](https://spectreconsole.net/)
- Inspired by the need for simple, effective note-taking workflows

## Support

If you find scrbl helpful, consider:

- ⭐ Starring the repository
- 🐛 Reporting bugs
- 💡 Suggesting new features
- 🔧 Contributing code or documentation

---

**Happy scrbling!** 📝
