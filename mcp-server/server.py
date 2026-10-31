#!/usr/bin/env python3
import sys
import json
import os
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent

def get_guidelines():
    guide_path = ROOT / "DEVELOPMENT_GUIDE.md"
    if guide_path.exists():
        return guide_path.read_text(encoding="utf-8")
    return "Development guide not found."

def get_codemap():
    return {
        "backend": {
            "path": "src/",
            "description": "C# .NET Backend, API Endpoints, Domain, Infrastructure",
            "key_files": ["src/PlexAutoplay.slnx", "src/PlexAutoplay.Web/"]
        },
        "frontend": {
            "path": "vue/plex-frontend/",
            "description": "Vue 3 + Vite + Pinia Frontend",
            "key_files": ["vue/plex-frontend/src/App.vue", "vue/plex-frontend/src/stores/", "vue/plex-frontend/src/views/"]
        },
        "tests": {
            "path": "tests/",
            "description": "Unit tests (C#) and E2E Playwright tests",
            "key_files": ["tests/unit/", "tests/e2e/"]
        },
        "openspec": {
            "path": "openspec/",
            "description": "OpenSpec changes, design, proposal, and tasks",
            "key_files": ["openspec/changes/"]
        }
    }

def handle_request(req):
    method = req.get("method")
    req_id = req.get("id")

    if method == "initialize":
        return {
            "jsonrpc": "2.0",
            "id": req_id,
            "result": {
                "protocolVersion": "2024-11-05",
                "capabilities": {
                    "resources": {},
                    "tools": {}
                },
                "serverInfo": {
                    "name": "plex-autoplay-mcp",
                    "version": "1.0.0"
                }
            }
        }

    elif method == "notifications/initialized":
        return None

    elif method == "resources/list":
        return {
            "jsonrpc": "2.0",
            "id": req_id,
            "result": {
                "resources": [
                    {
                        "uri": "plex://guidelines",
                        "name": "Development Guidelines",
                        "mimeType": "text/markdown",
                        "description": "Mandatory development and code review guidelines."
                    },
                    {
                        "uri": "plex://codemap",
                        "name": "Codebase Map",
                        "mimeType": "application/json",
                        "description": "Architecture map of backend, frontend, tests, and specs."
                    }
                ]
            }
        }

    elif method == "resources/read":
        uri = req.get("params", {}).get("uri")
        if uri == "plex://guidelines":
            content = get_guidelines()
            return {
                "jsonrpc": "2.0",
                "id": req_id,
                "result": {
                    "contents": [{"uri": uri, "mimeType": "text/markdown", "text": content}]
                }
            }
        elif uri == "plex://codemap":
            content = json.dumps(get_codemap(), indent=2)
            return {
                "jsonrpc": "2.0",
                "id": req_id,
                "result": {
                    "contents": [{"uri": uri, "mimeType": "application/json", "text": content}]
                }
            }
        else:
            return {
                "jsonrpc": "2.0",
                "id": req_id,
                "error": {"code": -32602, "message": f"Resource not found: {uri}"}
            }

    elif method == "tools/list":
        return {
            "jsonrpc": "2.0",
            "id": req_id,
            "result": {
                "tools": [
                    {
                        "name": "get_best_practices",
                        "description": "Retrieve development best practices and DOD checklist.",
                        "inputSchema": {"type": "object", "properties": {}}
                    },
                    {
                        "name": "get_code_map",
                        "description": "Retrieve structural code map of backend, frontend, and tests.",
                        "inputSchema": {"type": "object", "properties": {}}
                    },
                    {
                        "name": "validate_commit_msg",
                        "description": "Check if a commit message follows conventional commits format with emoji.",
                        "inputSchema": {
                            "type": "object",
                            "properties": {
                                "message": {"type": "string", "description": "Commit message to validate"}
                            },
                            "required": ["message"]
                        }
                    }
                ]
            }
        }

    elif method == "tools/call":
        params = req.get("params", {})
        tool_name = params.get("name")
        arguments = params.get("arguments", {})

        if tool_name == "get_best_practices":
            return {
                "jsonrpc": "2.0",
                "id": req_id,
                "result": {
                    "content": [{"type": "text", "text": get_guidelines()}]
                }
            }
        elif tool_name == "get_code_map":
            return {
                "jsonrpc": "2.0",
                "id": req_id,
                "result": {
                    "content": [{"type": "text", "text": json.dumps(get_codemap(), indent=2)}]
                }
            }
        elif tool_name == "validate_commit_msg":
            msg = arguments.get("message", "")
            import re
            pattern = r"^(feat|fix|docs|style|refactor|test|chore)(\(.+\))?(!)?: [✨🐛📝🎨♻️🧪🚀] .+"
            valid = bool(re.match(pattern, msg))
            res_text = "Valid conventional commit with emoji." if valid else "Invalid commit format. Expected: type(scope): [emoji] description (e.g. feat(auth): ✨ add login endpoint)"
            return {
                "jsonrpc": "2.0",
                "id": req_id,
                "result": {
                    "content": [{"type": "text", "text": json.dumps({"valid": valid, "reason": res_text})}]
                }
            }
        else:
            return {
                "jsonrpc": "2.0",
                "id": req_id,
                "error": {"code": -32601, "message": f"Tool not found: {tool_name}"}
            }

    else:
        return {
            "jsonrpc": "2.0",
            "id": req_id,
            "error": {"code": -32601, "message": f"Method not found: {method}"}
        }

def main():
    while True:
        line = sys.stdin.readline()
        if not line:
            break
        line = line.strip()
        if not line:
            continue
        try:
            req = json.loads(line)
        except json.JSONDecodeError:
            continue

        resp = handle_request(req)
        if resp:
            sys.stdout.write(json.dumps(resp) + "\n")
            sys.stdout.flush()

if __name__ == "__main__":
    main()