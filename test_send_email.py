#!/usr/bin/env python3
"""
Test Email Sending - SMTP Basic Auth and Microsoft Graph API
"""

import smtplib
import sys
import json
import requests
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
from datetime import datetime

# Configuration from .env.production.final
TENANT_ID = "c8847e8a-33a0-4b6c-8e01-2e0e6b4aaef5"
CLIENT_ID = "4e2575c6-e269-48eb-b055-ad730a2150a7"
CLIENT_SECRET = "Wx38Q~5VWvTmcizGb5qXNZREQyNp3yyzCUot.b5x"

SMTP_SERVER = "smtp.office365.com"
SMTP_PORT = 587
SMTP_USERNAME = "info@doganconsult.com"
# Note: We need SMTP_PASSWORD for Basic Auth - this should be an App Password
SMTP_PASSWORD = None  # Set this if testing Basic Auth

FROM_EMAIL = "info@doganconsult.com"
TO_EMAIL = "ahmet.dogan@doganconsult.com"

def get_access_token():
    """Get Microsoft Graph access token using client credentials"""
    token_url = f"https://login.microsoftonline.com/{TENANT_ID}/oauth2/v2.0/token"
    
    data = {
        "client_id": CLIENT_ID,
        "client_secret": CLIENT_SECRET,
        "scope": "https://graph.microsoft.com/.default",
        "grant_type": "client_credentials"
    }
    
    try:
        response = requests.post(token_url, data=data, timeout=30)
        response.raise_for_status()
        token_data = response.json()
        return token_data.get("access_token")
    except Exception as e:
        print(f"❌ Error getting access token: {e}")
        if hasattr(e, 'response') and e.response is not None:
            print(f"Response: {e.response.text}")
        return None

def send_via_graph_api(subject, body, is_html=True):
    """Send email via Microsoft Graph API"""
    print("\n📧 Testing Email via Microsoft Graph API...")
    print("=" * 60)
    
    access_token = get_access_token()
    if not access_token:
        print("❌ Failed to get access token")
        return False
    
    print("✅ Access token obtained")
    
    # Prepare Graph API message
    message = {
        "message": {
            "subject": subject,
            "body": {
                "contentType": "HTML" if is_html else "Text",
                "content": body
            },
            "toRecipients": [
                {
                    "emailAddress": {
                        "address": TO_EMAIL
                    }
                }
            ]
        }
    }
    
    graph_url = f"https://graph.microsoft.com/v1.0/users/{FROM_EMAIL}/sendMail"
    headers = {
        "Authorization": f"Bearer {access_token}",
        "Content-Type": "application/json"
    }
    
    try:
        print(f"Sending email from: {FROM_EMAIL}")
        print(f"Sending email to: {TO_EMAIL}")
        print(f"Subject: {subject}")
        
        response = requests.post(graph_url, headers=headers, json=message, timeout=30)
        
        if response.status_code == 202:
            print("✅ Email sent successfully via Microsoft Graph API!")
            print(f"Response: {response.status_code} {response.reason}")
            return True
        else:
            print(f"❌ Failed to send email: {response.status_code}")
            print(f"Response: {response.text}")
            return False
            
    except Exception as e:
        print(f"❌ Error sending email via Graph API: {e}")
        if hasattr(e, 'response') and e.response is not None:
            print(f"Response: {e.response.text}")
        return False

def send_via_smtp(subject, body, is_html=True):
    """Send email via SMTP Basic Auth"""
    print("\n📧 Testing Email via SMTP Basic Auth...")
    print("=" * 60)
    
    if not SMTP_PASSWORD:
        print("⚠️  SMTP_PASSWORD not set. Skipping SMTP Basic Auth test.")
        print("   To test SMTP, set SMTP_PASSWORD to your App Password")
        return False
    
    try:
        msg = MIMEMultipart("alternative")
        msg["From"] = FROM_EMAIL
        msg["To"] = TO_EMAIL
        msg["Subject"] = subject
        
        if is_html:
            msg.attach(MIMEText(body, "html"))
        else:
            msg.attach(MIMEText(body, "plain"))
        
        print(f"Connecting to {SMTP_SERVER}:{SMTP_PORT}...")
        server = smtplib.SMTP(SMTP_SERVER, SMTP_PORT, timeout=30)
        server.starttls()
        print("✅ TLS connection established")
        
        print(f"Authenticating as {SMTP_USERNAME}...")
        server.login(SMTP_USERNAME, SMTP_PASSWORD)
        print("✅ Authentication successful")
        
        print(f"Sending email to {TO_EMAIL}...")
        server.send_message(msg)
        print("✅ Email sent successfully via SMTP!")
        
        server.quit()
        return True
        
    except smtplib.SMTPAuthenticationError as e:
        print(f"❌ SMTP Authentication failed: {e}")
        print("   This usually means:")
        print("   - Wrong password")
        print("   - MFA enabled (need App Password)")
        print("   - Legacy auth disabled")
        return False
    except Exception as e:
        print(f"❌ Error sending email via SMTP: {e}")
        return False

def main():
    print("=" * 60)
    print("🧪 EMAIL SENDING TEST")
    print("=" * 60)
    print(f"From: {FROM_EMAIL}")
    print(f"To: {TO_EMAIL}")
    print(f"Time: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}")
    print()
    
    # Test message
    subject = f"Test Email from Shahin AI GRC Platform - {datetime.now().strftime('%Y-%m-%d %H:%M')}"
    body_html = f"""
    <html>
    <head></head>
    <body>
        <h2>Test Email from Shahin AI GRC Platform</h2>
        <p>This is a test email to verify email configuration is working correctly.</p>
        <p><strong>Sent at:</strong> {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}</p>
        <p><strong>Method:</strong> Microsoft Graph API (OAuth2)</p>
        <hr>
        <p style="color: #666; font-size: 12px;">If you received this email, your email configuration is working! ✅</p>
    </body>
    </html>
    """
    
    body_text = f"""
Test Email from Shahin AI GRC Platform

This is a test email to verify email configuration is working correctly.

Sent at: {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}
Method: Microsoft Graph API (OAuth2)

If you received this email, your email configuration is working! ✅
    """
    
    # Test 1: Microsoft Graph API (Recommended)
    success_graph = send_via_graph_api(subject, body_html, is_html=True)
    
    # Test 2: SMTP Basic Auth (if password is set)
    success_smtp = False
    if SMTP_PASSWORD:
        success_smtp = send_via_smtp(subject, body_text, is_html=False)
    else:
        print("\n💡 To test SMTP Basic Auth, edit the script and set SMTP_PASSWORD")
    
    # Summary
    print("\n" + "=" * 60)
    print("📊 TEST SUMMARY")
    print("=" * 60)
    print(f"Microsoft Graph API: {'✅ SUCCESS' if success_graph else '❌ FAILED'}")
    print(f"SMTP Basic Auth: {'✅ SUCCESS' if success_smtp else '⚠️  SKIPPED (no password)' if not SMTP_PASSWORD else '❌ FAILED'}")
    print()
    
    if success_graph or success_smtp:
        print("✅ At least one email method is working!")
        print(f"   Check {TO_EMAIL} inbox for the test email.")
    else:
        print("❌ Email sending failed. Check the errors above.")
        sys.exit(1)

if __name__ == "__main__":
    main()
