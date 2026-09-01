---
name: verify-doc
description: >
  Audit a technical reference document (architecture doc, performance guide,
  ADR, rule file) against live official documentation instead of trained
  knowledge, and fix what's wrong. Use when asked to verify, fact-check,
  audit, or check the correctness/currency of a doc — built for the
  Godot/.NET vault docs but works for any technical doc with checkable
  claims.
argument-hint: 'Path to the doc to audit'
user-invocable: true
---

# Verify Doc

**Portable, project-agnostic skill** — self-contained; works for anyone who
has only this file.

Trained knowledge about API surfaces, version numbers, and benchmark figures
goes stale silently — it reads exactly as confident as knowledge that's still
correct. This skill exists to replace "does this sound right" with "does the
current official source actually say this."

## Failure Classes to Check For

1. **API/engine correctness** — does the class, method, attribute, or
   signature shown actually exist, with that name and signature, in the
   cited engine/runtime version? Verify against docs.godotengine.org (Godot)
   or learn.microsoft.com (C#/.NET), not memory.
2. **Version currency** — is something framed as "current," "future,"
   "preview," or "potential" actually still true as of today's date? Check
   release dates, not assumptions — a version can ship between when a doc
   was written and when it's read.
3. **Unsourced precision** — a specific number (a percentage, a nanosecond
   figure, a byte count) stated as settled fact with no citation backing it.
   If no primary source states that number, either find one or soften the
   claim to the defensible reasoning behind it — keep the reasoning, drop
   the false precision.
4. **Link and wikilink validity** — does every citation URL resolve to a
   page that still supports the claim (not moved, not reorganized out from
   under it)? Do internal `[[wikilinks]]` point at files that still exist?
5. **Code that doesn't compile** — every snippet presented as "the right
   way" should actually type-check against the stated language/version
   rules (e.g. `stackalloc` requires an unmanaged type; a `params
   ReadOnlySpan<T>` parameter requires a C# version that supports it).
6. **Cross-doc consistency** — does this doc's guidance contradict a sibling
   doc's settled position (a deferred/Phase-10 concept like Fix64 or
   lockstep leaking into an MVP-scoped doc, for example)? Check the doc's
   own `related:` wikilinks and this project's rule files for a conflicting
   verdict before assuming the doc under review is the one that's right.

## Procedure

1. Read the target doc in full before checking anything — don't spot-check
   from a skim.
2. Extract every checkable claim and sort it into the failure classes above.
   Don't stop at the first plausible-sounding claim; API names and specific
   numbers are exactly where trained knowledge drifts quietest.
3. Past roughly 10-15 independent claims, split verification across
   parallel research (general-purpose agents scoped by section — e.g.
   "Godot API claims" vs ".NET/C# runtime claims" — each doing live
   WebFetch/WebSearch against primary sources, not memory). Spot-check any
   subagent citation that would actually change a fix, rather than relaying
   it uncritically — a subagent's doc quote can itself be stale or
   misreadable, exactly like the knowledge this skill exists to catch.
4. Categorize every finding:
   - **Correctness bug** — teaches something actually wrong or unsafe. Fix
     immediately.
   - **Stale** — was true, dated by a version or release that's since
     shipped or changed. Fix.
   - **Unsourced precision** — defensible reasoning, unverifiable specific
     number. Soften, don't delete.
   - **Verified-correct** — leave untouched. Don't rewrite for style.
5. Apply fixes surgically. Touch only the sentence or code block that's
   wrong — don't rewrite adjacent correct content, don't restructure
   headings unless the heading itself is inaccurate, and preserve the doc's
   existing voice and formatting conventions (Obsidian callouts, wikilinks,
   table-of-contents anchors — if a heading changes, check the ToC entry
   pointing at it still matches).
6. Report back categorized, not just "fixed a few things" — which claims
   were correctness bugs, which were stale, which were softened, and which
   were checked and left alone. The categorization is the point: it tells
   the user which failure class to watch for the next time they write a
   similar doc.

## Output Expectations

State, per finding: what was wrong, what changed, and the category (bug /
stale / unsourced precision). If a section was checked and needed nothing,
say so explicitly rather than silently skipping it, so "checked, found
nothing" stays distinguishable from "not checked."
