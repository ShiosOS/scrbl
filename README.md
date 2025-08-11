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
```

### 4. Edit When Needed
```bash
scrbl edit
```

## Commands

| Command | Description | Example |
|---------|-------------|---------|
| `setup <path>` | Configure notes directory | `scrbl setup ~/notes` |
| `create --daily` | Create daily entry | `scrbl create -d` |
| `write <content>` or `w <content>` | Add content to notes | `scrbl w "New idea"` |
| `edit` | Open in text editor | `scrbl edit` |
| `edit --editor <name>` | Use specific editor | `scrbl edit -e code` |

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
