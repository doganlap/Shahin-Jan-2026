# GRC System - Development Environment Setup Guide

This guide provides step-by-step instructions for setting up your IDE and development tools for the GRC system.

## Prerequisites

- **IDE:** Visual Studio 2022 or VS Code with C# extension
- **.NET SDK:** 8.0 or later
- **Git:** For version control and hooks

## Phase -1: IDE Configuration & Development Tools

### 1. IDE Extensions Installation

#### Visual Studio 2022:
1. **SonarLint:**
   - Go to Extensions → Manage Extensions
   - Search for "SonarLint"
   - Install SonarLint for Visual Studio
   - Restart Visual Studio

2. **CodeMaid:**
   - Search for "CodeMaid" in Extensions
   - Install CodeMaid
   - Restart Visual Studio

#### VS Code:
1. **SonarLint:**
   - Open Extensions (Ctrl+Shift+X)
   - Search for "SonarLint"
   - Install SonarLint extension
   - Reload VS Code

2. **C# Dev Kit:**
   - Install "C# Dev Kit" extension (includes Roslyn analyzers)

### 2. SonarLint Configuration

1. **Connect to SonarQube Server (Optional):**
   - Open SonarLint settings
   - Click "Connect to SonarQube"
   - Enter SonarQube server URL
   - Authenticate with token
   - Select project: `grc-system`

2. **Quality Profile:**
   - Use default profile or ABP Framework-compatible profile
   - Ensure real-time analysis is enabled

### 3. Security Agents Setup

#### Snyk Setup:
1. **Install Snyk CLI:**
   ```bash
   npm install -g snyk
   ```

2. **Authenticate:**
   ```bash
   snyk auth
   ```
   - Follow prompts to authenticate

3. **VS Code Extension:**
   - Install "Snyk" extension from marketplace
   - Authenticate in extension settings

4. **Configuration:**
   - Create `.snyk` file in solution root (will be created automatically on first scan)
   - Configure ignore rules if needed

#### GitGuardian Setup:
1. **Install GitGuardian CLI:**
   ```bash
   pip install ggshield
   ```

2. **VS Code Extension:**
   - Install "GitGuardian" extension
   - Configure API key in extension settings

3. **Configuration:**
   - Create `.gitguardian.yml` in solution root
   - Configure secret patterns if needed

### 4. Code Analysis Tools

#### Roslyn Analyzers:
- **Already configured** via `Directory.Build.props`
- Analyzers run automatically on build
- Check Error List for warnings/errors

#### StyleCop Analyzers:
- **Already configured** via `Directory.Build.props`
- Rules configured in `.editorconfig`
- Format code automatically on save (VS Code) or via CodeMaid (Visual Studio)

### 5. Pre-Commit Git Hooks

#### Setup Pre-Commit Hook:

1. **Create hook file:**
   ```bash
   cd /home/dogan/grc-system
   mkdir -p .git/hooks
   ```

2. **Create `.git/hooks/pre-commit`:**
   ```bash
   #!/bin/bash
   
   echo "Running pre-commit checks..."
   
   # Build check
   dotnet build --no-incremental
   if [ $? -ne 0 ]; then
       echo "Build failed. Commit aborted."
       exit 1
   fi
   
   # Format check
   dotnet format --verify-no-changes
   if [ $? -ne 0 ]; then
       echo "Code formatting check failed. Run 'dotnet format' to fix."
       exit 1
   fi
   
   # Secret scan (if GitGuardian CLI installed)
   if command -v ggshield &> /dev/null; then
       ggshield secret scan pre-commit
       if [ $? -ne 0 ]; then
           echo "Secret scan failed. Commit aborted."
           exit 1
       fi
   fi
   
   # Security scan (if Snyk CLI installed)
   if command -v snyk &> /dev/null; then
       snyk test --severity-threshold=high
       if [ $? -ne 0 ]; then
           echo "Security scan found high-severity vulnerabilities. Commit aborted."
           exit 1
       fi
   fi
   
   echo "All pre-commit checks passed."
   exit 0
   ```

3. **Make hook executable:**
   ```bash
   chmod +x .git/hooks/pre-commit
   ```

### 6. Verify Setup

#### Test Code Review Agents:
1. Create a test file with intentional issues:
   ```csharp
   // Test file: TestCodeQuality.cs
   public class BadCode
   {
       public void Test()
       {
           string password = "hardcoded-secret";  // GitGuardian should detect
           var result = ProcessData(null);  // CA1062 should warn
       }
       
       private string ProcessData(string input) => input.ToLower();  // CA1308 should warn
   }
   ```

2. Verify agents detect issues:
   - SonarLint shows code smell warnings
   - GitGuardian detects hardcoded secret
   - Roslyn analyzers show CA warnings

#### Test Pre-Commit Hook:
1. Try to commit code with errors:
   ```bash
   # Make a change that breaks build
   git add .
   git commit -m "Test commit"
   ```

2. Verify hook blocks commit with errors

#### Test Security Agents:
1. Add vulnerable package (for testing):
   ```xml
   <PackageReference Include="Example.Vulnerable.Package" Version="1.0.0" />
   ```

2. Verify Snyk detects vulnerability

### 7. Configuration Files

All configuration files are already created:
- ✅ `.editorconfig` - Code style rules
- ✅ `Directory.Build.props` - Analyzer packages
- ✅ `Grc.ruleset` - Code analysis rules

### 8. Troubleshooting

#### SonarLint not showing issues:
- Check SonarLint is enabled in IDE settings
- Verify SonarLint extension is active
- Restart IDE

#### Analyzers not running:
- Restore NuGet packages: `dotnet restore`
- Rebuild solution: `dotnet build`
- Check Error List in IDE

#### Pre-commit hook not running:
- Verify hook file is executable: `chmod +x .git/hooks/pre-commit`
- Check Git version supports hooks
- Test hook manually: `.git/hooks/pre-commit`

#### Snyk not detecting vulnerabilities:
- Verify Snyk CLI is installed: `snyk --version`
- Authenticate: `snyk auth`
- Run scan manually: `snyk test`

#### GitGuardian not detecting secrets:
- Verify GitGuardian CLI is installed: `ggshield --version`
- Configure API key in extension or environment
- Test manually: `ggshield secret scan path .`

### 9. Next Steps

After completing IDE setup:
1. Verify all agents are active
2. Proceed to **Phase 0: Core Integration & Error Handling**
3. Follow the plan execution checklist

### 10. Support

For issues or questions:
- Check plan documentation: `/root/.cursor/plans/grc_production_readiness_plan_6c402cfa.plan.md`
- Review Phase -1 requirements in plan
- Verify all configuration files match plan specifications
