# Rule Service Documentation

This directory contains comprehensive documentation for the Rule Service.

## Quick Links

| Section | Purpose | Start Here |
|---------|---------|------------|
| [00-overview](./00-overview/) | Problem, goals, scope | [problem-statement.md](./00-overview/problem-statement.md) |
| [01-domain](./01-domain/) | Domain model, DDD concepts | [ubiquitous-language.md](./01-domain/ubiquitous-language.md) |
| [02-architecture](./02-architecture/) | System design, ADRs | [context-diagram.md](./02-architecture/context-diagram.md) |
| [03-rule-engine](./03-rule-engine/) | Engine concepts, formulas | [evaluation-flow.md](./03-rule-engine/evaluation-flow.md) |
| [04-product-groups](./04-product-groups/) | SPR rules, lookups | [SPR/overview.md](./04-product-groups/SPR/overview.md) |
| [05-api](./05-api/) | Contract-first API docs | [runtime-apis.md](./05-api/runtime-apis.md) |
| [06-load-charts](./06-load-charts/) | Component load charts | [README.md](./06-load-charts/README.md) |
| [06-data-model](./06-data-model/) | Schema, versioning | [ruleset-versioning.md](./06-data-model/ruleset-versioning.md) |
| [07-runtime](./07-runtime/) | Evaluation, caching | [evaluation-pipeline.md](./07-runtime/evaluation-pipeline.md) |
| [08-governance](./08-governance/) | Safety, approval, audit | [approval-workflow.md](./08-governance/approval-workflow.md) |
| [09-examples](./09-examples/) | Test cases, training | [facts/spr-basic.json](./09-examples/facts/spr-basic.json) |

## For New Team Members

1. Start with [00-overview/problem-statement.md](./00-overview/problem-statement.md)
2. Review [01-domain/ubiquitous-language.md](./01-domain/ubiquitous-language.md)
3. Explore examples in [09-examples/](./09-examples/)

## For Rule Authors

1. Read [08-governance/rule-authoring-guidelines.md](./08-governance/rule-authoring-guidelines.md)
2. Review [03-rule-engine/formula-language.md](./03-rule-engine/formula-language.md)
3. Check [08-governance/validation-checklist.md](./08-governance/validation-checklist.md)
