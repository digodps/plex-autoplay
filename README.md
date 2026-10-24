# plex-autoplay 🍿⏭️

Automação para **Plex Media Server** projetada para proporcionar uma verdadeira experiência de maratona (*binge-watching*) contínua, **100% gratuita e sem necessidade de Plex Pass**.

O **`plex-autoplay`** atua como um serviço companheiro (*sidecar*) em segundo plano, monitorando as sessões ativas do Plex e assumindo o controle do player remotamente para:
1. **Pular a introdução** (Skip Intro) automaticamente assim que o ponto de abertura for alcançado.
2. **Avançar para o próximo episódio** imediatamente ao término do episódio atual (créditos finais), sem exibir telas de contagem e sem precisar tocar no controle remoto.

---

## ✨ Funcionalidades

- 🚫 **Sem Plex Pass**: Pule introduções e maratone séries automaticamente mesmo no plano gratuito do Plex.
- ⏭️ **Auto Skip Intro**: Pula a abertura usando marcadores nativos do Plex, arquivo local de tempos (`markers.json`) ou pulo padrão configurável.
- 🎬 **Auto Next Episode**: Identifica os créditos finais (ou threshold configurável, ex: últimos 15s) e comanda o player para iniciar o próximo episódio na sequência.
- 🛋️ **Experiência Hands-Free Universal**: Funciona com qualquer player conectado na sua rede (Smart TV LG/Samsung, Chromecast, Android TV, Fire TV, Apple TV, Web, etc.).
- 🎯 **Filtro por Usuários e Dispositivos**: Escolha quais aparelhos ou perfis da casa terão o autoplay ativado.
- ⚡ **WebSocket em Tempo Real + Polling de Fallback**: Reação imediata a eventos de reprodução combinada com verificação periódica de segurança.
- 🖥️ **Dashboard de TV shows**: Inventário genérico de episódios, cobertura de marcadores, cadastro de bibliotecas e edição manual autenticada; geração automática via Jikan/AniSkip permanece exclusiva para Anime.

## 📂 Requisito de organização das mídias

Para usar a geração automática de marcadores de anime, a biblioteca deverá seguir este padrão:

```text
<biblioteca>/
└── <Nome da série>/
    └── Temporada <número>/
        └── <nome do episódio> -<número do episódio>.<extensão>
```

Exemplo:

```text
Animes/
└── Kyouran Reijou Nia Liston/
    └── Temporada 1/
        └── Kyouran Reijou Nia Liston Byoujaku Reijou ni Tensei shita Kamigoroshi no Bujin no Karei Naru Musouroku -01.mkv
```

O nome da pasta da série é a referência principal para localizar o anime; `Temporada <número>` informa a temporada local; e o sufixo numérico após o hífen no nome do arquivo informa o episódio, com ou sem zeros à esquerda. A descoberta aceita extensões de vídeo comuns e ignora arquivos que não correspondam ao padrão. Na varredura automática do startup, associações incertas são estimadas e registradas nos logs; na CLI interativa, é possível confirmar a seleção. Os diretórios de mídia devem ser montados no container em modo somente leitura.

Este padrão é usado pelo comando de geração de marcadores descrito em [Geração de marcadores de anime](#-geração-de-marcadores-de-anime). Veja a [especificação da funcionalidade](./docs/specs/automatic-anime-markers.md).

---

## 🛠️ Como Funciona

```
                 ┌────────────────────────────────────────────────────────┐
                 │                   Plex Media Server                    │
                 │              (Servidor Local ou Docker)                │
                 └──────┬──────────────────────────────────▲──────────────┘
                        │ 1. WebSocket Alerts              │ 4. HTTP Player Command
                        │    (posição, estado, player)     │    (seekTo / playMedia)
                        ▼                                  │
          ┌────────────────────────────────────────────────┴──────────────┐
          │                    plex-autoplay (Serviço)                    │
          │                                                               │
          │  [Session Watcher]  ───>  [Marker Manager] ───> [Controller]  │
          │  Escuta eventos em         Verifica intro e      Dispara pulo │
          │  tempo real via            créditos (nativos     ou próximo   │
          │  PlexAPI                   ou markers.json)      episódio     │
          └───────────────────────────────────────────────────────────────┘
                                                │
                                                ▼
                                    ┌───────────────────────┐
                                    │    Smart TV / Player  │
                                    │  (Pula sem intervenção│
                                    │   humana na tela)     │
                                    └───────────────────────┘
```

1. **Monitoramento**: O serviço conecta-se ao Plex via WebSocket e mantém acompanhamento contínuo da reprodução.
2. **Ponto de Corte**: Ao atingir o timestamp da abertura, envia silenciosamente o comando `seekTo` para o player pular para o fim da intro.
3. **Próximo Episódio**: Ao atingir a tela de créditos (ou os últimos segundos configurados), busca o próximo episódio da temporada e comanda o player via `playMedia`.

---

## 📋 Pré-requisitos

- **Plex Media Server** configurado e acessível na rede local.
- **Plex Token** (X-Plex-Token) para autenticação das chamadas de API ([como obter seu token](https://support.plex.tv/articles/204059436-finding-an-authentication-token-x-plex-token/)).
- **Configuração no Player Plex (TV/Celular/Web)**:
  - No app do Plex do seu dispositivo, certifique-se de que a opção **"Permitir controle remoto"** (*Advertise as player* / *Allow remote control*) está ativada (já vem ativa por padrão nas configurações avançadas do player).
- **Python 3.10+** ou **Docker**.

---

## 🚀 Instalação e Execução

### 1. Clonar o Repositório

```bash
git clone https://github.com/digodps/plex-autoplay.git
cd plex-autoplay
```

### 2. Configurar o Ambiente

Copie o arquivo de exemplo de configuração:

```bash
cp .env.example .env
```

Edite o `.env` com as configurações do seu Plex e os caminhos do host:

```env
PLEX_URL=http://host.docker.internal:32400
PLEX_TOKEN=seu_token_aqui
MEDIA_ROOT_HOST=/caminho/no/host/que-contem-as-bibliotecas
WEB_HOST_PORT=8080
AUTO_SKIP_INTRO=true
AUTO_NEXT_EPISODE=true
NEXT_EPISODE_THRESHOLD_SECONDS=15
POLL_INTERVAL_SECONDS=2
```

O Compose usa rede bridge, então `localhost` em `PLEX_URL` apontaria para o próprio container. Para um Plex no mesmo host, `host.docker.internal` é mapeado para o gateway do host; para outro servidor, informe um nome DNS ou endereço IP acessível pelo container. Escolha em `MEDIA_ROOT_HOST` uma pasta que contenha todas as bibliotecas que deseja cadastrar. Ela será montada uma única vez, somente para leitura, em `/media-root`.

### Dashboard de bibliotecas

Após iniciar o container, acesse `http://<endereço-do-host>:<WEB_HOST_PORT>` (porta padrão `8080`). No primeiro acesso, crie a conta administrativa; nos acessos seguintes, entre com essa conta. Pela interface, selecione subpastas disponíveis sob a raiz de mídia configurada, classifique cada biblioteca como Anime ou Série e inicie uma varredura para consultar episódios e cobertura de marcadores.

Use **Gerenciar bibliotecas** para navegar pelas subpastas disponíveis, adicionar bibliotecas como Anime ou Série e remover cadastros sem apagar os arquivos. O painel consulta o estado da varredura a cada cinco segundos; **Executar varredura** inicia uma atualização em segundo plano, e o botão fica indisponível enquanto houver uma execução ativa. Ao concluir, o painel recarrega o inventário e a cobertura. Uma solicitação concorrente é recusada e o estado atual continua visível.

As credenciais administrativas, bibliotecas cadastradas, marcadores e relatórios são persistidos em `/config`. O Compose local mapeia esse diretório para `./config`; o exemplo ZimaOS usa `/DATA/AppData/plex-autoplay`. Proteja esse volume como dado sensível e mantenha o dashboard em uma rede local confiável. Para acesso remoto, use um proxy reverso com HTTPS e configure `DASHBOARD_COOKIE_SECURE=true`; não exponha o serviço diretamente à Internet por HTTP.

Ao iniciar, o serviço agenda uma varredura em segundo plano de todas as bibliotecas cadastradas. O inventário e a cobertura são genéricos; somente bibliotecas classificadas como Anime podem consultar Jikan/AniSkip, se `AUTO_GENERATE_ANIME_MARKERS=true`. Uma falha em uma biblioteca é registrada sem interromper o watcher Plex nem as demais bibliotecas. O relatório mais recente fica em `/config/scan-report.json`; uma nova solicitação enquanto a varredura está ativa é recusada até a execução atual terminar.

---

### Opção A: Rodando via Docker (Recomendado)

O `docker-compose.yml` usa rede bridge, publica a interface web e monta uma única raiz de mídia em modo somente leitura:

```bash
docker compose up -d
```

Para visualizar os logs em tempo real:
```bash
docker compose logs -f
```

### Opção B: Instalando pela interface do ZimaOS

O arquivo [`docker-compose.zimaos.yml`](./docker-compose.zimaos.yml) pode ser importado como um aplicativo personalizado no ZimaOS. O serviço roda em um container separado do Plex e usa rede bridge. O workflow do GitHub Actions publica `ghcr.io/digodps/plex-autoplay:latest` no GHCR a cada atualização da branch `main`; antes de importar, confirme que o pacote está público no GitHub Packages para que o ZimaOS possa baixar a imagem sem autenticação.

1. Antes da migração, faça backup de `/DATA/AppData/plex-autoplay`, especialmente de `markers.json`.
2. Na aba **Packages** do repositório GitHub, abra as configurações do pacote `plex-autoplay` e defina a visibilidade como pública. Se o pacote ainda não existir, aguarde a primeira execução bem-sucedida do workflow.
3. Edite `PLEX_URL`, `PLEX_TOKEN` e o `source` do mount `/media-root` em `docker-compose.zimaos.yml` para corresponder ao seu servidor. Não publique nem compartilhe o token.
4. Na interface do ZimaOS, importe o Compose. Ele usa a imagem pública `ghcr.io/digodps/plex-autoplay:latest`; não é necessário compilar a imagem no ZimaOS.
5. Inicie o aplicativo, acesse `http://<endereço-do-zimaos>:8080` e cadastre as pastas de Anime e Séries em **Gerenciar bibliotecas**.

As bibliotecas antigas montadas individualmente devem estar dentro da nova raiz escolhida. Se estiverem em caminhos separados, escolha uma pasta pai comum ou reorganize os mounts antes da migração. A lista de bibliotecas é armazenada em `/config/libraries.json`; cadastre novamente as subpastas pela interface após atualizar o Compose. O volume antigo `/config` mantém `markers.json` e o cache de metadados.

---

### Opção C: Rodando com Python

Crie o `.env` como acima e configure `MEDIA_ROOT` para uma pasta local de mídia; `MEDIA_ROOT_HOST` só é usado pelo Compose. As dependências incluem Flask e Waitress para o dashboard, servido na porta interna `WEB_PORT` (padrão `8080`).

```bash
# Crie e ative o ambiente virtual
python3 -m venv .venv
source .venv/bin/activate

# Instale as dependências
pip install -r requirements.txt

# Inicie o serviço
python -m plex_autoplay
```

---

### Opção C: Rodando como Serviço do Sistema (`systemd`) no Linux

Crie o arquivo `/etc/systemd/system/plex-autoplay.service`:

```ini
[Unit]
Description=Plex Autoplay and Skip Intro Service
After=network.target

[Service]
Type=simple
User=diego
WorkingDirectory=/var/home/diego/Documentos/Digodps/plex-autoplay
ExecStart=/var/home/diego/Documentos/Digodps/plex-autoplay/.venv/bin/python -m plex_autoplay
Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
```

Ative e inicie o serviço:
```bash
sudo systemctl daemon-reload
sudo systemctl enable --now plex-autoplay
```

---

## 🎯 Configuração de Marcadores de Abertura (`markers.json`)

Para séries em que você deseja definir os tempos exatos de início e fim da abertura (ou créditos), você pode criar um arquivo `markers.json` (veja [markers.example.json](file:///var/home/diego/Documentos/Digodps/plex-autoplay/markers.example.json)):

```json
{
  "Stranger Things": {
    "default": {
      "start": 125,
      "end": 195,
      "credits": 2800
    },
    "season_1": {
      "1": {
        "start": 0,
        "end": 65
      }
    }
  },
  "The Office": {
    "default": {
      "start": 80,
      "end": 115
    }
  }
}
```
*Os tempos são informados em segundos.*

---

## 🎌 Geração de marcadores de anime

Quando executado pelo Docker Compose fornecido, o serviço inventaria no startup as bibliotecas cadastradas no dashboard. Todas as bibliotecas recebem descoberta e cobertura genéricas; somente as classificadas como Anime podem gerar marcadores por Jikan/AniSkip quando `AUTO_GENERATE_ANIME_MARKERS=true`. Essa é uma varredura no startup, não um monitoramento contínuo de alterações nas pastas. A raiz única é montada como `/media-root:ro`; nenhum arquivo de vídeo é enviado às APIs. `ffprobe` obtém a duração real quando necessário.

No Compose, configure `MEDIA_ROOT_HOST` com uma pasta que contenha os diretórios de mídia e cadastre as bibliotecas pela interface. O exemplo Compose habilita o enriquecimento Anime com `AUTO_GENERATE_ANIME_MARKERS=true`; fora desses arquivos, a opção é desabilitada por padrão e pode ser ativada por variável de ambiente. No startup, a seleção de candidatos e o offset de temporadas podem ser estimados automaticamente; as estimativas e itens pendentes são registrados nos logs. Falhas na geração não impedem o serviço Plex de iniciar.

Também é possível solicitar uma prévia manual:
```bash
python -m plex_autoplay \
  --media-root /media/anime \
  --output /config/markers.json \
  --cache-file /config/metadata-cache.json
```

Em uma sessão interativa, o comando pode pedir a seleção de obra e o offset da numeração de episódios por temporada quando necessário; esses mapeamentos são guardados no cache local. Sem escolha interativa, itens ambíguos ficam pendentes, a menos que `--auto-guess` seja informado. A prévia identifica dados ausentes e episódios pendentes. Para apenas visualizar sem persistir marcadores:

```bash
python -m plex_autoplay --media-root /media/anime --output /config/markers.json --dry-run
```

A gravação exige confirmação. `--yes` confirma explicitamente em automações não interativas; `--force` também exige confirmação e substitui campos já existentes. Sem `--force`, os marcadores existentes são preservados e somente campos ausentes são preenchidos. O cache expira após 168 horas por padrão (`--cache-ttl-hours` altera o prazo). Após resposta HTTP 429, novas consultas ficam pausadas pelo período informado em `Retry-After` (ou 60 segundos quando ausente).

---

## 📁 Estrutura do Projeto

```text
plex-autoplay/
├── .env.example            # Modelo de variáveis de ambiente
├── .gitignore              # Arquivos ignorados pelo Git
├── docker-compose.yml      # Configuração do Docker Compose
├── Dockerfile              # Imagem do container Docker
├── DESCRIPTION.txt         # Descrição do projeto para o GitHub
├── LICENSE                 # Licença MIT
├── markers.example.json    # Exemplo de configuração de marcadores
├── README.md               # Documentação completa
├── requirements.txt        # Dependências do projeto
├── plex_autoplay/
│   ├── __init__.py
│   ├── __main__.py                  # Ponto de entrada: python -m plex_autoplay
│   ├── config.py                   # Leitura e validação de configurações
│   ├── main.py                     # Composição e ciclo de vida do serviço
│   ├── domain/
│   │   ├── __init__.py
│   │   ├── models.py               # Estado de reprodução
│   │   └── playback_rules.py       # Regras puras de reprodução
│   ├── application/
│   │   ├── __init__.py
│   │   ├── ports.py                # Contratos dos adaptadores
│   │   └── playback_service.py     # Caso de uso de monitoramento
│   └── infrastructure/
│       ├── markers/
│       │   ├── __init__.py
│       │   └── json_repository.py  # Marcadores locais e nativos do Plex
│       └── plex/
│           ├── __init__.py
│           ├── client.py           # Conexão e comandos do Plex
│           └── session_watcher.py  # WebSocket e polling
└── tests/
    └── test_components.py          # Testes das regras e componentes
```

---

## 🧪 Testes

Execute a suíte de testes com:

```bash
python3 -m unittest discover tests
```

## 🧭 OpenSpec

O projeto usa OpenSpec para propor, especificar e acompanhar mudanças. No VS Code, reinicie a janela após configurar o OpenSpec para carregar os workflows do GitHub Copilot.

Use `/opsx:explore` para investigar uma ideia e `/opsx:propose` para criar uma proposta com requisitos e tarefas. Depois da implementação, use `/opsx:verify` quando disponível e `/opsx:archive` para arquivar a mudança concluída. O CLI também pode listar e validar artefatos:

```bash
openspec list
openspec validate --all
```

Consulte [openspec/config.yaml](./openspec/config.yaml) para o contexto do projeto e a documentação oficial do [OpenSpec](https://openspec.dev/docs/).

---

## 🤝 Contribuindo

Contribuições são muito bem-vindas! Sinta-se livre para abrir issues ou enviar Pull Requests. Para manter o histórico consistente, siga a [convenção de commits](./CONTRIBUTING.md).

---

## 📄 Licença

Distribuído sob a licença [MIT](LICENSE).
