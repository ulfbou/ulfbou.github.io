# Portfolio visual system

## Purpose and scope

This document primarily governs the visual system and presentation of the portfolio area accepted at `/work/`. The portfolio is one area of the wider `ulfbou.github.io` platform; its visual and interaction requirements do not require the landing page, articles, or AWS Cloud and AI learning area to become one continuous portfolio page.

The portfolio visual system may inform a future shared platform identity, including typography, focus treatment, navigation language, and editorial tone. Unless a platform document separately adopts a rule, it remains a portfolio requirement rather than an established platform-wide rule.

The accepted portfolio allocation is `/work/`, and the supplied repository applies the portfolio visuals there through `Pages/Work.razor`, shared portfolio styles, and `wwwroot/work/index.html`.

## Editorial direction

Create one continuous editorial portfolio experience, not a CV, dashboard, template gallery, or IDE imitation. Within the portfolio, begin spaciously, use an orbit/facet motif as meaningful navigation, transition to a darker expandable work field, present artifacts as readable content, and finish with working principles.

This continuous-page direction applies to the portfolio experience, not to the entire multi-area platform. The professional landing page at `/` provides orientation and may feature portfolio summaries, while the complete editorial portfolio experience belongs at `/work/`.

The presentation must preserve a precise, reflective, technically specific tone. Project proposition, contribution, reflection, evidence, status, and limitations must remain distinguishable. Visual hierarchy must support progressive disclosure without making evidence decorative or overwhelming the project story.

## Typography, spacing, and hierarchy

Use at most display, body, and monospace roles. Prefer stable system or open fonts. Typography must preserve clear hierarchy, readable line lengths, logical headings, and distinction between narrative, controls, metadata, evidence, quotations, code, and terminal material.

Spacing must support an editorial rhythm and make project boundaries, expanded detail, related groups, and qualification text understandable. Responsive layouts may reduce spacing or simplify arrangement, but they must not collapse distinct meanings into an ambiguous visual block.

Project accents never carry meaning alone. Status, selection, evidence, limitations, errors, expanded state, and interaction feedback require textual, structural, or other non-color distinctions.

## Project and story presentation

Projects must remain scan-friendly before expansion and readable after expansion. Summary presentation must preserve project title, proposition, status, and useful technology context. Expanded presentation must preserve approved story blocks, contribution, reflection, artifacts, evidence, and limitations.

Artifacts are readable content, not decoration. Code and terminal material must remain selectable, labelled, and horizontally manageable. Evidence must retain its approved labels and relationship to the claims it supports. Limitations must remain visually associated with the meaning they qualify.

Images must use meaningful alternatives and responsive sizing. Quotations require attribution. Reflection and evidence must remain visually and semantically distinct. Expanded states must have a clear relationship to their disclosure control and must not depend on animation to be understood.

## Controls, focus, and interaction feedback

Portfolio controls must use native semantics where practical, remain operable by keyboard and touch, and expose selected or expanded state. Focus must be clearly visible against every applicable background and must not be removed or obscured.

Contrast must pass in normal, hover, focus, selected, expanded, error, disabled, and print states. Hover styling cannot be the only indication of interactivity. Interaction feedback must state or expose what changed, including filtering, expansion, loading, empty results, and failure.

Motion may clarify selection or expansion but is never required. Respect reduced motion; avoid parallax, cursor following, continuous orbiting, and disruptive layout movement.

## Responsive behavior, zoom, and reflow

Narrow layouts retain essential facts, use touch-sized controls, prevent overflow, and simplify the orbit when needed. Project titles, status, propositions, evidence, limitations, and disclosure state must remain available.

The portfolio must remain usable with browser zoom, text enlargement, and reflow. Multi-column project details, evidence grids, and diagnostic comparisons may become single-column layouts. Code and terminal material may scroll within a labelled region where wrapping would damage meaning.

Responsive changes must preserve predictable reading order and focus order. Content must not require hover, precise pointer movement, or landscape orientation.

## Print behavior

Print output must preserve the complete portfolio meaning rather than reproduce interactive filtering or collapsed state. All admitted projects and their details must be printable, including story blocks, contribution, reflection, artifacts, evidence, status, and limitations.

Navigation-only controls, orbit presentation, filter controls, and expansion toggles may be omitted from print when their operated content is included. Printed code and terminal content must wrap or otherwise remain readable without clipping. Background-dependent meaning must survive a light print presentation.

Project sections and meaningful blocks should avoid inappropriate internal page breaks where practical. Print presentation must preserve heading hierarchy, qualifications, attribution, and the association between evidence and the project it supports.

## Accessibility release requirements

Release requires landmarks, headings, native controls, visible focus, accessible labels, communicated expanded and selected state, predictable reading and tab order, no hover-only content, non-color distinctions, keyboard operation, zoom resilience, reflow, reduced-motion behavior, and meaningful image alternatives.

The portfolio must also preserve readable and selectable artifact text, sufficient contrast, understandable loading and failure states, touch usability, and print access to content otherwise disclosed interactively.

These requirements govern the portfolio and may inform shared platform identity. They do not, by themselves, define a universal platform design system or claim authority over the visual systems of `/`, `/articles/`, or `/aws/ai/`.

## Current and target behavior

### Verified current behavior

The supplied repository applies the portfolio visual system at `/work/`: the spacious opening, orbit/facet motif, dark work field, expandable projects, readable artifacts, accents, responsive rules, focus styles, reduced-motion rules, story-block presentation, and print rules exposing filtered and collapsed detail. The root professional landing page is also implemented with related typography and identity, without becoming the complete portfolio.

These files establish configured markup and styles, not measured contrast, browser, zoom, responsive, reduced-motion, print, or live-site results.

### Continuing target behavior

The portfolio visual system remains required at `/work/`. Shared platform identity may adopt compatible concerns through separate authority, but this document does not impose the portfolio presentation on articles or learning experiences.

