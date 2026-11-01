# AI interaction logging skill

## Purpose

Use this skill for every AI-assisted interaction and task result. Append a single JSON object per interaction to `logs/ai-interactions.log` using the repository's traceability-first schema.

## Ready-to-use template

Use this exact template for each new entry:

{
  "id": "20261027T120000Z-my-task-name",
  "timestamp": "2026-10-27T12:00:00Z",
  "stage": "setup",
  "agent": "agent",
  "agent_name": "GitHub Copilot",
  "agent_type": "built-in",
  "ai_tool": "GitHub Copilot",
  "model": "MAI-Code-1.1-Flash",
  "mode": "ask",
  "related_ids": [],
  "parent_task": null,
  "context": "Short project context",
  "request": "Actual prompt or user request",
  "skills_used": [
    {
      "name": "ai-logging",
      "type": "custom",
      "purpose": "Capture and trace AI-assisted work",
      "invocation": "explicit"
    }
  ],
  "subagents_used": [],
  "mcp_tools_used": [],
  "tools_used": [
    "read_file",
    "insert_edit_into_file"
  ],
  "suggestion": "Short suggestion or plan",
  "human_decision": "Human instruction or decision",
  "action_verification": "What was checked or validated",
  "limitation": null,
  "status": "applied",
  "artifacts": [
    "path/to/file.ext"
  ],
  "outcome": "completed"
}

## Required schema

Each line in `logs/ai-interactions.log` must be a JSON object with this structure:

{
  "id": "20261019T063542Z-agent-requirement-transcription",
  "timestamp": "2026-10-19T06:35:42Z",
  "stage": "setup",
  "agent": "agent",
  "agent_name": "GitHub Copilot",
  "agent_type": "built-in",
  "ai_tool": "GitHub Copilot",
  "model": "MAI-Code-1.1-Flash",
  "mode": "ask",
  "related_ids": [],
  "parent_task": null,
  "context": "Assignment for AI in SDLC",
  "request": "Analyse Assignment requirement",
  "skills_used": [
    {
      "name": "<skill_name>",
      "type": "<built-in | custom>",
      "purpose": "<skill_purpose>",
      "invocation": "<automatic | explicit>"
    }
  ],
  "subagents_used": [
    {
      "name": "<subagent_name>",
      "type": "<built-in | custom>",
      "task": "<delegated_task>",
      "status": "<completed | failed | partial>"
    }
  ],
  "mcp_tools_used": [
    {
      "server": "<mcp_server_name>",
      "tool": "<mcp_tool_name>",
      "purpose": "<purpose>"
    }
  ],
  "tools_used": [
    "<tool_name>"
  ],
  "suggestion": "<Any suggestions>",
  "human_decision": null,
  "action_verification": "<action verification>",
  "limitation": null,
  "status": "applied",
  "artifacts": [
    "requirement/complete-requirement.md"
  ],
  "outcome": "<success | partial | rework>"
}

## Rules

- Append one JSON object per interaction.
- Never add prose or markdown to the log file.
- Use the same field names and ordering for every record.
- Include the actual prompt/request, key tools used, skills invoked, validation evidence, and outcome.
- Capture human decisions, tool usage, and limitations where relevant.
- Prefer a single JSONL-style stream—one record per line.
- Keep entries concrete and evidential, not generic.

## Logging expectations

For every future prompt or task:
- identify the `agent_name`, `agent_type`, `ai_tool`, `model`, and `mode`
- record the `request`, `context`, and any follow-up interactions
- list all `skills_used` and `subagents_used`
- document `tools_used` and any MCP integrations
- include `human_decision` and `action_verification`
- state the final `status` and `outcome`

## Purpose of the log

The goal is traceability: understand how AI was used, which capabilities were leveraged, what was validated, and whether human oversight was applied.
