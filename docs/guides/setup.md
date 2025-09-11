---
title: "Setup — Local Development"
slug: "setup-local"
date: "2025-09-04"
authors:
  - name: "Ulf Bourelius"
category: guides
tags:
  - setup
  - dev
summary: "How to get the Zentient mono-repo running locally and build the docs with CLI safeguards."
---

# Local setup

Follow these two quick steps to get started.

## Install prerequisites

- .NET SDK 7+  
- Node 18+ (optional for tooling)  
- `docfx` CLI installed globally or available in CI

## Build commands (CLI-first)

```bash
# generate metadata (validate front matter)
docfx metadata

# build with strictness: treat warnings as errors
docfx build --warningsAsErrors
