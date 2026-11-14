# Spec Delta

## Purpose
Define a revamped streaming-style UI for series catalog cards, featuring polished aspect ratios, badge overlays, hover scaling, and clean dark/light theme typography.

## ADDED Requirements

### Requirement: Streaming-style series catalog cards
The system SHALL display series cards in the series catalog grid with a polished media streaming aesthetic (2/3 aspect ratio cover art, rounded corners, clean title truncation, hover scale, and subtle shadow).

#### Scenario: Series cards render with streaming aesthetic
- **WHEN** user views `/libraries/{key}/series`
- **THEN** each series card displays its thumbnail with `aspect-[2/3]`, rounded corners, and title below
- **AND** hover or keyboard focus triggers smooth scale transition and subtle overlay highlight

#### Scenario: Card click navigates to series markers
- **WHEN** user clicks or presses Enter on any series card
- **THEN** router navigates to `/series/{seriesKey}/markers` with library context
