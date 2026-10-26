#!/usr/bin/env bash
# ================================================================
# setup-dev.sh  –  Configura ambiente de desenvolvimento completo
# ================================================================
# Uso: ./setup-dev.sh
# ================================================================

set -euo pipefail

cd "$(dirname "$0")"          # raiz do repo

echo "🔧  Configurando ambiente de desenvolvimento para plex-autoplay"
echo ""

# ---------------------------------------------------------------
# 1. Verificar Python
# ---------------------------------------------------------------
REQUIRED_PYTHON="3.12"
CURRENT_PYTHON=$(python3 -c "import sys; print(f'{sys.version_info.major}.{sys.version_info.minor}')")

if [[ "$(printf '%s\n' "$REQUIRED_PYTHON" "$CURRENT_PYTHON" | sort -V | head -n1)" != "$REQUIRED_PYTHON" ]]; then
    echo "❌  Python $REQUIRED_PYTHON+ requerido (encontrado $CURRENT_PYTHON)"
    exit 1
fi
echo "✅  Python $CURRENT_PYTHON OK"

# ---------------------------------------------------------------
# 2. Criar/ativar venv (opcional mas recomendado)
# ---------------------------------------------------------------
if [[ ! -d ".venv" ]]; then
    echo "📦  Criando virtualenv em .venv..."
    python3 -m venv .venv
fi

# shellcheck disable=SC1091
source .venv/bin/activate
echo "✅  Virtualenv ativado"

# ---------------------------------------------------------------
# 3. Atualizar pip/setuptools/wheel
# ---------------------------------------------------------------
echo "⬆️  Atualizando pip/setuptools/wheel..."
pip install -q --upgrade pip setuptools wheel

# ---------------------------------------------------------------
# 4. Instalar dependências do projeto
# ---------------------------------------------------------------
echo "📦  Instalando dependências (requirements.txt)..."
pip install -q -r requirements.txt

# ---------------------------------------------------------------
# 5. Instalar ferramentas de desenvolvimento
# ---------------------------------------------------------------
echo "🛠️  Instalando ferramentas de dev (ruff, pre-commit, pytest, mypy)..."
pip install -q ruff pre-commit pytest pytest-cov mypy

# ---------------------------------------------------------------
# 6. Instalar pre-commit hooks
# ---------------------------------------------------------------
echo "🪝  Instalando pre-commit hooks..."
pre-commit install
pre-commit install --hook-type commit-msg  # opcional: valida mensagens de commit

# ---------------------------------------------------------------
# 7. Rodar lint inicial para garantir que tudo passa
# ---------------------------------------------------------------
echo "🔍  Rodando lint inicial (ruff check --fix)..."
ruff check . --fix --unsafe-fixes || true
ruff format . || true

# ---------------------------------------------------------------
# 8. Rodar testes rápidos
# ---------------------------------------------------------------
echo "🧪  Rodando testes unitários..."
pytest tests/test_components.py tests/test_process_metrics.py -q

# ---------------------------------------------------------------
# 9. Verificar Docker (opcional)
# ---------------------------------------------------------------
if command -v docker &>/dev/null && command -v docker compose &>/dev/null; then
    echo "🐳  Docker/Compose disponíveis"
    echo "    Para build:  ./start.sh"
    echo "    Para logs:   ./logs.sh"
else
    echo "⚠️  Docker não encontrado – necessário para rodar o container"
fi

# ---------------------------------------------------------------
# 10. Resumo
# ---------------------------------------------------------------
echo ""
echo "================================================================"
echo "✅  Ambiente de desenvolvimento pronto!"
echo "================================================================"
echo ""
echo "Comandos úteis:"
echo "  source .venv/bin/activate     # Ativar venv (já ativo agora)"
echo "  ruff check . --fix            # Corrigir lint"
echo "  ruff format .                 # Formatar código"
echo "  pytest -q                     # Rodar todos os testes"
echo "  pytest -q -k 'nome_do_teste'  # Rodar teste específico"
echo "  ./start.sh                    # Build + up containers"
echo "  ./logs.sh                     # Ver logs"
echo "  pre-commit run --all-files    # Rodar todos os hooks manualmente"
echo ""
echo "Arquivos de config criados/verificados:"
echo "  .pre-commit-config.yaml       # Hooks de pre-commit"
echo "  pyproject.toml                # Config ruff/mypy/pytest"
echo "  .venv/                        # Virtualenv (ignorado pelo git)"
echo ""
echo "Happy coding! 🚀"
