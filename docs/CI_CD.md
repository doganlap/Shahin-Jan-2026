# CI/CD Pipeline Documentation

## Overview

The GRC System uses GitHub Actions for Continuous Integration and Continuous Deployment. This document describes the CI/CD pipelines, their configuration, and how to use them.

## CI Pipeline

### Trigger Events

The CI pipeline runs on:
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches

### Jobs

#### 1. Build and Test

**File:** `.github/workflows/ci.yml`

**Steps:**
- Checkout code
- Setup .NET 8.0 SDK
- Cache NuGet packages
- Restore dependencies
- Build solution in Release mode
- Run code analysis
- Run tests (if test projects exist)
- Upload test results

**Artifacts:**
- Test results (if available)
- Code coverage reports

#### 2. Security Scan

**Steps:**
- Checkout code
- Run Snyk security scan
- Upload security scan results

**Requirements:**
- `SNYK_TOKEN` secret must be configured in GitHub repository settings

#### 3. Docker Build

**Steps:**
- Checkout code
- Setup Docker Buildx
- Build API image
- Build Blazor image
- Build Migrator image

**Note:** Only runs on push events (not pull requests)

#### 4. Code Quality

**Steps:**
- Checkout code
- Setup .NET SDK
- Install SonarCloud (if configured)
- Run SonarCloud analysis

**Requirements:**
- `SONAR_TOKEN` secret must be configured
- SonarCloud project must be set up

#### 5. CodeQL Analysis

**File:** `.github/workflows/codeql.yml`

**Steps:**
- Checkout code
- Initialize CodeQL
- Autobuild
- Perform CodeQL analysis

**Schedule:** Runs weekly on Sunday at midnight UTC

## CD Pipeline

### Trigger Events

The CD pipeline runs on:
- Push to `main` branch
- Tags starting with `v*` (e.g., `v1.0.0`)
- Manual workflow dispatch

### Jobs

#### 1. Build and Push Docker Images

**File:** `.github/workflows/deploy.yml`

**Steps:**
- Checkout code
- Setup Docker Buildx
- Login to GitHub Container Registry
- Extract metadata (tags, labels)
- Build and push Docker images for:
  - API Host
  - Blazor
  - DbMigrator

**Image Tags:**
- Branch name (for branches)
- Semantic version (for tags)
- SHA (for commits)
- `latest` (for default branch)

**Platforms:** linux/amd64, linux/arm64

#### 2. Deploy to Staging

**Trigger:** Push to `main` or manual dispatch with `staging` environment

**Steps:**
- Checkout code
- Deploy to staging environment

**Environment:** `staging`
**URL:** https://staging.grc.example.com

#### 3. Deploy to Production

**Trigger:** Tag starting with `v*` or manual dispatch with `production` environment

**Steps:**
- Checkout code
- Deploy to production environment
- Run smoke tests

**Environment:** `production`
**URL:** https://grc.example.com

#### 4. Notify Deployment

**Steps:**
- Send deployment notifications

## Configuration

### Required Secrets

Configure these secrets in GitHub repository settings (Settings → Secrets and variables → Actions):

1. **SNYK_TOKEN** (optional)
   - Snyk API token for security scanning
   - Get from: https://app.snyk.io/account

2. **SONAR_TOKEN** (optional)
   - SonarCloud authentication token
   - Get from: https://sonarcloud.io/account/security

3. **GITHUB_TOKEN** (automatic)
   - Automatically provided by GitHub Actions
   - Used for GitHub Container Registry authentication

### Environment Variables

Set in repository settings or workflow files:

- `DOTNET_VERSION`: .NET SDK version (default: `8.0.x`)
- `SOLUTION_FILE`: Solution file path (default: `Grc.sln`)
- `REGISTRY`: Container registry (default: `ghcr.io`)
- `IMAGE_PREFIX`: Image name prefix (default: `{owner}/grc`)

## Usage

### Running CI Manually

CI runs automatically on push/PR, but you can also trigger manually:

1. Go to Actions tab in GitHub
2. Select "CI" workflow
3. Click "Run workflow"
4. Select branch and click "Run workflow"

### Running CD Manually

1. Go to Actions tab in GitHub
2. Select "Deploy" workflow
3. Click "Run workflow"
4. Select:
   - Branch
   - Environment (staging/production)
5. Click "Run workflow"

### Creating a Release

1. Create and push a tag:
   ```bash
   git tag -a v1.0.0 -m "Release version 1.0.0"
   git push origin v1.0.0
   ```

2. This triggers:
   - Build and push with version tags
   - Deploy to production
   - Run smoke tests

## Workflow Status

### Viewing Workflow Runs

1. Go to GitHub repository
2. Click "Actions" tab
3. Select workflow from left sidebar
4. Click on a run to see details

### Workflow Badges

Add to README.md:

```markdown
![CI](https://github.com/your-org/grc-system/workflows/CI/badge.svg)
![Deploy](https://github.com/your-org/grc-system/workflows/Deploy/badge.svg)
```

## Troubleshooting

### Build Failures

1. **Check logs:**
   - Go to Actions → Failed workflow → Failed job
   - Review error messages

2. **Common issues:**
   - Missing dependencies
   - Compilation errors
   - Test failures
   - Code analysis errors

### Deployment Failures

1. **Check environment:**
   - Verify environment secrets are set
   - Check environment protection rules

2. **Check permissions:**
   - Verify GitHub token has required permissions
   - Check registry access

### Security Scan Failures

1. **Snyk:**
   - Verify `SNYK_TOKEN` is set
   - Check Snyk project configuration
   - Review vulnerability reports

2. **CodeQL:**
   - Check CodeQL analysis results
   - Review security alerts

## Best Practices

### Branch Strategy

- **main:** Production-ready code
- **develop:** Development branch
- **feature/***: Feature branches

### Commit Messages

Use conventional commits:
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation
- `refactor:` Code refactoring
- `test:` Tests
- `chore:` Maintenance

### Pull Requests

- Create PR from feature branch to `develop` or `main`
- CI runs automatically on PR
- Review CI results before merging
- Require CI to pass before merge

### Releases

- Use semantic versioning (v1.0.0)
- Create release notes
- Tag releases
- Deploy to production automatically

## Customization

### Adding New Jobs

Edit `.github/workflows/ci.yml` or `.github/workflows/deploy.yml`:

```yaml
jobs:
  new-job:
    name: New Job
    runs-on: ubuntu-latest
    steps:
      - name: Step 1
        run: echo "Hello"
```

### Adding New Environments

1. Add environment in repository settings
2. Configure environment secrets
3. Add deployment job in workflow

### Custom Deployment

Modify deployment steps in `deploy.yml`:

```yaml
- name: Deploy
  run: |
    # Your deployment commands
    kubectl apply -f k8s/
    # or
    docker-compose -f docker-compose.prod.yml up -d
```

## Monitoring

### Workflow Notifications

Configure notifications in repository settings:
- Email notifications
- Slack integration
- Teams integration

### Metrics

Track:
- Build success rate
- Deployment frequency
- Mean time to recovery (MTTR)
- Lead time for changes

## Additional Resources

- [GitHub Actions Documentation](https://docs.github.com/actions)
- [GitHub Container Registry](https://docs.github.com/packages/guides/about-github-container-registry)
- [Docker Buildx](https://docs.docker.com/buildx/)
- [Snyk Documentation](https://docs.snyk.io/)
- [SonarCloud Documentation](https://docs.sonarcloud.io/)

## Additional Workflows

### Dependency Review

**File:** `.github/workflows/dependency-review.yml`

Automatically reviews dependencies in pull requests for:
- Security vulnerabilities
- License compliance
- Outdated packages

**Trigger:** Pull requests to `main` or `develop`

### Release Workflow

**File:** `.github/workflows/release.yml`

Builds and pushes Docker images when a GitHub release is published.

**Trigger:** GitHub release published

**Actions:**
- Builds release images with version tags
- Pushes to GitHub Container Registry
- Tags images with release version and `latest`

---

## GitHub Templates

### Pull Request Template

**File:** `.github/PULL_REQUEST_TEMPLATE.md`

Standard template for pull requests with:
- Description
- Type of change
- Testing checklist
- Code review checklist

### Issue Templates

**Files:**
- `.github/ISSUE_TEMPLATE/bug_report.md` - Bug report template
- `.github/ISSUE_TEMPLATE/feature_request.md` - Feature request template

---

**Last Updated:** 2026-01-02
