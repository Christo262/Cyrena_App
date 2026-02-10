# Contributing to Cyréna

Thanks for your interest in contributing!

Cyréna is an offline-first AI engineering assistant focused on **structured, reliable software workflows**. The project prioritizes architecture clarity, predictability, and long-term maintainability over clever hacks or rapid feature dumping.

This document explains how to contribute in a way that keeps the system stable and extensible.

---

## Philosophy

Cyréna follows a few core principles:

- Structure > cleverness
- Predictability > freedom
- The AI must be constrained by architecture
- Specifications must reflect real code
- Avoid hidden behavior and magic state

When in doubt, choose the solution that will still make sense six months from now.

---

## Before You Start

Please:

- Search existing issues before opening a new one
- Discuss large changes before implementing them
- Keep pull requests focused and scoped
- Avoid mixing refactors with features

Large architectural changes should be proposed first, not introduced as surprise pull requests.

---

## Development Setup

1. Clone or fork the repository
2. Build using the standard .NET toolchain
3. Run Cyréna locally
4. Use included example/test projects to validate behavior

If you add new behavior, test it by running real workflows inside the app.

---

## Contribution Workflow

1. Fork the repository
2. Create a feature branch from `main`
3. Implement your change
4. Test locally
5. Submit a pull request

Please do not commit directly to `main`.

Each pull request should focus on a single logical change. Smaller PRs are easier to review and merge.

---

## Code Guidelines

- Prefer simple, explicit code
- Avoid hidden global state
- Avoid magic strings and implicit behavior
- Fail visibly, not silently
- Keep async flows understandable
- Do not swallow exceptions

Architecture clarity matters more than micro-optimizations.

---

## Event Engine & Core Systems

The event engine and project lifecycle are sensitive areas.

When modifying them:

- Preserve ordering guarantees
- Avoid blocking operations
- Keep concurrency bounded
- Maintain predictable state transitions

If unsure, open a discussion before changing behavior.

---

## UI Changes

Cyréna is a developer tool, not a visual showcase.

- Prioritize clarity over styling
- Avoid heavy UI frameworks
- Keep the interface lightweight and functional

---

## Bug Reports

When submitting a bug report, please include:

- Chat export
- Crash log (if available)
- Clear reproduction steps
- Environment details

Good bug reports dramatically reduce debugging time.

---

## Pull Request Expectations

Pull requests should:

- Explain the problem being solved
- Describe the chosen approach
- Avoid unrelated cleanup
- Keep changes focused and reviewable

Small, understandable PRs are preferred over large rewrites.

---

## What Not to Contribute

Please avoid:

- Feature dumps without discussion
- Logging frameworks or telemetry systems
- Heavy new dependencies
- Architecture rewrites without consensus

Cyréna intentionally stays lightweight.

---

## Final Notes

This project is evolving quickly. Breaking changes are expected.

The goal is not perfection — the goal is a strong foundation that can grow.

Thanks for helping build Cyréna.
