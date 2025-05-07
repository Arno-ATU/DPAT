## Branching Strategy

This project follows a GitFlow-inspired branching strategy:

- **main**: Production-ready code. Protected and only updated through reviewed PRs from staging.
- **staging**: Pre-production testing environment. Changes are thoroughly tested here before promotion to main.
- **develop**: Integration branch for features. All feature work is merged here first.
- **feature/xxx**: Feature branches are created from develop and merged back via PR.
- **hotfix/xxx**: Critical fixes that need to bypass the normal flow. Created from main and merged to both main and develop.

### Branch Workflow

1. Create feature branches from `develop`
2. Submit PRs to merge completed features into `develop`
3. Periodically promote `develop` to `staging` for testing
4. Once verified in staging, promote to `main` for production
