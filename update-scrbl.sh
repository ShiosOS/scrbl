#!/bin/bash
# update-scrbl.sh - Cross-platform update script for scrbl

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m'

error() {
    echo -e "${RED}❌ $1${NC}"
}

success() {
    echo -e "${GREEN}✅ $1${NC}"
}

info() {
    echo -e "${CYAN}ℹ️  $1${NC}"
}

warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

status() {
    echo -e "${GREEN}🚀 $1${NC}"
}

FORCE=false
VERBOSE=false
SKIP_BUILD=false

while [[ $# -gt 0 ]]; do
    case $1 in
        --force)
            FORCE=true
            shift
            ;;
        --verbose)
            VERBOSE=true
            shift
            ;;
        --skip-build)
            SKIP_BUILD=true
            shift
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--force] [--verbose] [--skip-build]"
            exit 1
            ;;
    esac
done

status "Updating scrbl tool..."
echo

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )"
PROJECT_DIR="$SCRIPT_DIR"

if [[ ! -f "$PROJECT_DIR/scrbl.csproj" ]]; then
    error "scrbl.csproj not found in $PROJECT_DIR"
    info "Make sure you're running this script from the scrbl project directory"
    exit 1
fi

info "Project directory: $PROJECT_DIR"
cd "$PROJECT_DIR"

if dotnet tool list --global | grep -q "scrbl"; then
    info "scrbl is currently installed"
else
    warning "scrbl is not currently installed as a global tool"
fi

if [[ "$SKIP_BUILD" != true ]]; then
    echo -e "${YELLOW}🔨 Building project...${NC}"
    
    if ! dotnet clean --configuration Release > /dev/null 2>&1; then
        warning "Clean failed, continuing anyway..."
    fi
    
    if ! dotnet build --configuration Release > /dev/null 2>&1; then
        error "Build failed!"
        exit 1
    fi
    success "Build completed"
    
    echo -e "${YELLOW}📦 Packing tool...${NC}"
    if ! dotnet pack --configuration Release --no-build > /dev/null 2>&1; then
        error "Pack failed!"
        exit 1
    fi
    success "Pack completed"
else
    info "Skipping build (--skip-build specified)"
fi

if [[ ! -d "./scrbl" ]]; then
    error "Package directory not found: ./scrbl"
    exit 1
fi

if ! ls ./scrbl/*.nupkg 1> /dev/null 2>&1; then
    error "No .nupkg files found in ./scrbl"
    exit 1
fi

echo -e "${YELLOW}🗑️  Uninstalling current version...${NC}"
if dotnet tool uninstall --global scrbl > /dev/null 2>&1; then
    success "Uninstalled successfully"
else
    warning "Uninstall failed or tool wasn't installed"
fi

echo -e "${YELLOW}📥 Installing new version...${NC}"
if ! dotnet tool install --global --add-source ./scrbl scrbl; then
    error "Installation failed!"
    echo
    info "Troubleshooting suggestions:"
    info "1. Make sure no scrbl processes are running"
    info "2. Try running with sudo (if on Linux/Mac)"
    info "3. Check if the package is corrupted"
    exit 1
fi

success "Installation completed!"

echo -e "${YELLOW}🔍 Verifying installation...${NC}"
if scrbl --help > /dev/null 2>&1; then
    success "Verification successful!"
else
    warning "Verification failed - tool may not be properly installed"
fi

echo
success "🎉 scrbl has been updated successfully!"
info "You can now use 'scrbl --help' to see available commands"

echo
echo -e "${CYAN}💡 Quick start:${NC}"
echo -e "${GRAY}   scrbl setup <path>     # Configure notes directory${NC}"
echo -e "${GRAY}   scrbl create -d        # Create daily template${NC}"
echo -e "${GRAY}   scrbl write 'content'  # Add content to notes${NC}"
echo -e "${GRAY}   scrbl edit             # Edit notes in editor${NC}"
