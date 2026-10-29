# Spec Delta

## Purpose

Define o comportamento de preferência de tema do usuário: seleção de cor de destaque a partir de uma paleta pré-definida, escolha de modo claro/escuro/seguindo o sistema, persistência no banco de dados e aplicação da preferência ao carregar a aplicação.

## ADDED Requirements

### Requirement: User can select accent color
The system SHALL allow an authenticated user to select an accent color from a predefined palette of colors, which is applied to interactive elements such as buttons, focus rings, and active navigation items.

#### Scenario: User selects a new accent color
- **WHEN** the user picks a color from the palette on the home screen
- **THEN** the system updates the accent color immediately and persists the choice

#### Scenario: Accent color defaults to Plex amber
- **WHEN** no accent color preference is saved for the user
- **THEN** the system applies the default accent color #e5a00d

### Requirement: User can choose theme mode
The system SHALL allow an authenticated user to choose between light mode, dark mode, and system-follow mode, overriding the operating system preference when a manual choice is made.

#### Scenario: User forces dark mode
- **WHEN** the user selects dark mode on the home screen
- **THEN** the system applies a dark background and light text regardless of the OS color scheme

#### Scenario: User forces light mode
- **WHEN** the user selects light mode on the home screen
- **THEN** the system applies a light background and dark text regardless of the OS color scheme

#### Scenario: User selects system-follow mode
- **WHEN** the user selects system-follow mode
- **THEN** the system follows the operating system's color-scheme preference

### Requirement: Theme preference persists across sessions
The system SHALL store the user's accent color and theme mode in the database and reapply them when the user reopens the application.

#### Scenario: Preference is loaded on app start
- **WHEN** the authenticated user opens the home screen after previously saving a theme
- **THEN** the system applies the saved accent color and theme mode without requiring re-selection

### Requirement: Theme is scoped to the authenticated user
The system SHALL associate each theme preference with the authenticated user so that different users can have different themes.

#### Scenario: User-specific theme isolation
- **WHEN** one user saves a dark mode preference and another user logs in
- **THEN** the second user sees their own saved theme, not the first user's
