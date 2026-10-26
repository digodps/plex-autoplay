"""WSGI server factory for HTTP and direct HTTPS dashboard deployments."""

from __future__ import annotations

from pathlib import Path
from typing import Any

from cheroot.ssl.builtin import BuiltinSSLAdapter
from cheroot.wsgi import Server as CherootServer
from waitress import create_server

from plex_autoplay.infrastructure.web.tls import ensure_self_signed_certificate


class DashboardServer:
    def __init__(self, server: Any, *, tls_enabled: bool):
        self.server = server
        self.tls_enabled = tls_enabled

    def run(self) -> None:
        if self.tls_enabled:
            self.server.start()
        else:
            self.server.run()

    def close(self) -> None:
        if self.tls_enabled:
            self.server.stop()
        else:
            self.server.close()


def create_dashboard_server(
    application: Any,
    host: str,
    port: int,
    *,
    https_enabled: bool,
    config_dir: str | Path,
    tls_hosts: list[str],
) -> DashboardServer:
    if https_enabled:
        certificate, private_key = ensure_self_signed_certificate(config_dir, tls_hosts)
        server = CherootServer((host, port), application)
        server.ssl_adapter = BuiltinSSLAdapter(str(certificate), str(private_key))
        return DashboardServer(server, tls_enabled=True)
    return DashboardServer(create_server(application, host=host, port=port), tls_enabled=False)
