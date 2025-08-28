# scrbl
A simple command-line tool for managing your daily notes.

## Why scrbl?
After trying many different note-taking software and not finding a suitable match, I decided to keep my notes in a single, long Markdown file. However, I still had some issues:

1. Sometimes I would forget to save the file, which caused my notes to be lost at the end of the day
2. I wanted a faster way to add notes from the terminal to this file

## Features

### 🚀 Quick Note Management
```bash
scrbl write "I love neovim"                    # Add content to notes file
scrbl w "Quick note"                           # Short alias for write
scrbl append "Add to today's section"          # Append to current day
```

### 📝 Daily Templates
```bash
scrbl create --daily                           # Create daily entry
```

### 🛠️ Editor Integration
```bash
scrbl edit                                     # Open notes in your editor
scrbl edit --editor code                      # Use specific editor
```

### 📊 Status Dashboard
```bash
scrbl status                                   # View file stats, sync status, and activity
scrbl config                                   # Check current configuration
scrbl show                                     # Display recent notes with pretty formatting
```

### ☁️ Cloud Sync (Optional)
```bash
scrbl sync setup                               # Configure sync with remote server
scrbl sync                                     # Manual sync
scrbl sync --auto                              # Enable automatic syncing
```

The sync feature is designed to work with a custom .NET API that you would need to build and deploy yourself. I built my own API and deployed it on a VPS, but the sync functionality is optional - scrbl works perfectly fine as a local-only tool.

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

### Install as .NET Global Tool
```bash
# Clone and build
git clone https://github.com/ShiosOS/scrbl.git
cd scrbl
dotnet pack

# Install globally
dotnet tool install --global --add-source ./scrbl scrbl
```

### Update Tool
```bash
# Use the included update script
.\update-scrbl.ps1        # PowerShell (recommended)
.\quick-update.bat        # Batch file
./update-scrbl.sh         # Unix/Linux
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
scrbl write "Today I learned something new"
scrbl w "Review PR #123"                       # Short alias
scrbl append "Add to today's section"          # Append to current day
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
| `setup <path>` | Configure notes directory | `scrbl setup ~/notes` |
| `create --daily` | Create daily entry | `scrbl create -d` |
| `write <content>` or `w <content>` | Add content to notes | `scrbl w "New idea"` |
| `append <content>` | Add to today's section | `scrbl append "Meeting notes"` |
| `show [--lines N]` | Display recent notes | `scrbl show --lines 20` |
| `status` | View file stats and sync status | `scrbl status` |
| `config` | Show current configuration | `scrbl config` |
| `edit` | Open in text editor | `scrbl edit` |
| `edit --editor <name>` | Use specific editor | `scrbl edit -e code` |
| `sync` | Sync with remote server | `scrbl sync` |
| `sync setup` | Configure sync settings | `scrbl sync setup` |

## Daily Template

The daily template creates a simple structure:

```markdown
## YYYY.MM.DD
### Daily Summary

```

## File Structure
```
your-notes/
└── scrbl.md                    # Your main notes file
```

## Configuration

Scrbl stores its configuration in:
- **Windows**: `%APPDATA%\scrbl\config.json`
- **Linux/Mac**: `~/.config/scrbl/config.json`

### Configuration Options
- **Notes File Path**: Location of your main notes file
- **Sync Settings**: Optional remote server URL and API key for cloud sync (requires custom API deployment)
- **Editor Preference**: Your preferred text editor for the `edit` command

Use `scrbl config` to view your current settings or `scrbl status` for a complete overview including file statistics and sync status.

## Testing

The project includes comprehensive tests using xUnit:

```bash
dotnet test                                    # Run all tests
```
