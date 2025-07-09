# scrbl
A command-line tool for managing your daily notes.

## Why scrbl?
After trying many different note-taking software and not finding a suitable match, I decided to keep my notes in a single, long Markdown file. However, I still had some issues:

1. Sometimes I would forget to save the file, which caused my notes to be lost at the end of the day
2. I wanted a faster way to add notes from the terminal to this file
3. Finding specific content in large note files was becoming difficult

## Features

### 🚀 Quick Note Management
```bash
scrbl write "I love neovim"                    # Add to current day's notes
scrbl write "Meeting notes" --section tasks    # Add to specific section
```

### 📝 Smart Templates
```bash
scrbl create --daily                           # Create daily template
scrbl create --template meeting                # Use custom templates
```

### ⚡ Fast Indexing
- Intelligent content indexing for fast searches
- Automatic header and section detection
- Word-level indexing for quick lookups
- Persistent index caching for performance

### 🎯 Organized Structure
- Daily entries with `## YYYY.MM.DD` headers
- Section support with `### Section Name`
- Automatic content insertion at the right location
- Template-based consistent formatting

### 🛠️ Editor Integration
```bash
scrbl edit                                     # Open notes in your editor
scrbl edit --editor code                      # Use specific editor
```

## Installation

### Install as .NET Global Tool
```bash
# Clone and build
git clone <your-repo-url>
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

### 2. Create Daily Structure
```bash
scrbl create --daily
```

### 3. Add Content
```bash
scrbl write "Today I learned about indexing"
scrbl write "Review PR #123"
```

### 4. Edit When Needed
```bash
scrbl edit
```

## Commands

| Command | Description | Example |
|---------|-------------|---------|
| `setup <path>` | Configure notes directory | `scrbl setup ~/notes` |
| `create --daily` | Create daily template | `scrbl create -d` |
| `create --template <name>` | Use specific template | `scrbl create -t meeting` |
| `write <content>` | Add content to notes | `scrbl write "New idea"` |
| `write <content> --section <name>` | Add to specific section | `scrbl write "Task" -s todos` |
| `edit` | Open in text editor | `scrbl edit` |
| `edit --editor <name>` | Use specific editor | `scrbl edit -e code` |

## Templates

Scrbl comes with built-in templates and supports custom ones:

### Default Daily Template
```markdown
## 2024.07.09
### Daily Summary
```

### Custom Templates
Templates support date placeholders like `{date:yyyy.MM.dd}` and can include multiple sections.

## Architecture

### Indexing System
- **Headers**: Tracks `## ` level 2 headers (daily entries)
- **Sections**: Tracks `### ` level 3 headers (sections within days)
- **Word Index**: Maps words to line numbers for fast searches
- **Persistent Caching**: Saves indexes to disk to avoid rebuilding
- **Incremental Updates**: Updates indexes efficiently when content changes

### File Structure
```
your-notes/
├── scrbl.md                    # Your main notes file
└── scrbl.md.scrbl-index        # Cached index (auto-generated)
```

## Configuration

Scrbl stores its configuration in:
- **Windows**: `%APPDATA%\scrbl\config.json`
- **Linux/Mac**: `~/.config/scrbl/config.json`
