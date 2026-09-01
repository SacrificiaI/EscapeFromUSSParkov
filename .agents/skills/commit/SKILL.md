---
name: commit
description: >
  Split the working tree into atomic, well-scoped commits using this
  project's branch- and commit-naming conventions. Use when asked to
  "commit," "commit atomically," or "commit this" for a working tree that
  mixes more than one logical change. Triggers on: /commit.
argument-hint: 'Optional: a scope to commit (default: everything pending)'
user-invocable: true
---

# /commit

**Project-scoped skill** — assumes this repository's own git history as the
style precedent; travels with the rest of `.Codex/`, not standalone.

## Branch naming

- `feature/brief-name-of-specific-task` — general work toward a specific
  idea. Can run long if the task warrants it
  (`feature/brief-name-could-go-quite-long-if-important`).
- `fix/brief-name-for-a-fix` — a fix for a feature already "completed," or
  general bug-fixing.

## Commit message format

`<Prefix>: <Active-present-tense description>`

- **Feat:** a reasonably grouped set of files that does a task.
  `Feat: Adds player movement and basic collisions`
- **Chore:** items that aren't fixes — usually dependencies, configs, or
  reorganization. `Chore: Adjusts dependencies and analyzers for an unused
  CA5079`
- **Fix:** fixes for features already merged to main.
  `Fix: Changes player movement values so to prevent crashing`
- **Docs:** adjustments for documentation.
  `Docs: Includes text explanations for our stack`

The verb right after the prefix is active-present tense describing what the
commit *does* ("Adds", "Changes", "Categorizes", "Routes"), not imperative
mood ("Add") and not "Fixes X" repeating the prefix. Subject line stays
under ~70 characters; put detail in the body instead of stretching the
subject.

Body (when the change needs one — a one-line rename doesn't):
paragraph form, explains what changed and why, calls out anything verified
(build/test results, `.tscn`/scene references checked before a file move,
values preserved across a move). Close with a verification line when the
project has one to report, e.g. `Build: 0 warnings, 0 errors. Tests: 86/86
(unchanged).` No `Co-Authored-By` trailer on `Fix:`/`Chore:`/`Docs:` commits
by default — reserve it (if at all) for commits that already carry one in
recent `git log`, which trends toward larger milestone-style work.

## Splitting a mixed working tree into atomic commits

A commit is atomic when it's a single reviewable idea that leaves the tree
in a working state on its own. When a working tree mixes several such
ideas, split along these lines, in this order:

1. **Unrelated pre-existing debris first.** If `git status` shows stale,
   uncommitted leftovers from earlier work that have nothing to do with the
   current task (an orphaned deletion, a forgotten untracked file), give
   that its own small `Fix:`/`Chore:` commit before touching anything else,
   rather than silently folding it into unrelated work. Say plainly what it
   is and where it came from if that's discoverable (`git log`, `git
   blame`).
2. **Behavior changes, one logical fix per commit.** Two unrelated bug
   fixes are two commits even if found in the same session — don't bundle
   "fixed A" and "fixed B" into one `Fix:` unless B only exists because of
   A.
3. **Pure reorganization separate from behavior changes.** A file move/
   rename with no content change is its own `Chore:`, cleanly separable
   from any fix that happens to touch the same file — commit the fix first
   (at whatever path the file is at when the fix lands), then the
   reorganization commit covers everything else that moved, using the
   pathspec form below so already-committed files aren't re-included.
4. **Documentation last, as its own `Docs:` commit**, describing the
   end state after the code commits that precede it — not interleaved
   hunk-by-hunk with the code changes it documents.

## Committing only specific files: `git commit -- <pathspec>`

To land part of a staged/unstaged tree without touching the rest:

```
git commit -F <message-file> -- path/one path/two
```

This computes the commit from the current `HEAD` vs. those specific paths
(working tree content for unstaged changes, index content for staged ones),
leaving everything else pending for a later commit. Preferred over `git
add -p` for splitting by whole file rather than by hunk.

**Windows gotcha:** when `core.ignorecase=true` (default on Windows/macOS),
this pathspec form is unreliable for a **pure case-only rename** — a folder
renamed only in casing (`enums/` → `Enums/`) with no other path segment
changed. Git's dirty-check does a `stat()` on the path, which succeeds
case-insensitively and reports "unchanged," so both `git commit --
<pathspec>` and even `git reset` can silently drop the rename entirely —
not error, just skip it with no trace. `git mv` itself still stages it
correctly (it writes the index directly, no stat comparison), and a
**bare** `git commit -m "..."` with nothing else staged commits it fine,
because a pathspec-less commit uses the index as-is with no HEAD-vs-
working-tree comparison to fall into the trap. So: for a pure case rename,
isolate it by staging *only* that rename (`git mv`, nothing else touched),
commit with no pathspec, then move on to the next batch. A rename that also
moves into a new directory segment (`ChargeMeter.cs` → `Player/
ChargeMeter.cs`) is unaffected — only the *pure* case-only case collapses.

## Verification before committing

Run this project's actual build/test commands (see `AGENTS.md`) before the
first commit and confirm the result to quote in commit bodies — don't
assert "0 warnings, 0 errors" without having just run it.

## Example

```
Working tree: one bug fix in Player.cs, a folder rename touching 12 files
across two assemblies, and a leftover uncommitted deletion from three
commits ago.

1. Fix: Stages leftover old-path deletion from three commits ago
2. Fix: Routes Player's closest-sun direction through SimMath
3. Chore: Categorizes Sim into PascalCase folders
4. Chore: Categorizes Game's classes/ into PascalCase folders
5. Docs: Records the new folder categories in AGENTS.md
```
