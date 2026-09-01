# Research, Review, and Handoff

Use this reference for live research, technical verification, course audits,
reader testing, or assignments to another author.

## Source hierarchy

Use the strongest available source for each kind of claim:

1. **Target implementation truth:** source, manifests, lock files, tests,
   configuration, build output, runtime behavior, and immutable reference state.
2. **Primary technical authority:** official versioned documentation,
   specifications, standards, source repositories, release notes, papers, and
   developer postmortems.
3. **Validated educational authority:** well-maintained tutorials, books,
   university material, vendor or industry certification courses, and
   established educators with reproducible examples.
4. **Community evidence:** issue reports, discussions, forums, and informal
   posts used to discover edge cases, terminology, or common confusion.

Do not use educational reputation as a substitute for verifying an API or
behavior. Do not use official API documentation as proof that a lesson sequence
is pedagogically effective. Record each source's role.

## Validate an online tutorial or course

Evaluate the specific resource, not only its publisher:

- author and publisher expertise relevant to the topic;
- publication and update dates;
- pinned language, framework, tool, and platform versions;
- links to primary sources or a public reference implementation;
- code that can be built, tested, or otherwise reproduced;
- corrections, maintenance history, and handling of known limitations;
- clear separation between fact, opinion, and product preference;
- evidence that the sequence produces visible working outcomes;
- fit for the intended reader rather than generic popularity.

For every technical claim adopted from a secondary source, verify it against the
target implementation, current primary documentation, or a reproducible test.
If access is unavailable or the claim cannot be confirmed, omit it, soften it to
an explicitly bounded observation, or mark it as unresolved in the plan. Never
present an inaccessible course's marketing description as validated content.

Use strong educational sources for their teaching moves: meaningful achievable
goals, concrete-to-abstract progression, focused explanatory blocks, rapid
write/run feedback, complete running examples, direct adoption decisions,
troubleshooting, retrieval practice, and transfer. Re-express those moves in an
original curriculum appropriate to the topic.

### Starting discovery set

These are useful starting points, not a closed whitelist. Recheck the relevant
page during the current research pass and apply the validation rubric above.

- **Diátaxis tutorials** — learning-oriented practical activity, meaningful
  achievable goals, visible results, and instructor responsibility for a safe
  path: `https://diataxis.fr/tutorials/`
- **CodeCrafters authoring guidance** — focused stage instructions, explicit
  tests, concept-sized explanatory blocks, and writing for one known developer:
  `https://docs.codecrafters.io/contributors/authoring-challenges/writing-stage-instructions`
  `https://docs.codecrafters.io/contributors/authoring-concepts/style-guide/byte-sized-blocks`
  `https://codecrafters.io/blog/writing-for-developers`
- **Ship That Code** — cumulative systems built from an empty file, rapid
  write/run feedback, and executable graded outcomes:
  `https://shipthatcode.com/`
- **Mukesh Murugan / Code With Mukesh** — long-form, junior-facing architecture
  tutorials that combine definition, adoption choice, implementation, testing,
  and troubleshooting in one running example:
  `https://codewithmukesh.com/`
- **Official vendor learning paths** — Microsoft Learn, Unity Learn, Unreal
  Engine learning material, Godot documentation, AWS Skill Builder, and the
  equivalent maintained portal for the selected stack. Treat certification as
  evidence of an intentional curriculum, then verify currency and technical
  claims like any other secondary source.
- **Canonical books, specifications, papers, and production postmortems** — use
  them when they directly explain the topic's model, tradeoffs, or failure
  behavior. Carry their language, platform, and era constraints into the
  interpretation.

Discover additional sources for the actual topic. Prefer resources with public
working artifacts and precise version scope. A fashionable publisher, large
audience, paid certificate, or polished presentation does not bypass validation.

## Live technical verification

Browse or otherwise retrieve current primary sources whenever a claim can vary
by version, platform, release, package, toolchain, law, standard, or service.
Pin documentation to the versions used by the course. Re-open cited pages during
the lesson session rather than relying on a stored URL or trained knowledge.

For each checkable claim:

1. Write the exact claim narrowly enough to test.
2. Identify the primary source and applicable version/date/platform.
3. Locate the passage, signature, behavior, or release note that supports it.
4. Compare it with the target code and executable behavior.
5. Record caveats and scope without expanding them into unrelated teaching.
6. Cite the primary source close to the claim and list sources actually used.

Prefer two independent forms of evidence for load-bearing or high-risk claims:
documentation plus a build/test/runtime check, specification plus implementation,
or official docs plus a primary production account.

When sources conflict:

- the pinned target implementation defines what the current course actually
  builds;
- current official versioned documentation defines supported public behavior;
- specifications outrank remembered behavior;
- tutorials and blog posts do not overrule primary sources;
- if the project intentionally uses legacy or unsupported behavior, teach that
  exact state with a clear version boundary rather than silently modernizing it.

Use short quotations only when exact wording matters. Summarize and cite rather
than reproducing substantial source text, code, exercises, or paid material.

## Source ledger

Maintain a compact ledger in the master plan:

| Source | Role | Version/date | Claims or teaching moves | Routed lessons | Verification status |
|---|---|---|---|---|---|

Roles include implementation truth, primary technical authority, pedagogy
exemplar, architecture judgment, experience report, and edge-case discovery.
An AI-generated account may suggest questions to investigate but is not a
technical source. Verify its useful claims independently and do not cite it as
authority in a lesson.

## Audit order

For a read-only review, lead with evidence-backed findings ordered by impact:

1. correctness and unsafe instructions;
2. broken prerequisites, missing contracts, or misleading outcomes;
3. unverifiable or stale technical claims;
4. examples that do not build, run, or match the stated state;
5. sequence, repetition, cognitive load, and hidden optional dependencies;
6. tone, navigation, formatting, and retrieval quality.

For each finding, provide:

- exact file and location;
- observed problem;
- why it affects learning or correctness;
- concrete correction;
- source or executable evidence when technical.

Do not edit during a critique unless authorized. If authorized, make the
smallest coherent correction and rerun the relevant lesson and course checks.

## Course-wide acceptance

Check:

- the deliverable tree, plan counts, filenames, headings, and index agree;
- every internal link and glossary target resolves;
- each lesson has one unique objective and correct minimum prerequisite;
- required paths do not depend on optional material;
- terminology, code contracts, versions, and ranges remain consistent;
- every working snippet maps to a verified reference state;
- the cumulative example evolves without parallel replacement examples;
- chapter spines and sub-lessons do not duplicate one another;
- failures are routed to the lesson where readers first have enough context;
- source citations support the nearby claims and use the right version;
- the complete basic path reaches a useful stopping point before advanced
  branches;
- the capstone and exercises test transfer, not answer-shaped recall;
- the output renders correctly in its publication system.

For a build-along, have a reader who did not author it follow the instructions
from a clean checkout or clean workspace without live rescue. Record and repair
the first ambiguous instruction, unexpected result, missing prerequisite, and
environment assumption. Pilot optional branches separately so basic-path readers
do not pay their setup cost.

## Execution handoff

Assign one bounded lesson by default. A handoff must be self-contained enough
for the receiving author to act without rereading the entire drafting history.

```text
Task:
Destination and canonical plan:
Allowed files and supporting edits:
Files that must remain unchanged:
Reader and lesson role:
Minimum prerequisite and required reading:
Single observable outcome:
Exact H2 sequence:
Reference state before and after:
Frozen contracts, APIs, versions, ranges, and critical snippets:
Implementation or reasoning order:
Required primary and educational sources:
Verification commands, runtime checks, and expected evidence:
Failure modes and deliberate omissions:
Lesson-format checklist:
Stop conditions and completion report:
```

Tell the receiving author to inspect current files and status before editing,
preserve unrelated changes, and stop after the assigned lesson and its required
supporting artifacts. Do not delegate unresolved architecture or curriculum
decisions; settle them in the plan first.
