# ===========================================
# Enable SMTP AUTH for info@doganconsult.com
# ===========================================
# Run in PowerShell as Administrator
# 
# Usage: 
#   .\enable-smtp-auth.ps1
# ===========================================

$EmailAddress = "info@doganconsult.com"
$AdminEmail = "admin@doganconsult.com"

Write-Host ""
Write-Host "╔══════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║   Office 365 SMTP AUTH Configuration                 ║" -ForegroundColor Cyan
Write-Host "║   Email: $EmailAddress                      ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Check if module is installed
$module = Get-Module -ListAvailable -Name ExchangeOnlineManagement
if (-not $module) {
    Write-Host "[1/5] Installing ExchangeOnlineManagement module..." -ForegroundColor Yellow
    Install-Module -Name ExchangeOnlineManagement -Force -AllowClobber -Scope CurrentUser
    Write-Host "      ✓ Module installed" -ForegroundColor Green
} else {
    Write-Host "[1/5] ExchangeOnlineManagement module already installed" -ForegroundColor Green
}

# Import module
Write-Host "[2/5] Importing module..." -ForegroundColor Yellow
Import-Module ExchangeOnlineManagement -ErrorAction Stop
Write-Host "      ✓ Module imported" -ForegroundColor Green

# Connect
Write-Host "[3/5] Connecting to Exchange Online..." -ForegroundColor Yellow
Write-Host "      (Browser will open for authentication)" -ForegroundColor Gray
try {
    Connect-ExchangeOnline -UserPrincipalName $AdminEmail -ShowBanner:$false
    Write-Host "      ✓ Connected" -ForegroundColor Green
} catch {
    Write-Host "      ✗ Connection failed: $_" -ForegroundColor Red
    exit 1
}

# Enable SMTP AUTH
Write-Host "[4/5] Enabling SMTP AUTH for $EmailAddress..." -ForegroundColor Yellow
try {
    Set-CASMailbox -Identity $EmailAddress -SmtpClientAuthenticationDisabled $false
    Write-Host "      ✓ SMTP AUTH enabled" -ForegroundColor Green
} catch {
    Write-Host "      ✗ Failed: $_" -ForegroundColor Red
}

# Verify
Write-Host "[5/5] Verifying..." -ForegroundColor Yellow
$mailbox = Get-CASMailbox -Identity $EmailAddress | Select-Object SmtpClientAuthenticationDisabled
if ($mailbox.SmtpClientAuthenticationDisabled -eq $false) {
    Write-Host "      ✓ SMTP AUTH is ENABLED" -ForegroundColor Green
    Write-Host ""
    Write-Host "╔══════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║   SUCCESS! SMTP AUTH is now enabled.                 ║" -ForegroundColor Green
    Write-Host "║   You can now send emails via OAuth2.                ║" -ForegroundColor Green
    Write-Host "╚══════════════════════════════════════════════════════╝" -ForegroundColor Green
} else {
    Write-Host "      ✗ SMTP AUTH is still DISABLED" -ForegroundColor Red
    Write-Host ""
    Write-Host "Check if SMTP AUTH is blocked at organization level:" -ForegroundColor Yellow
    Write-Host "  Get-TransportConfig | Select SmtpClientAuthenticationDisabled" -ForegroundColor Gray
}

# Disconnect
Disconnect-ExchangeOnline -Confirm:$false -ErrorAction SilentlyContinue
Write-Host ""
Write-Host "Disconnected from Exchange Online." -ForegroundColor Gray
