# Central Agent Source

Central source for all agent configurations (.agents, .claude, .continue, .opencode, .github, .dsh)

## Structure
- `skills/` - Skill definitions (shared across all clients)
- `commands/` - Command documentation (shared across all clients)  
- `workflows/` - Workflow documentation (shared across all clients)
- `prompts/` - Workflow prompts (shared across all clients)

## Usage
- Edit files in this directory only
- Run `npm run sync-agent-configs` to generate files for all clients
- Client-specific formats are preserved via adapter scripts

## Supported Clients
- .agents
- .claude
- .continue
- .opencode
- .github
- .dsh