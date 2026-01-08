# Office 365 SMTP AUTH Enable Script
# Run this in PowerShell as Administrator

Write-Host "=== Office 365 SMTP AUTH Configuration ===" -ForegroundColor Cyan
Write-Host ""

# Step 1: Install Exchange Online Management Module
Write-Host "Step 1: Installing Exchange Online Management Module..." -ForegroundColor Yellow
try {
    Install-Module -Name ExchangeOnlineManagement -Force -AllowClobber -Scope CurrentUser
    Write-Host "   ✅ Module installed successfully!" -ForegroundColor Green
} catch {
    Write-Host "   ⚠️ Module may already be installed or requires admin rights" -ForegroundColor Yellow
}

# Step 2: Import the module
Write-Host ""
Write-Host "Step 2: Importing module..." -ForegroundColor Yellow
Import-Module ExchangeOnlineManagement
Write-Host "   ✅ Module imported!" -ForegroundColor Green

# Step 3: Connect to Exchange Online
Write-Host ""
Write-Host "Step 3: Connecting to Exchange Online..." -ForegroundColor Yellow
Write-Host "   A browser window will open for authentication" -ForegroundColor Gray
Connect-ExchangeOnline -UserPrincipalName admin@doganconsult.com

# Step 4: Enable SMTP AUTH for the mailbox
Write-Host ""
Write-Host "Step 4: Enabling SMTP AUTH for info@doganconsult.com..." -ForegroundColor Yellow
Set-CASMailbox -Identity "info@doganconsult.com" -SmtpClientAuthenticationDisabled $false
Write-Host "   ✅ SMTP AUTH enabled!" -ForegroundColor Green

# Step 5: Verify the setting
Write-Host ""
Write-Host "Step 5: Verifying configuration..." -ForegroundColor Yellow
$result = Get-CASMailbox -Identity "info@doganconsult.com" | Select-Object SmtpClientAuthenticationDisabled
Write-Host "   SmtpClientAuthenticationDisabled: $($result.SmtpClientAuthenticationDisabled)" -ForegroundColor Cyan
if ($result.SmtpClientAuthenticationDisabled -eq $false) {
    Write-Host "   ✅ SMTP AUTH is now ENABLED!" -ForegroundColor Green
} else {
    Write-Host "   ❌ SMTP AUTH is still disabled" -ForegroundColor Red
}

# Step 6: Disconnect
Write-Host ""
Write-Host "Step 6: Disconnecting..." -ForegroundColor Yellow
Disconnect-ExchangeOnline -Confirm:$false
Write-Host "   ✅ Disconnected!" -ForegroundColor Green

Write-Host ""
Write-Host "=== Configuration Complete ===" -ForegroundColor Cyan
Write-Host "Now run the email test again!" -ForegroundColor Green
