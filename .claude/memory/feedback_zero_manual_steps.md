---
name: Zero Manual Steps Principle
description: Never ask the user to manually configure Unity assets — automate everything via YAML editing, Editor scripts, or AssetDatabase. Agent-improver must be triggered when workflow failures occur.
type: feedback
---

Never ask the user to do manual configuration in Unity Editor. Everything that can be automated MUST be automated: Animator controller parameters/transitions (edit .controller YAML), prefab wiring (AssetDatabase.LoadAssetAtPath + SerializedObject), scene setup (Editor scripts with [MenuItem]).

**Why:** The user was asked to manually add Animator parameters, create transitions, and assign a prefab reference — all of which could have been done programmatically. This wasted time and frustrated the user.

**How to apply:** When any agent (brainstorm, leaddev, dev-ux, dev-unity) identifies a task, it must classify it as "automatable" and specify HOW. The only acceptable manual user actions are: clicking a menu item to run a setup script, and pressing Play to test. When a workflow step fails or requires user intervention, the lead must trigger the `agent-improver` agent to analyze and fix the relevant agent prompts.
