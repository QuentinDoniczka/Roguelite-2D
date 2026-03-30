# Plan Issue #31 — CI/CD Pipeline

## Overview
GitHub Actions branch protection for Unity 2D project. Tests are run locally by agents, not in CI.

## Architecture Decisions

### Branch Protection
- `main`: protected by `protect-main.yml` workflow + GitHub branch protection rules
  - Only `release/*` and `hotfix/*` branches can target it (workflow check)
  - `check-source-branch` required status check
  - PR required, no direct push, enforce admins
- `dev`: protected by GitHub branch protection rules
  - PR required, no direct push, enforce admins
  - No CI tests required (agents run tests locally before merge)

### Why no CI tests?
- Unity license activation for CI is complex and fragile (Personal license .ulf expiry)
- Agents already run EditMode and PlayMode tests locally before every merge
- Branch protection ensures no direct pushes bypass the PR workflow

## Branch Naming Conventions

| Target | Allowed branches |
|--------|-----------------|
| `dev` | `feature/*`, `fix/*`, `refactor/*`, `chore/*`, `test/*` |
| `main` | `release/*`, `hotfix/*` |

## Workflows

| File | Trigger | Purpose |
|------|---------|---------|
| `protect-main.yml` | PR to `main` | Block non-release/hotfix PRs |
| `protect-dev.yml` | PR to `dev` | Block non-feature/fix/refactor/chore/test PRs |

## GitHub Branch Protection Rules (configured via API)

| Rule | `main` | `dev` |
|------|--------|-------|
| PR required | Yes | Yes |
| Enforce admins | Yes | Yes |
| Force push blocked | Yes | Yes |
| Deletion blocked | Yes | Yes |
| Required status check | `check-source-branch` | `check-source-branch` |
