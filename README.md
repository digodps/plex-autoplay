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

Edite o `.env` com as configurações do seu Plex:

```env
PLEX_URL=http://localhost:32400
PLEX_TOKEN=seu_token_aqui
AUTO_SKIP_INTRO=true
AUTO_NEXT_EPISODE=true
NEXT_EPISODE_THRESHOLD_SECONDS=15
POLL_INTERVAL_SECONDS=2
```

---

### Opção A: Rodando via Docker (Recomendado)

O `docker-compose.yml` já está configurado com `network_mode: host` para comunicação direta com os clientes e servidores na rede local:

```bash
docker compose up -d
```

Para visualizar os logs em tempo real:
```bash
docker compose logs -f
```

### Opção B: Instalando pela interface do ZimaOS

O arquivo [`docker-compose.zimaos.yml`](./docker-compose.zimaos.yml) pode ser importado como um aplicativo personalizado no ZimaOS. O serviço roda em um container separado do Plex e se conecta ao servidor pela rede.

1. Baixe o arquivo `docker-compose.zimaos.yml` deste repositório e abra-o em um editor de texto.
2. Altere `PLEX_URL` para o endereço do servidor Plex acessível pelo ZimaOS, por exemplo `http://192.168.1.50:32400`, e substitua `PLEX_TOKEN` pelo seu token. Não publique nem compartilhe o arquivo depois de inserir o token.
3. Na interface do ZimaOS, abra a área de aplicativos, escolha a opção para instalar/importar um aplicativo personalizado via Docker Compose e selecione o arquivo editado.
4. Inicie o aplicativo e consulte os logs no ZimaOS. Procure a mensagem de conexão bem-sucedida com o Plex.

O Compose constrói a imagem a partir do repositório público do projeto durante a instalação. A opção `network_mode: host` permite que o serviço alcance o Plex e os players na rede local. Mantenha `AUTO_NEXT_EPISODE` como `false` no primeiro teste; depois de confirmar o funcionamento, você pode mudar para `true` nas configurações do aplicativo.

O diretório `/DATA/AppData/plex-autoplay` é montado em `/config` para marcadores personalizados. Se quiser usá-los, crie `markers.json` nesse diretório com base em [`markers.example.json`](./markers.example.json). Para remoção, pare e remova o aplicativo pela interface do ZimaOS.

---

### Opção C: Rodando com Python

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

---

## 🤝 Contribuindo

Contribuições são muito bem-vindas! Sinta-se livre para abrir issues ou enviar Pull Requests. Para manter o histórico consistente, siga a [convenção de commits](./CONTRIBUTING.md).

---

## 📄 Licença

Distribuído sob a licença [MIT](LICENSE).
