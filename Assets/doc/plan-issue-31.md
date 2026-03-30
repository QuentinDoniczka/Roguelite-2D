# Plan Issue #31 — CI/CD Pipeline

## Overview
GitHub Actions CI/CD pipeline for Unity 2D project with branch protection.

## Architecture Decisions

### Test Runner
- **Game CI** (`game-ci/unity-test-runner@v4`) — community-maintained, proven with Unity 6000.x
- Image auto-resolved by game-ci from `unityVersion: 6000.3.6f1`
- Matrix strategy: EditMode and PlayMode run in parallel

### Branch Protection
- `main`: protected by `protect-main.yml` — only `release/*` and `hotfix/*` branches can target it
- `dev`: required status checks — `Tests (EditMode)` and `Tests (PlayMode)` must pass

### Cache Strategy
- Unity `Library/` folder cached per test mode
- Key: hash of `Assets/Scripts/**`, `Assets/Settings/**`, `Packages/manifest.json`, `Packages/packages-lock.json`, `ProjectSettings/*.asset`
- Fallback restore key for partial cache hits

### Unity License
- Personal license activation via `unity-activate.yml` (manual dispatch)
- `.ulf` file stored as `UNITY_LICENSE` GitHub Secret
- License may expire periodically — re-run activation workflow when needed

## Workflows

| File | Trigger | Purpose |
|------|---------|---------|
| `unity-tests.yml` | PR to `dev` | Run EditMode + PlayMode tests |
| `unity-activate.yml` | Manual dispatch | Generate `.alf` for license activation |
| `protect-main.yml` | PR to `main` | Block non-release/hotfix PRs |

## GitHub Secrets Required

| Secret | Value |
|--------|-------|
| `UNITY_LICENSE` | Contents of `.ulf` license file |
| `UNITY_EMAIL` | Unity account email |
| `UNITY_PASSWORD` | Unity account password |

## License Renewal Process
1. Go to Actions tab → "Unity License Activation" → Run workflow
2. Download the `.alf` artifact
3. Go to https://license.unity3d.com/manual
4. Upload `.alf`, download `.ulf`
5. Update `UNITY_LICENSE` secret with `.ulf` contents
