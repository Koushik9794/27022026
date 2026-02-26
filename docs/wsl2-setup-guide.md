# WSL2 Setup Guide for Windows Developers

## Why WSL2?

Windows Subsystem for Linux 2 (WSL2) provides a **significantly better development experience** for Docker-based projects:

- ✅ **10x faster** file I/O performance
- ✅ **Native Linux environment** matching production
- ✅ **Better Docker compatibility** and performance
- ✅ **Seamless integration** with VS Code
- ✅ **Access to Linux tools** (bash, grep, sed, etc.)

## Quick Setup

### 1. Install WSL2

```powershell
# Run in PowerShell as Administrator
wsl --install

# This installs:
# - WSL2
# - Ubuntu (default distribution)
# - Virtual Machine Platform

# Restart your computer
```

### 2. Install Ubuntu 22.04 (Recommended)

```powershell
# Install specific Ubuntu version
wsl --install -d Ubuntu-22.04

# Set as default
wsl --set-default Ubuntu-22.04

# Verify installation
wsl --list --verbose
```

### 3. Initial Ubuntu Setup

```bash
# Open Ubuntu from Start Menu
# Create username and password when prompted

# Update packages
sudo apt update && sudo apt upgrade -y

# Install essential tools
sudo apt install -y git curl wget build-essential
```

### 4. Install .NET 10 SDK in WSL

```bash
# Download and run .NET install script
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# Add to PATH
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
source ~/.bashrc

# Verify installation
dotnet --version
```

### 5. Install Docker Desktop

1. **Download Docker Desktop**: https://www.docker.com/products/docker-desktop
2. **During installation**: Ensure "Use WSL 2 instead of Hyper-V" is checked
3. **After installation**:
   - Open Docker Desktop
   - Go to Settings → Resources → WSL Integration
   - Enable integration with Ubuntu-22.04
   - Click "Apply & Restart"

### 6. Verify Docker in WSL

```bash
# Open Ubuntu terminal
docker --version
docker compose version

# Test Docker
docker run hello-world
```

## Project Setup in WSL

### Clone Repository

```bash
# Navigate to home directory
cd ~

# Create projects directory
mkdir -p projects
cd projects

# Clone repository
git clone <repository-url>
cd gss-backend

# Verify you're in WSL filesystem
pwd
# Should show: /home/yourusername/projects/gss-backend
# NOT: /mnt/c/Users/...
```

> [!IMPORTANT]
> **Critical Performance Tip**: Always work in WSL filesystem (`~/projects/`), NOT in Windows filesystem (`/mnt/c/`). Working in `/mnt/c/` is 10x slower!

### Run Docker Compose

```bash
# Start all services
docker compose up -d

# View logs
docker compose logs -f

# Stop services
docker compose down
```

## VS Code Integration

### Install VS Code Extensions

In Windows VS Code, install:
- **Remote - WSL** (ms-vscode-remote.remote-wsl)
- **C# Dev Kit** (ms-dotnettools.csdevkit)
- **Docker** (ms-azuretools.vscode-docker)

### Open Project in WSL

```bash
# From WSL terminal, in project directory
code .
```

VS Code will:
- Automatically connect to WSL
- Run in Linux context
- Provide full IntelliSense and debugging

### Verify WSL Connection

Look for "WSL: Ubuntu-22.04" in bottom-left corner of VS Code.

## Common Commands

### WSL Management

```powershell
# List installed distributions
wsl --list --verbose

# Set default distribution
wsl --set-default Ubuntu-22.04

# Shutdown WSL
wsl --shutdown

# Restart WSL
wsl

# Update WSL
wsl --update
```

### File Access

```bash
# Access Windows files from WSL
cd /mnt/c/Users/YourUsername/Documents

# Access WSL files from Windows
# In File Explorer: \\wsl$\Ubuntu-22.04\home\yourusername\projects
```

## Development Workflow

### Recommended Workflow

1. **Open Ubuntu terminal** (from Start Menu)
2. **Navigate to project**: `cd ~/projects/gss-backend`
3. **Open VS Code**: `code .`
4. **Run Docker**: `docker compose up -d`
5. **Develop** in VS Code (automatically in WSL context)
6. **Run commands** in Ubuntu terminal

### Git Configuration

```bash
# Configure Git in WSL
git config --global user.name "Your Name"
git config --global user.email "your.email@example.com"

# Use VS Code as default editor
git config --global core.editor "code --wait"

# Set line endings (important for cross-platform)
git config --global core.autocrlf input
```

## Troubleshooting

### Docker Not Found in WSL

```bash
# Ensure Docker Desktop is running
# Check WSL integration in Docker Desktop settings

# Restart Docker Desktop
# Restart WSL: wsl --shutdown (in PowerShell)
```

### Slow Performance

```bash
# Verify you're in WSL filesystem
pwd
# Should be /home/... NOT /mnt/c/...

# Move project to WSL if needed
cd ~
mkdir -p projects
mv /mnt/c/path/to/gss-backend ~/projects/
```

### VS Code Not Connecting to WSL

```powershell
# Reinstall Remote - WSL extension
# Restart VS Code
# Run: wsl --shutdown
# Open Ubuntu and try: code .
```

### Port Already in Use

```bash
# Find process using port
sudo lsof -i :5001

# Kill process
sudo kill -9 <PID>

# Or restart Docker
docker compose down
docker compose up -d
```

## Performance Comparison

| Operation | Windows Filesystem | WSL2 Filesystem |
|-----------|-------------------|-----------------|
| `docker compose up` | 2-3 minutes | 15-20 seconds |
| File watching | Unreliable | Instant |
| `dotnet build` | 30-45 seconds | 5-10 seconds |
| `dotnet test` | 20-30 seconds | 3-5 seconds |

## Best Practices

✅ **Do**:
- Work in WSL filesystem (`~/projects/`)
- Use Ubuntu terminal for all commands
- Open VS Code from WSL terminal (`code .`)
- Keep Docker Desktop running
- Use WSL for Git operations

❌ **Don't**:
- Work in `/mnt/c/` (Windows filesystem)
- Mix Windows and WSL Git operations
- Edit files in Windows and run commands in WSL
- Forget to enable WSL integration in Docker Desktop

## Resources

- [WSL Documentation](https://docs.microsoft.com/en-us/windows/wsl/)
- [Docker Desktop WSL 2 Backend](https://docs.docker.com/desktop/windows/wsl/)
- [VS Code Remote - WSL](https://code.visualstudio.com/docs/remote/wsl)
- [.NET on WSL](https://docs.microsoft.com/en-us/dotnet/core/install/linux-ubuntu)

## Getting Help

If you encounter issues:
1. Check Docker Desktop is running
2. Verify WSL integration is enabled
3. Ensure you're in WSL filesystem
4. Restart WSL: `wsl --shutdown`
5. Ask the development team

---

**Ready to develop!** 🚀 Your Windows machine now has a Linux development environment with excellent Docker performance.
