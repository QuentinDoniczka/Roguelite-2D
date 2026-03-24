# Plan — Issue #59: Combat Movement Overhaul

## Goal
Fix character pushing, add directional flipping, arc-based attack range, and blocked retargeting.

## Approach: Hybrid (Option C)
Physics colliders for natural collision + CircleCast blocked detection + movement/attack flip logic.

## Sub-Tasks

### 1. Friction Bump
- **File**: `CharacterMover.cs` — `Awake()`
- Change `PhysicsMaterial2D.friction` from `0f` to `0.15f`

### 2. Arc Attack (360-degree range)
- **File**: `CombatController.cs` — `FixedUpdate()`
- Replace front-only rectangle check (`targetInFront && deltaX && deltaY`) with `Vector2.Distance <= _attackRange`
- Remove `VerticalRangeRatio` constant and `facingSign`/`targetInFront` logic

### 3. Flip During Movement
- **File**: `CharacterMover.cs`
- Add `FlipToward(float directionX)` method: sets `localScale.x` based on direction
- Call in both FixedUpdate branches (target + home anchor)
- Update `_faceOffset` computation to use `rawDirX` instead of `localScale.x`

### 4. Flip During Attack
- **File**: `CombatController.cs`
- Add `FaceTarget()` method: sets `localScale.x` toward target
- Call in `SetState(Attacking)` and `StartAttackSwing()`

### 5. CircleCast Blocked Detection
- **File**: `CharacterMover.cs` — `FixedUpdate()` (target branch only)
- `Physics2D.CircleCast` in movement direction before setting velocity
- If hits another character (not self, not target): stop, set `_isBlocked = true`, fire `OnBlocked` event
- Only fire event on false→true transition (no spam)
- Home anchor branch: no blocked detection

### 6. Blocked Retarget
- **File**: `CombatController.cs`
- Subscribe to `_mover.OnBlocked` in `Awake()`
- `HandleBlocked()`: if Moving, call `FindNewTarget()`, retarget if different from current
- If same or null: stay idle, wait for path to clear

## Implementation Order
Steps 1, 2, 3 are independent. Then 4 (depends on 2), 5 (depends on 3), 6 (depends on 5).

## Files Modified
- `CharacterMover.cs` — friction, flip, blocked detection
- `CombatController.cs` — arc attack, flip attack, blocked retarget
