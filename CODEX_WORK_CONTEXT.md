# Codex Work Context

## Connection Notes

- 2026-05-31: Connected successfully from home on MacBook Air 13.
- Current workspace path on Mac:
  `/Volumes/Extreme SSD/UnityProject/project/CharacterSortPuzzle_git`
- User plans to continue tomorrow from academy on a Windows desktop.

## Project Notes

- Unity project: `CharacterSortPuzzle_git`
- Current runner prototype is under `Assets/Scripts2`.
- Preserve the existing 2-door runner gate flow.
- Extend the new quiz gate as a separate 3-door system instead of overwriting the working 2-door scripts.

## Next Expected Work

- Review/update from Windows desktop environment.
- Start with the 3-door quiz gate plan when implementation resumes:
  `QuizData`, `QuizManager`, `DoorData`, `QuizDoorController`, `GateQuizController`, `DoorModelLibrary`, `WallFailTrigger`.

## 3-Door Quiz Notes

- Quiz types should include `Add`, `Multiply`, and `Luck`.
- `Luck` quiz question label should be exactly `U+2190 or U+2192, ?` (displayed as left arrow, `or`, right arrow, comma, question mark).
- `Luck` quiz must create three doors:
  - two open-door prefabs
  - one closed-door prefab, always present
- For the two open doors, only one is the success door; the other is a fail door.
- The closed door is always fail, uses the closed-door prefab, and should have empty TMP text.
- Recommended door data shape:
  - `DoorResultType.Luck` + `isSuccessDoor = true` for the lucky pass door
  - `DoorResultType.Luck` + `isSuccessDoor = false` for the lucky fail door
  - `DoorResultType.Closed` + `isSuccessDoor = false` for the closed door
- Luck door labels should be one left-arrow label and one right-arrow label; labels do not have to match physical slot position.
- Add/Multiply quizzes still use `Answer`, `Wrong`, and a third special door (`Luck` or `Closed`).
