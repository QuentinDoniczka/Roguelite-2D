# Plan — Issue #80: Rework Target Distribution

## Approach: Attack Slot Registry + LeastContested Targeting

### Root Cause
`CharacterMover.cs` uses a fixed `FaceOffset = 0.25f` — ALL attackers aim for the same point `(target.x ± 0.25, target.y)`. When multiple characters target the same enemy, they stack.

### Sub-Tasks

#### 1. Create `AttackSlotRegistry.cs` (new static class)
- Manages positional slots around each target
- Slot layout: front-center first, fan out vertically, then behind (overflow)
- API: `Acquire()`, `Release()`, `ReleaseAll()`, `AttackerCount()`, `Clear()`
- Data: `Dictionary<Transform, List<SlotEntry>>`

#### 2. Modify `CombatController.cs` — Slot lifecycle
- Acquire slot on `Target` set, release on retarget/death/destroy/disengage
- Pass `_slotOffset` to `CharacterMover.SetSlotOffset()`
- Range check uses slot position, not raw target center
- New field `_attackerFacesRight` set at spawn

#### 3. Modify `CharacterMover.cs` — Dynamic slot offset
- Remove fixed `FaceOffset` constant
- Add `SetSlotOffset(Vector2)` method
- Navigate to `target.position + _slotOffset` instead of fixed offset

#### 4. Add `LeastContested()` to `TargetFinder.cs`
- Score = distance + attackerCount * ContestPenalty (1.0f)
- Distributes attackers more evenly across targets

#### 5. Modify `LevelManager.cs` — Wire LeastContested + Clear
- Replace `TargetFinder.Closest` with `LeastContested` in retarget delegates
- Call `AttackSlotRegistry.Clear()` on level complete/lost

#### 6. Modify `CombatSpawnManager.cs` — Wire ally facing
- Call `SetAttackerFacing(true)` on spawned allies

#### 7. Modify `CombatStats.cs` — ReleaseAll on death
- Call `AttackSlotRegistry.ReleaseAll(transform)` in TakeDamage when IsDead

### Implementation Order
1 → 3 → 2 → 4 → 7 → 6 → 5

### Files
| Action | File |
|--------|------|
| Create | `Assets/Scripts/Combat/AttackSlotRegistry.cs` |
| Modify | `Assets/Scripts/Combat/CharacterMover.cs` |
| Modify | `Assets/Scripts/Combat/CombatController.cs` |
| Modify | `Assets/Scripts/Combat/TargetFinder.cs` |
| Modify | `Assets/Scripts/Combat/CombatStats.cs` |
| Modify | `Assets/Scripts/Combat/CombatSpawnManager.cs` |
| Modify | `Assets/Scripts/Combat/LevelManager.cs` |

### Critical Risk
Attack range check in `CombatController.FixedUpdate()` MUST use `target.position + _slotOffset` not raw `target.position`. Otherwise characters oscillate Moving/Attacking.
