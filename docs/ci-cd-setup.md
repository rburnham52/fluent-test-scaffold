# CI/CD Setup Guide

This guide explains how to set up and configure the CI/CD pipeline for FluentTestScaffold.

## Overview

The CI/CD pipeline consists of several GitHub Actions workflows that handle different aspects of the development and release process:

- **CI**: Continuous Integration - builds, tests, and validates code
- **CD**: Continuous Deployment - publishes packages and creates releases
- **PR Check**: Pull Request validation - ensures code quality
- **NuGet Publish**: Publishes packages to NuGet.org
- **Scheduled Maintenance**: Weekly maintenance tasks

## Prerequisites

1. **GitHub Repository**: The workflows are designed for GitHub repositories
2. **GitHub Actions**: Enabled in your repository settings
3. **NuGet API Key**: For publishing to NuGet.org (optional)

## Setup Steps

### 1. Enable GitHub Actions

1. Go to your GitHub repository
2. Navigate to Settings → Actions → General
3. Enable "Allow all actions and reusable workflows"
4. Save the changes

### 2. Configure Secrets

Navigate to Settings → Secrets and variables → Actions and add the following secrets:

#### Required Secrets

- **`GITHUB_TOKEN`**: Automatically provided by GitHub Actions (no action needed)

#### Optional Secrets

- **`NUGET_API_KEY`**: Your NuGet.org API key for publishing packages
  - Get this from [NuGet.org](https://www.nuget.org/account/apikeys)
  - Create a new API key with "Push" permissions
  - Copy the key and add it as a secret

### 3. Configure Package Publishing

#### GitHub Packages (Automatic)

The CD workflow automatically publishes to GitHub Packages using the `GITHUB_TOKEN`. No additional configuration is needed.

#### NuGet.org (Optional)

To publish to NuGet.org:

1. Add the `NUGET_API_KEY` secret as described above
2. The `nuget-publish.yml` workflow will automatically run when tags are pushed

### 4. Version Management

The project uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) for version management:

- **`version.json`**: Main version configuration
- **`Directory.Build.props`**: Common build properties with version synchronization
- **Git tags**: Used for releases

### 5. Manual Intervention Process

The CI/CD pipeline includes manual intervention steps to ensure quality:

#### Release Approval Process

1. **Automatic Release Creation**: When a tag is pushed, a draft release is created
2. **Manual Review**: Review the draft release and packages
3. **Approval Workflow**: Use "Approve Release for NuGet Publishing" workflow
4. **NuGet Publishing**: Manually trigger "Publish to NuGet.org" workflow

#### Version Synchronization

The pipeline ensures version consistency across:
- Git tags (e.g., `v1.0.0`)
- Assembly versions (calculated by GitVersion)
- Package versions (NuGet packages)
- Release versions (GitHub releases)

#### Creating a Release

```bash
# Create and push a tag
git tag v1.0.0
git push origin v1.0.0
```

This will trigger:
1. CI workflow (build and test)
2. CD workflow (publish to GitHub Packages and create draft release)
3. Manual approval required for NuGet.org publishing

## Workflow Details

### CI Workflow (`ci.yml`)

**Triggers**: Push to main/develop, pull requests, tags

**Jobs**:
1. **Build and Test**: Builds and tests on multiple .NET versions
2. **Validate Packages**: Creates and validates NuGet packages
3. **Code Coverage**: Generates coverage reports
4. **Security Scan**: Runs security analysis

### CD Workflow (`cd.yml`)

**Triggers**: Push tags (v*)

**Jobs**:
1. **Publish**: Publishes packages to GitHub Packages
2. **Create Release**: Creates GitHub release with artifacts

### PR Check Workflow (`pr-check.yml`)

**Triggers**: Pull requests to main/develop

**Jobs**:
1. **Format Check**: Validates code formatting
2. **Lint Check**: Runs code analyzers
3. **Dependency Check**: Checks for outdated/vulnerable packages
4. **Test Coverage**: Ensures minimum coverage (80%)

### NuGet Publish Workflow (`nuget-publish.yml`)

**Triggers**: Manual workflow dispatch

**Jobs**:
1. **Validate Release**: Validates version consistency and builds packages
2. **Publish to NuGet.org**: Publishes packages to NuGet.org (requires approval)

### Approve Release Workflow (`approve-release.yml`)

**Triggers**: Manual workflow dispatch

**Jobs**:
1. **Validate Release Approval**: Validates release and creates approval record
2. **Reject Release**: Creates rejection record if not approved

### Scheduled Maintenance Workflow (`scheduled-maintenance.yml`)

**Triggers**: Weekly (Sundays at 2 AM UTC), manual dispatch

**Jobs**:
1. **Dependency Updates**: Checks for outdated packages
2. **Security Scan**: Runs comprehensive security analysis
3. **Performance Test**: Runs performance tests
4. **Documentation Check**: Validates documentation completeness

## Configuration

### Customizing Workflows

The workflows can be customized by modifying the YAML files in `.github/workflows/`:

- **`.github/ci-config.json`**: Common configuration settings
- **`version.json`**: Version management settings
- **`Directory.Build.props`**: Build properties

### Environment Variables

Common environment variables used across workflows:

```yaml
env:
  DOTNET_VERSION: '8.0.x'
  DOTNET_VERSIONS: '["6.0.x", "7.0.x", "8.0.x"]'
```

### Matrix Strategy

The CI workflow uses matrix strategy to test against multiple .NET versions:

```yaml
strategy:
  matrix:
    dotnet-version: [6.0.x, 7.0.x, 8.0.x]
```

## Troubleshooting

### Common Issues

1. **Build Failures**:
   - Check that all dependencies are properly restored
   - Verify .NET version compatibility
   - Check for syntax errors in source code

2. **Test Failures**:
   - Review test output for specific failures
   - Check test data and mock configurations
   - Verify test environment setup

3. **Package Publishing Issues**:
   - Verify API keys are correctly configured
   - Check package version conflicts
   - Ensure package metadata is complete

4. **Coverage Threshold Failures**:
   - Add more tests to increase coverage
   - Review uncovered code paths
   - Consider excluding non-critical code

### Debugging Workflows

1. **Enable Debug Logging**:
   ```yaml
   env:
     ACTIONS_STEP_DEBUG: true
   ```

2. **Check Workflow Logs**:
   - Go to Actions tab in GitHub
   - Click on the failed workflow
   - Review step-by-step logs

3. **Local Testing**:
   ```bash
   # Test build locally
   dotnet build --configuration Release
   
   # Test packaging
   dotnet pack --configuration Release
   
   # Test with specific .NET version
   dotnet --version
   ```

## Best Practices

### Code Quality

1. **Format Code**: Use `dotnet format` before committing
2. **Run Tests Locally**: Ensure all tests pass before pushing
3. **Check Dependencies**: Regularly update packages
4. **Documentation**: Keep documentation up to date

### Release Process

1. **Version Bumping**: Update version in `version.json` if needed
2. **Changelog**: Document changes for releases
3. **Testing**: Ensure all tests pass before tagging
4. **Communication**: Notify team of releases

### Security

1. **Regular Scans**: Monitor security scan results
2. **Dependency Updates**: Keep dependencies updated
3. **Secret Management**: Rotate API keys regularly
4. **Access Control**: Limit access to sensitive workflows

## Monitoring

### Workflow Status

Monitor workflow status through:
- GitHub Actions tab
- Status badges in README
- Email notifications (if configured)

### Metrics

Track important metrics:
- Build success rate
- Test coverage percentage
- Security scan results
- Release frequency

## Support

For issues with the CI/CD setup:

1. **Check Documentation**: Review this guide and GitHub Actions docs
2. **Review Logs**: Examine workflow execution logs
3. **Community**: Ask questions in GitHub Discussions
4. **Issues**: Create GitHub issues for bugs or feature requests 