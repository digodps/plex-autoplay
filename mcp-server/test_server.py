#!/usr/bin/env python3
"""Smoke test do MCP server: python3 mcp-server/test_server.py"""
import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
REQS = [
    {"jsonrpc": "2.0", "method": "initialize", "id": 1},
    {"jsonrpc": "2.0", "method": "tools/list", "id": 2},
    {"jsonrpc": "2.0", "method": "resources/read", "id": 3, "params": {"uri": "plex://codemap"}},
    {"jsonrpc": "2.0", "method": "tools/call", "id": 4,
     "params": {"name": "validate_commit_msg", "arguments": {"message": "feat(auth): ✨ add login"}}},
]


def main() -> int:
    proc = subprocess.Popen(
        [sys.executable, str(ROOT / "mcp-server/server.py")],
        stdin=subprocess.PIPE, stdout=subprocess.PIPE, text=True, cwd=ROOT,
    )
    try:
        responses = {}
        for req in REQS:
            proc.stdin.write(json.dumps(req) + "\n")
            proc.stdin.flush()
            resp = json.loads(proc.stdout.readline())
            assert resp["id"] == req["id"], f"id mismatch: {resp}"
            assert "error" not in resp, f"error on {req['method']}: {resp}"
            responses[req["id"]] = resp

        names = {t["name"] for t in responses[2]["result"]["tools"]}
        assert {"get_best_practices", "get_code_map", "validate_commit_msg"} <= names
        assert "backend" in responses[3]["result"]["contents"][0]["text"]
        valid = json.loads(responses[4]["result"]["content"][0]["text"])["valid"]
        assert valid is True, "commit validation rejected valid message"
    finally:
        proc.terminate()
        proc.wait()

    print("MCP smoke test OK")
    return 0


if __name__ == "__main__":
    sys.exit(main())
