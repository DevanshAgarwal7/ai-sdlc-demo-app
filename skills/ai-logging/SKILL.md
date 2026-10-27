# AI interaction logging skill

## Purpose

Use this skill whenever an AI interaction or task result is completed. Append a structured JSON record to `logs/ai-interactions.log` in the exact repository format.

## Required format

Each line in `logs/ai-interactions.log` must be a single JSON object:

{"timestamp":"2026-10-27T12:00:00Z","session":"ai-session-2026-10-27","agent":"GitHub Copilot","event":"user-interaction","summary":"Short description of the work or prompt","repository":"c:\\Skill-up\\AI-in-SDLC\\ai-sdlc-demo-app"}

## Rules

- Append one JSON object per interaction.
- Do not write prose or markdown in the log file.
- Use the same field names and ordering every time.
- Include a short summary of the user request or completed task.
- Keep the summary concise but specific.

## Logging commands

- Manual: `pwsh ./scripts/log-ai-interaction.ps1 "summary"`
- Git hook: `.githooks/pre-commit`
