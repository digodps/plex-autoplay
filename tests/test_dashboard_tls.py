import ssl
import tempfile
import threading
import unittest
from pathlib import Path
from urllib.request import urlopen

from plex_autoplay.infrastructure.web.server import create_dashboard_server
from plex_autoplay.infrastructure.web.tls import ensure_self_signed_certificate


class TestDashboardTLS(unittest.TestCase):
    def test_generates_certificate_with_configured_names_and_private_key_permissions(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            certificate, private_key = ensure_self_signed_certificate(
                temp_dir,
                ["plex.local", "192.168.1.20"],
            )

            self.assertTrue(certificate.is_file())
            self.assertEqual(private_key.stat().st_mode & 0o777, 0o600)
            ssl.SSLContext(ssl.PROTOCOL_TLS_SERVER).load_cert_chain(certificate, private_key)

    def test_rejects_invalid_certificate_hostnames(self):
        with tempfile.TemporaryDirectory() as temp_dir:
            with self.assertRaisesRegex(ValueError, "Nome inválido"):
                ensure_self_signed_certificate(temp_dir, ["invalid host"])

    def test_dashboard_server_serves_https_directly(self):
        def application(environ, start_response):
            start_response("200 OK", [("Content-Type", "text/plain")])
            return [environ["wsgi.url_scheme"].encode("ascii")]

        with tempfile.TemporaryDirectory() as temp_dir:
            dashboard = create_dashboard_server(
                application,
                host="127.0.0.1",
                port=0,
                https_enabled=True,
                config_dir=Path(temp_dir),
                tls_hosts=["localhost", "127.0.0.1"],
            )
            dashboard.server.prepare()
            port = dashboard.server.socket.getsockname()[1]
            worker = threading.Thread(target=dashboard.server.serve, daemon=True)
            worker.start()
            try:
                context = ssl._create_unverified_context()
                with urlopen(f"https://127.0.0.1:{port}/", context=context, timeout=5) as response:
                    self.assertEqual(response.status, 200)
                    self.assertEqual(response.read(), b"https")
            finally:
                dashboard.close()
                worker.join(timeout=5)
            self.assertFalse(worker.is_alive())

    def test_dashboard_server_keeps_explicit_http_mode_available(self):
        def application(environ, start_response):
            start_response("200 OK", [("Content-Type", "text/plain")])
            return [environ["wsgi.url_scheme"].encode("ascii")]

        with tempfile.TemporaryDirectory() as temp_dir:
            dashboard = create_dashboard_server(
                application,
                host="127.0.0.1",
                port=0,
                https_enabled=False,
                config_dir=Path(temp_dir),
                tls_hosts=["localhost"],
            )
            worker = threading.Thread(target=dashboard.run, daemon=True)
            worker.start()
            try:
                with urlopen(f"http://127.0.0.1:{dashboard.server.effective_port}/", timeout=5) as response:
                    self.assertEqual(response.status, 200)
                    self.assertEqual(response.read(), b"http")
            finally:
                dashboard.close()
                worker.join(timeout=5)
            self.assertFalse(worker.is_alive())


if __name__ == "__main__":
    unittest.main()
