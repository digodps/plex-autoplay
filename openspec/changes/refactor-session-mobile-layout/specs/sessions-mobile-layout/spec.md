# Spec Delta

## Purpose
Provides a responsive mobile-friendly layout for the sessions view, replacing rigid tables with stacked cards on small screens.

## ADDED Requirements

### Requirement: Responsive Sessions Layout on Mobile
The system SHALL display active sessions and skip history in a card-based layout on small screens and standard tables on desktop screens.

#### Scenario: Viewing active sessions on mobile screen
- **WHEN** user views the sessions page on a mobile screen smaller than 768px (`< md`)
- **THEN** active sessions are rendered as stacked cards displaying client, position, duration, next skip, and state

#### Scenario: Viewing active sessions on desktop screen
- **WHEN** user views the sessions page on a desktop screen of 768px or larger (`>= md`)
- **THEN** active sessions are rendered in a tabular layout
