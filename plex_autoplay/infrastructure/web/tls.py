"""Self-signed TLS certificate management for direct dashboard HTTPS."""

from __future__ import annotations

import ipaddress
import os
import re
import shutil
import subprocess
import tempfile
from pathlib import Path

_DNS_LABEL_RE = re.compile(r"^[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?$")


def _certificate_subject_alt_names(hosts: list[str]) -> tuple[list[str], str]:
    normalized_hosts = []
    subject_alt_names = []
    for value in hosts:
        host = value.strip().rstrip(".")
        if not host:
            continue
        try:
            address = ipaddress.ip_address(host)
        except ValueError:
            try:
                hostname = host.encode("idna").decode("ascii")
            except UnicodeError as error:
                raise ValueError(f"Nome inválido para o certificado TLS: {host}") from error
            if len(hostname) > 253 or not all(_DNS_LABEL_RE.fullmatch(label) for label in hostname.split(".")):
                raise ValueError(f"Nome inválido para o certificado TLS: {host}")
            normalized_hosts.append(hostname)
            subject_alt_names.append(f"DNS:{hostname}")
        else:
            normalized_hosts.append(str(address))
            subject_alt_names.append(f"IP:{address}")

    if not subject_alt_names:
        raise ValueError("Configure ao menos um nome ou endereço IP em DASHBOARD_TLS_HOSTS.")
    return subject_alt_names, normalized_hosts[0]


def ensure_self_signed_certificate(config_dir: str | Path, hosts: list[str]) -> tuple[Path, Path]:
    config_path = Path(config_dir)
    config_path.mkdir(parents=True, exist_ok=True)
    certificate_path = config_path / "dashboard-tls.crt"
    private_key_path = config_path / "dashboard-tls.key"
    if certificate_path.is_file() and private_key_path.is_file():
        return certificate_path, private_key_path

    openssl = shutil.which("openssl")
    if openssl is None:
        raise RuntimeError("openssl não está instalado; não é possível criar o certificado HTTPS do dashboard.")
    subject_alt_names, common_name = _certificate_subject_alt_names(hosts)
    with tempfile.TemporaryDirectory(prefix=".dashboard-tls-", dir=config_path) as temporary_directory:
        temporary_path = Path(temporary_directory)
        temporary_certificate = temporary_path / "dashboard-tls.crt"
        temporary_key = temporary_path / "dashboard-tls.key"
        subprocess.run(
            [
                openssl,
                "req",
                "-x509",
                "-newkey",
                "rsa:2048",
                "-nodes",
                "-sha256",
                "-days",
                "825",
                "-keyout",
                str(temporary_key),
                "-out",
                str(temporary_certificate),
                "-subj",
                f"/CN={common_name}",
                "-addext",
                f"subjectAltName={','.join(subject_alt_names)}",
            ],
            check=True,
            capture_output=True,
            text=True,
            timeout=30,
        )
        os.chmod(temporary_key, 0o600)
        os.chmod(temporary_certificate, 0o644)
        os.replace(temporary_key, private_key_path)
        os.replace(temporary_certificate, certificate_path)
    return certificate_path, private_key_path
