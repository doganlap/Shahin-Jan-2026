#!/usr/bin/env python3
"""
Security Fix: Replace ex.Message exposure with safe error handling.
This script fixes exception disclosure vulnerabilities in ASP.NET Core controllers.
"""

import os
import re
import sys
from pathlib import Path

# Patterns to find and replace
PATTERNS = [
    # Pattern 1: return BadRequest(ApiResponse<T>.ErrorResponse(ex.Message))
    (
        r'return BadRequest\(ApiResponse<([^>]+)>\.ErrorResponse\(ex\.Message\)\);',
        r'return BadRequest(ApiResponse<\1>.ErrorResponse("An error occurred processing your request."));'
    ),
    # Pattern 2: return BadRequest(ApiResponse.ErrorResponse(ex.Message))
    (
        r'return BadRequest\(ApiResponse\.ErrorResponse\(ex\.Message\)\);',
        r'return BadRequest(ApiResponse.ErrorResponse("An error occurred processing your request."));'
    ),
    # Pattern 3: return BadRequest(new { error = ex.Message })
    (
        r'return BadRequest\(new \{ error = ex\.Message \}\);',
        r'return BadRequest(new { error = "An error occurred processing your request." });'
    ),
    # Pattern 4: return BadRequest(new { success = false, error = ex.Message })
    (
        r'return BadRequest\(new \{ success = false, error = ex\.Message \}\);',
        r'return BadRequest(new { success = false, error = "An error occurred processing your request." });'
    ),
    # Pattern 5: return StatusCode(500, new { error = ex.Message })
    (
        r'return StatusCode\(500, new \{ error = ex\.Message \}\);',
        r'return StatusCode(500, new { error = "An internal error occurred. Please try again later." });'
    ),
    # Pattern 6: return StatusCode(500, new { error = "...", details = ex.Message })
    (
        r'return StatusCode\(500, new \{ error = "[^"]+", details = ex\.Message \}\);',
        r'return StatusCode(500, new { error = "An internal error occurred. Please try again later." });'
    ),
    # Pattern 7: return StatusCode(500, new { error = "...", message = ex.Message })
    (
        r'return StatusCode\(500, new \{ error = "[^"]+", message = ex\.Message \}\);',
        r'return StatusCode(500, new { error = "An internal error occurred. Please try again later." });'
    ),
    # Pattern 8: return NotFound(new { error = ex.Message })
    (
        r'return NotFound\(new \{ error = ex\.Message \}\);',
        r'return NotFound(new { error = "The requested resource was not found." });'
    ),
    # Pattern 9: return Json(new { success = false, error = ex.Message })
    (
        r'return Json\(new \{ success = false, error = ex\.Message \}\);',
        r'return Json(new { success = false, error = "An error occurred processing your request." });'
    ),
    # Pattern 10: TempData["Error"] = $"Error: {ex.Message}"
    (
        r'TempData\["Error"\] = \$"Error: \{ex\.Message\}";',
        r'TempData["Error"] = "An error occurred. Please try again.";'
    ),
    # Pattern 11: new { error = "GRC:...", message = ex.Message }
    (
        r'return BadRequest\(new \{ error = "GRC:[^"]+", message = ex\.Message \}\);',
        r'return BadRequest(new { error = "GRC:ERROR", message = "An error occurred processing your request." });'
    ),
    # Pattern 12: return NotFound(new { error = "GRC:...", message = ex.Message })
    (
        r'return NotFound\(new \{ error = "GRC:[^"]+", message = ex\.Message \}\);',
        r'return NotFound(new { error = "GRC:NOT_FOUND", message = "The requested resource was not found." });'
    ),
    # Pattern 13: TempData["Error"] = $"Policy Violation: {pex.Message}. {pex.RemediationHint}"
    (
        r'TempData\["Error"\] = \$"Policy Violation: \{pex\.Message\}\. \{pex\.RemediationHint\}";',
        r'TempData["Error"] = "A policy violation occurred. Please review the requirements.";'
    ),
    # Pattern 14: TempData["ErrorMessage"] = $"Policy Violation: {pex.Message}. {pex.RemediationHint}"
    (
        r'TempData\["ErrorMessage"\] = \$"Policy Violation: \{pex\.Message\}\. \{pex\.RemediationHint\}";',
        r'TempData["ErrorMessage"] = "A policy violation occurred. Please review the requirements.";'
    ),
    # Pattern 15: TempData["ErrorMessage"] = $"Policy Violation: {pex.Message}"
    (
        r'TempData\["ErrorMessage"\] = \$"Policy Violation: \{pex\.Message\}";',
        r'TempData["ErrorMessage"] = "A policy violation occurred. Please review the requirements.";'
    ),
    # Pattern 16: TempData["Error"] = "Error resending credentials: " + ex.Message
    (
        r'TempData\["Error"\] = "Error [^"]+: " \+ ex\.Message;',
        r'TempData["Error"] = "An error occurred. Please try again.";'
    ),
    # Pattern 17: return BadRequest(ex.Message)
    (
        r'return BadRequest\(ex\.Message\);',
        r'return BadRequest("An error occurred processing your request.");'
    ),
    # Pattern 18: return BadRequest(ApiResponse<object>.ErrorResponse($"...: {ex.Message}"))
    (
        r'return BadRequest\(ApiResponse<object>\.ErrorResponse\(\$"[^"]+: \{ex\.Message\}"\)\);',
        r'return BadRequest(ApiResponse<object>.ErrorResponse("An error occurred processing your request."));'
    ),
    # Pattern 19: TempData["Error"] = $"...: {ex.Message}" (Arabic)
    (
        r'TempData\["Error"\] = \$"[^"]+: \{ex\.Message\}";',
        r'TempData["Error"] = "An error occurred. Please try again.";'
    ),
    # Pattern 20: return NotFound(new { success = false, error = ex.Message })
    (
        r'return NotFound\(new \{ success = false, error = ex\.Message \}\);',
        r'return NotFound(new { success = false, error = "The requested resource was not found." });'
    ),
    # Pattern 21: return StatusCode(500, new { success = false, error = ex.Message })
    (
        r'return StatusCode\(500, new \{ success = false, error = ex\.Message \}\);',
        r'return StatusCode(500, new { success = false, error = "An internal error occurred." });'
    ),
    # Pattern 22: return StatusCode(500, new { success = false, error = ex.Message });
    (
        r'return StatusCode\(500, new \{ success = false, error = ex\.Message \}\)\s*;',
        r'return StatusCode(500, new { success = false, error = "An internal error occurred." });'
    ),
    # Pattern 23: ModelState.AddModelError("", ex.Message)
    (
        r'ModelState\.AddModelError\("", ex\.Message\);',
        r'ModelState.AddModelError("", "An error occurred processing your request.");'
    ),
    # Pattern 24: ModelState.AddModelError("", $"Policy Violation: {pex.Message}")
    (
        r'ModelState\.AddModelError\("", \$"Policy Violation: \{pex\.Message\}"\);',
        r'ModelState.AddModelError("", "A policy violation occurred. Please review the requirements.");'
    ),
    # Pattern 25: errors.Add($"Risk '{risk.Name}': {ex.Message}")
    (
        r'errors\.Add\(\$"[^"]+: \{ex\.Message\}"\);',
        r'errors.Add("An error occurred processing this item.");'
    ),
    # Pattern 26: Detail = ex.Message,
    (
        r'Detail = ex\.Message,',
        r'Detail = "An error occurred processing your request.",',
    ),
    # Pattern 27: return Forbid(ex.Message);
    (
        r'return Forbid\(ex\.Message\);',
        r'return Forbid("Access denied.");',
    ),
    # Pattern 28: return Conflict(new { error = ex.Message });
    (
        r'return Conflict\(new \{ error = ex\.Message \}\);',
        r'return Conflict(new { error = "A conflict occurred with the current state." });',
    ),
    # Pattern 29: error = ex.Message (with newline)
    (
        r'error = ex\.Message,?\n',
        r'error = "An error occurred.",\n',
    ),
    # Pattern 30: StatusCode(500, new { success = false, message = ex.Message })
    (
        r'return StatusCode\(500, new \{ success = false, message = ex\.Message \}\);',
        r'return StatusCode(500, new { success = false, message = "An internal error occurred." });',
    ),
]

# Files to skip (already manually fixed or special cases)
SKIP_FILES = {
    'SecureApiControllerBase.cs',  # Our new base controller
}

# Patterns that are OK to keep (logging, etc.)
SAFE_PATTERNS = [
    r'_logger\.Log',  # Logging is OK
    r'LogError.*ex\.Message',  # Logging errors is OK
    r'LogWarning.*ex\.Message',  # Logging warnings is OK
    r'LogInformation.*ex\.Message',  # Logging info is OK
    r'ex\.Message\.Contains',  # Checking message content is OK
    r'ModelState\.AddModelError.*ex\.Message',  # ModelState for MVC is OK (shown to user anyway)
]

def should_skip_line(line: str) -> bool:
    """Check if line contains safe patterns that shouldn't be modified."""
    for pattern in SAFE_PATTERNS:
        if re.search(pattern, line):
            return True
    return False

def fix_file(filepath: Path, dry_run: bool = False) -> int:
    """Fix exception disclosure in a single file. Returns count of fixes."""
    if filepath.name in SKIP_FILES:
        return 0
    
    try:
        content = filepath.read_text(encoding='utf-8')
    except Exception as e:
        print(f"  Error reading {filepath}: {e}")
        return 0
    
    original_content = content
    fix_count = 0
    
    for pattern, replacement in PATTERNS:
        # Find all matches first
        matches = list(re.finditer(pattern, content))
        for match in reversed(matches):  # Reverse to preserve positions
            # Get the full line containing the match
            line_start = content.rfind('\n', 0, match.start()) + 1
            line_end = content.find('\n', match.end())
            line = content[line_start:line_end if line_end != -1 else len(content)]
            
            # Skip if it's a safe pattern
            if should_skip_line(line):
                continue
            
            # Apply the fix
            content = content[:match.start()] + re.sub(pattern, replacement, match.group()) + content[match.end():]
            fix_count += 1
    
    if fix_count > 0 and not dry_run:
        filepath.write_text(content, encoding='utf-8')
    
    return fix_count

def main():
    dry_run = '--dry-run' in sys.argv
    verbose = '--verbose' in sys.argv or '-v' in sys.argv
    
    controllers_dir = Path('/home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc/Controllers')
    
    if not controllers_dir.exists():
        print(f"Error: Controllers directory not found: {controllers_dir}")
        sys.exit(1)
    
    total_fixes = 0
    files_fixed = 0
    
    # Find all .cs files in Controllers directory
    cs_files = list(controllers_dir.rglob('*.cs'))
    cs_files = [f for f in cs_files if '.backup' not in str(f)]
    
    print(f"{'[DRY RUN] ' if dry_run else ''}Scanning {len(cs_files)} controller files...")
    print()
    
    for filepath in sorted(cs_files):
        fixes = fix_file(filepath, dry_run)
        if fixes > 0:
            files_fixed += 1
            total_fixes += fixes
            if verbose or fixes > 0:
                print(f"  {filepath.name}: {fixes} fixes")
    
    print()
    print(f"{'[DRY RUN] ' if dry_run else ''}Summary:")
    print(f"  Files processed: {len(cs_files)}")
    print(f"  Files modified: {files_fixed}")
    print(f"  Total fixes: {total_fixes}")
    
    if dry_run:
        print("\nRun without --dry-run to apply changes.")

if __name__ == '__main__':
    main()
