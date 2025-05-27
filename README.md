# Personal Data Privacy Audit Tool (DPAT)

![Build Status](https://github.com/Arno-ATU/DPAT/actions/workflows/build.yml/badge.svg)
![Test Status](https://github.com/Arno-ATU/DPAT/actions/workflows/tests.yml/badge.svg)
![Security Status](https://github.com/Arno-ATU/DPAT/actions/workflows/security.yml/badge.svg)
![Database Status](https://github.com/Arno-ATU/DPAT/actions/workflows/databases.yml/badge.svg)
![Deploy Status](https://github.com/Arno-ATU/DPAT/actions/workflows/deploy.yml/badge.svg)

## Project Overview

The Personal Data Privacy Audit Tool is a web application designed to help users understand and improve their Google account privacy settings. The tool works by analyzing specific JSON files from Google Takeout, providing privacy insights without accessing sensitive personal information.

## Key Features

- Privacy-first design with local data processing
- Comprehensive privacy analysis across multiple dimensions
- Interactive dashboard with privacy scoring
- Actionable recommendations for privacy improvement

## Development

This project follows DevOps best practices with continuous integration and deployment. We maintain separate environments for development, staging, and production to ensure code quality and stability.

## Branching Strategy

This project follows a GitFlow-inspired branching strategy:

- **main**: Production-ready code. Protected and only updated through reviewed PRs from staging.
- **staging**: Pre-production testing environment. Changes are thoroughly tested here before promotion to main.
- **development**: Integration branch for features. All feature work is merged here first.
- **feature/xxx**: Feature branches are created from develop and merged back via PR.
- **hotfix/xxx**: Critical fixes that need to bypass the normal flow. Created from main and merged to both main and develop.

### Branch Workflow

1. Create feature branches from `development`
2. Submit PRs to merge completed features into `development`
3. Periodically promote `development` to `staging` for testing
4. Once verified in staging, promote to `main` for production


The full technical documentation and DevOps implementation details are available in the project report.
