#!/usr/bin/env python3
"""
Simple HTTP server to serve the unified dashboard
Run with: python3 serve-dashboard.py
"""

import http.server
import socketserver
import os
import urllib.parse

PORT = 8000
DIRECTORY = os.path.dirname(os.path.abspath(__file__))
GRC_APP_URL = "http://localhost:5000"  # .NET app URL

class MyHTTPRequestHandler(http.server.SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=DIRECTORY, **kwargs)

    def do_GET(self):
        # Serve dashboard portal at root
        if self.path == '/':
            self.path = '/index.html'

        # Redirect /onboarding to .NET app
        if self.path.startswith('/onboarding') or self.path.startswith('/Onboarding'):
            self.send_response(302)
            self.send_header('Location', f'{GRC_APP_URL}{self.path}')
            self.end_headers()
            return

        # Serve other files normally
        super().do_GET()

    def end_headers(self):
        # Add CORS headers to allow embedding
        self.send_header('Access-Control-Allow-Origin', '*')
        self.send_header('Access-Control-Allow-Methods', 'GET, POST, OPTIONS')
        self.send_header('Access-Control-Allow-Headers', 'Content-Type')
        super().end_headers()
    
    def list_directory(self, path):
        """Override to prevent directory listing"""
        # Redirect to dashboard portal instead of showing directory listing
        self.send_response(302)
        self.send_header('Location', '/index.html')
        self.end_headers()
        return None

if __name__ == "__main__":
    handler = MyHTTPRequestHandler

    with socketserver.TCPServer(("", PORT), handler) as httpd:
        print(f"╔═══════════════════════════════════════════════════════════╗")
        print(f"║  🎯 GRC Role-Based Dashboard Portal                      ║")
        print(f"╚═══════════════════════════════════════════════════════════╝")
        print(f"")
        print(f"  🌐 Portal URL:     http://localhost:{PORT}")
        print(f"  📁 Serving from:   {DIRECTORY}")
        print(f"")
        print(f"  Role-Based Dashboards:")
        print(f"  ├─ 👑 Platform Owner:  http://localhost:{PORT}/dashboard-platform-owner.html")
        print(f"  ├─ ⚙️  Platform Admin:  http://localhost:{PORT}/dashboard-platform-admin.html")
        print(f"  └─ 📊 General User:     http://localhost:{PORT}/unified-dashboard.html")
        print(f"")
        print(f"  Services Available:")
        print(f"  ├─ 📈 Grafana:     http://localhost:3030")
        print(f"  ├─ 🔍 Superset:    http://localhost:8088")
        print(f"  ├─ 📉 Metabase:    http://localhost:3001")
        print(f"  ├─ 🔄 Kafka UI:    http://localhost:9080")
        print(f"  ├─ 🔗 n8n:         http://localhost:5678")
        print(f"  ├─ ⚙️  Camunda:     http://localhost:8085")
        print(f"  └─ 🏢 GRC MVC:     http://localhost:8888")
        print(f"")
        print(f"  Press Ctrl+C to stop the server")
        print(f"")

        try:
            httpd.serve_forever()
        except KeyboardInterrupt:
            print(f"\n\n  Server stopped.")
