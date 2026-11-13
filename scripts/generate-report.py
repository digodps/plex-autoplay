#!/usr/bin/env python3
"""Gera relatório HTML consolidado de uma change OpenSpec.

Árvore de alterações (estilo githud/copilot), relatórios QA por iteração,
rodadas de correção e evidências de teste.
Saída: openspec/changes/<change>/report.html (ponto de entrada único, sem deps).
"""
import argparse
import html
import os
import re
import subprocess
import sys

ICON = {"A": "✅", "M": "🛠️", "D": "❌", "R": "➡️", "T": "📝", "?": "📂"}
COLOR = {"A": "#16a34a", "M": "#d97706", "D": "#dc2626", "R": "#2563eb", "T": "#7c3aed", "?": "#64748b"}


def git(repo, *args):
    r = subprocess.run(["git", "-C", repo, *args], capture_output=True, text=True)
    return r.stdout.strip()


def changed_files(repo, base):
    """path -> [status, added, deleted] unindo commits do branch e não commitado."""
    files = {}
    status_map = {}

    if base:
        names = git(repo, "diff", "--name-status", f"{base}...HEAD")
        for line in names.splitlines():
            m = re.match(r"([ACDMRT])\d*\t(.+)", line)
            if m:
                status_map[m.group(2)] = m.group(1)
        stats = git(repo, "diff", "--numstat", f"{base}...HEAD")
    else:
        stats = ""

    for line in stats.splitlines():
        parts = line.split("\t")
        if len(parts) == 3:
            add, dele, path = parts
            files[path] = [status_map.get(path, "M"), add, dele]

    # alterações não commitadas (working tree vs HEAD)
    unstaged = git(repo, "diff", "--name-status", "HEAD")
    for line in unstaged.splitlines():
        m = re.match(r"([ACDMRT ])\t(.+)", line)
        if m:
            status_map.setdefault(m.group(2), m.group(1).strip() or "?")
    stats_u = git(repo, "diff", "--numstat", "HEAD")
    for line in stats_u.splitlines():
        parts = line.split("\t")
        if len(parts) == 3:
            add, dele, path = parts
            if path not in files:
                files[path] = [status_map.get(path, "?"), add, dele]

    # arquivos novos não rastreados (git diff ignora untracked)
    untracked = git(repo, "ls-files", "--others", "--exclude-standard")
    for path in untracked.splitlines():
        if path and path not in files:
            try:
                with open(os.path.join(repo, path), encoding="utf-8", errors="ignore") as f:
                    nlines = sum(1 for _ in f)
            except OSError:
                nlines = 0
            files[path] = ["A", str(nlines), "0"]
    return files


def build_tree(files):
    """Diretórios aninhados. Devolve raiz e totais."""
    root = {"children": {}, "add": 0, "dele": 0, "is_file": False, "info": None}
    totals = {"A": 0, "M": 0, "D": 0, "R": 0, "T": 0, "add": 0, "dele": 0, "files": 0}

    for path, (status, add, dele) in files.items():
        add_i = int(add) if add.isdigit() else 0
        dele_i = int(dele) if dele.isdigit() else 0
        node = root
        parts = path.split("/")
        for i, part in enumerate(parts):
            is_file = i == len(parts) - 1
            if part not in node["children"]:
                node["children"][part] = {
                    "children": {}, "is_file": is_file, "info": None,
                    "add": 0, "dele": 0,
                }
            node = node["children"][part]
            if is_file:
                node["info"] = (status, add_i, dele_i)
                node["add"] = add_i
                node["dele"] = dele_i
                totals[status] = totals.get(status, 0) + 1
                totals["add"] += add_i
                totals["dele"] += dele_i
                totals["files"] += 1
            else:
                node["add"] += add_i
                node["dele"] += dele_i
    return root, totals


def render_tree(node, name):
    """HTML recursivo estilo árvore (detalhes aninhados)."""
    lines = []
    kids = sorted(node["children"].items(), key=lambda kv: (not kv[1]["is_file"], kv[0]))
    if node["is_file"]:
        status, add, dele = node["info"]
        icon = ICON.get(status, "📄")
        color = COLOR.get(status, "#64748b")
        lines.append(
            f'<div class="file" style="color:{color}">'
            f'{icon} <b>{html.escape(name)}</b> <span class="stat">+{add} -{dele}</span>'
            f' <span class="tag">{status}</span></div>'
        )
        return lines

    has_kids = bool(kids)
    add = node["add"]; dele = node["dele"]
    if not name:
        for child_name, child in kids:
            lines.extend(render_tree(child, child_name))
        return lines
    head = f'{html.escape(name)}/ <span class="stat">+{add} -{dele}</span>'
    if has_kids:
        lines.append(f'<details class="dir"><summary>{head}</summary>')
        for child_name, child in kids:
            lines.extend(render_tree(child, child_name))
        lines.append("</details>")
    else:
        lines.append(f'<div class="emptydir">{head}</div>')
    return lines


def md_to_html(text):
    """Conversor mínimo de markdown (sem dependências)."""
    text = text.replace("\r\n", "\n").replace("\r", "\n")
    blocks = re.split(r"\n{2,}", text)
    out = []
    for block in blocks:
        if not block.strip():
            continue
        m = re.match(r"```(\w+)?\n(.*?)\n```$", block, flags=re.S)
        if m:
            out.append(f'<pre><code class="lang-{html.escape(m.group(1) or "")}">{html.escape(m.group(2))}</code></pre>')
            continue
        hm = re.match(r"#{1,6} (.+)$", block)
        if hm:
            level = len(block) - len(block.lstrip("#"))
            out.append(f"<h{level}>{inline(block[level + 1:].strip())}</h{level}>")
            continue
        items = re.findall(r"^- (.+)$", block, flags=re.M)
        if items:
            lis = "".join(f"<li>{inline(i)}</li>" for i in items)
            out.append(f"<ul>{lis}</ul>")
            continue
        out.append(f"<p>{inline(block)}</p>")
    return "\n".join(out)


def inline(s):
    s = html.escape(s)
    s = re.sub(r"\*\*(.+?)\*\*", r"<strong>\1</strong>", s)
    s = re.sub(r"`([^`]+)`", r"<code>\1</code>", s)
    s = re.sub(r"\[([^\]]+)\]\(([^)]+)\)", r'<a href="\2">\1</a>', s)
    return s.replace("\n", "<br>")


def collect_qa(change_dir):
    """Lista de qa-reports ordenados por número."""
    reports = []
    if not os.path.isdir(change_dir):
        return reports
    for name in sorted(os.listdir(change_dir)):
        m = re.match(r"qa-report-(\d+)(?:-final)?\.md$", name)
        if not m:
            continue
        with open(os.path.join(change_dir, name), encoding="utf-8") as f:
            content = f.read()
        reports.append({
            "file": name,
            "iteration": int(m.group(1)),
            "status": "PASS" if re.search(r"^PASS\b", content, re.M) else ("FAIL" if re.search(r"^FAIL\b", content, re.M) else "?"),
            "content": content,
            "urls": re.findall(r"https?://\S+", content),
        })
    return reports


def collect_evidence(repo):
    """Artefatos de teste disponíveis (Playwright, screenshots, traces, logs)."""
    evidence = []
    candidates = [
        "test-artifacts/playwright-report/index.html",
        "tests/e2e/playwright-report/index.html",
        "test-results/.last-run.json",
    ]
    for rel in candidates:
        p = os.path.join(repo, rel)
        if os.path.exists(p):
            evidence.append((rel, p))
    # screenshots/traces soltos
    for root, dirs, fs in os.walk(os.path.join(repo, "test-artifacts")):
        for f in fs:
            rel = os.path.relpath(os.path.join(root, f), repo)
            if rel not in [e[0] for e in evidence]:
                evidence.append((rel, os.path.join(root, f)))
    return evidence


def main():
    ap = argparse.ArgumentParser(description="Relatório HTML OpenSpec")
    ap.add_argument("change")
    ap.add_argument("--repo", default=".")
    ap.add_argument("--base", default=None, help="commit base do diff (padrão: merge-base main/HEAD)")
    ap.add_argument("--out", default=None)
    args = ap.parse_args()

    repo = os.path.abspath(args.repo)
    change_dir = os.path.join(repo, "openspec", "changes", args.change)
    out = args.out or os.path.join(change_dir, "report.html")

    base = args.base or git(repo, "merge-base", "main", "HEAD") or git(repo, "rev-parse", "HEAD~1") or None
    files = changed_files(repo, base)
    out_rel = os.path.relpath(out, repo)
    files.pop(out_rel, None)  # não listar o próprio relatório
    root, totals = build_tree(files)
    tree_html = "\n".join(render_tree(root, ""))

    qa = collect_qa(change_dir)
    evidence = collect_evidence(repo)
    qa_sections = ""
    for r in qa:
        badge = 'class="badge pass"' if r["status"] == "PASS" else ('class="badge fail"' if r["status"] == "FAIL" else 'class="badge unk"')
        qa_sections += (
            f"<details><summary>📋 {html.escape(r['file'])} — <span {badge}>{r['status']}</span></summary>"
            f"{md_to_html(r['content'])}"
        )
        if r["urls"]:
            qa_sections += "<p><strong>URLs de validação/evidência:</strong><ul>"
            for u in r["urls"]:
                qa_sections += f'<li><a href="{html.escape(u)}">{html.escape(u)}</a></li>'
            qa_sections += "</ul></p>"
        qa_sections += "</details>"

    ev_html = ""
    if evidence:
        ev_html = "<h2>🔎 Evidências de teste</h2><ul>"
        for rel, p in evidence:
            link = os.path.relpath(p, os.path.dirname(out))
            ev_html += f'<li><a href="{html.escape(link)}">{html.escape(rel)}</a></li>'
        ev_html += "</ul>"

    html_doc = f"""<!DOCTYPE html>
<html lang="pt-BR"><head><meta charset="utf-8">
<title>Relatório OpenSpec — {html.escape(args.change)}</title>
<style>
body{{font-family:system-ui,-apple-system,Segoe UI,Roboto,sans-serif;margin:24px;line-height:1.5;color:#1f2937}}
h1{{font-size:22px;margin-bottom:4px}} h2{{font-size:18px;margin-top:28px;border-bottom:1px solid #e5e7eb;padding-bottom:6px}}
h3{{font-size:16px;margin-top:20px}}
.badge{{font-size:11px;padding:2px 7px;border-radius:99px;color:#fff;font-weight:600}}
.badge.pass{{background:#16a34a}}.badge.fail{{background:#dc2626}}.badge.unk{{background:#64748b}}
.summary{{display:flex;gap:16px;margin:16px 0;border:1px solid #e5e7eb;border-radius:8px;padding:12px;background:#f9fafb}}
.summary div{{flex:1}}
.summary .big{{font-size:20px;font-weight:700;color:#111827}}
.tree{{font-size:13px;line-height:1.6}}
.details>summary,details>summary{{font-weight:600;cursor:pointer;list-style:none}}
details>summary::marker{{content:"▸"}}
.file{{margin-left:14px;padding:2px 0;border-left:2px solid #e5e7eb}}
.stat{{color:#6b7280;font-size:11.5px}}
.tag{{font-size:10px;padding:1px 5px;border-radius:3px;color:#fff;margin-left:6px}}
.emptydir{{color:#9ca3af;font-style:italic;margin-left:14px}}
code{{background:#f3f4f6;padding:1px 4px;border-radius:4px;font-size:0.9em}}
pre{{background:#1f2937;color:#e5e7eb;padding:10px;border-radius:6px;overflow:auto}}
a{{color:#2563eb;text-decoration:none}} a:hover{{text-decoration:underline}}
.meta{{color:#6b7280;font-size:12.5px;margin-top:8px}}
</style></head>
<body>
<h1>Relatório OpenSpec — {html.escape(args.change)}</h1>
<div class="meta">Worktree: {html.escape(repo)} &nbsp;|&nbsp; Base do diff: {html.escape(base or "sem base")} &nbsp;|&nbsp; Relatório: <b>{html.escape(out)}</b></div>

<div class="summary">
 <div><span class="big">{totals['files']}</span> arquivos alterados</div>
 <div><span class="big" style="color:#16a34a">+{totals['add']}</span> linhas adicionadas</div>
 <div><span class="big" style="color:#dc2626">-{totals['dele']}</span> linhas removidas</div>
 <div><span class="big">{len(qa)}</span> rodada(s) de QA</div>
 <div><span class="big">{len(evidence)}</span> evidência(s)</div>
</div>

<h2>🌳 Árvore de alterações (adicionado / modificado / removido)</h2>
<div class="tree">{tree_html}</div>
<p class="meta">Totais: A={totals['A']} M={totals['M']} D={totals['D']} R={totals['R']} &nbsp;•&nbsp; +{totals['add']} / -{totals['dele']}</p>

<h2>🧪 Relatórios QA e rodadas de correção</h2>
{qa_sections if qa_sections else "<p>Nenhum qa-report-*.md encontrado.</p>"}

{ev_html}

<p class="meta">Próximos passos: revisar árvore e QA acima, então executar <code>/opsx-archive</code> para arquivar a change ou manter a worktree <code>feature/spec-{html.escape(args.change)}</code> para merge.</p>
</body></html>"""

    os.makedirs(os.path.dirname(out), exist_ok=True)
    with open(out, "w", encoding="utf-8") as f:
        f.write(html_doc)
    print(out)


if __name__ == "__main__":
    main()
