# CI/CD Setup Guide

This guide explains how to set up and configure the CI/CD pipeline for FluentTestScaffold.

## Overview

The CI/CD pipeline consists of several GitHub Actions workflows that handle different aspects of the development and release process:

- **CI**: Continuous Integration - builds, tests, and validates code with coverage
- **Release Publishing**: Continuous Deployment - publishes packages and creates releases
- **CodeQL**: Automated security scanning

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

The release publishing workflow automatically publishes to GitHub Packages using the `GITHUB_TOKEN`. No additional configuration is needed.

#### NuGet.org (Optional)

To publish to NuGet.org:

1. Add the `NUGET_API_KEY` secret as described above
2. The `release-publish.yml` workflow will automatically publish to NuGet.org when releases are published

### 4. Version Management

The project uses **release tag-based versioning** for version management:

- **Git release tags**: Primary source of version information (e.g., `v1.0.0`)
- **GitHub Actions**: Extracts version from release tags during release workflow
- **Assembly versioning**: Set dynamically during build from release tag
- **NuGet packages**: Versioned using extracted tag version

### 5. Release Process

The CI/CD pipeline includes automated release publishing:

#### Release Publishing Process

1. **Automatic Release Publishing**: When a release is published, packages are automatically built and published
2. **Dual Publishing**: Packages are published to both GitHub Packages and NuGet.org
3. **Release Notes Update**: Release notes are automatically updated with publishing information

#### Version Synchronization

The pipeline ensures version consistency across:
- Git release tags (e.g., `v1.0.0`)
- Assembly versions (extracted from release tag)
- Package versions (NuGet packages)
- Release versions (GitHub releases)

#### Creating a Release

```bash
# Ensure you're on main branch
git checkout main
git pull origin main

# Create and push a tag
git tag v1.0.0
git push origin v1.0.0

# Create GitHub release (triggers release-publish workflow)
gh release create v1.0.0 --title "Release v1.0.0" --notes "Release notes here"
```

This will trigger:
1. Release publishing workflow validation (ensures release is from main branch)
2. Build and test with version extracted from tag
3. Automatic publishing to GitHub Packages and NuGet.org
4. Release notes update with publishing information

## Workflow Details

### CI Workflow (`ci.yml`)

**Triggers**: Push to main, pull requests

**Jobs**:
1. **Build and Test**: Builds and tests on multiple .NET versions (6.0, 7.0, 8.0)
2. **Code Coverage**: Generates coverage reports with 90% threshold enforcement
3. **Security Scan**: Runs security analysis
4. **Code Quality**: Runs code formatting and style analysis

### Release Publishing Workflow (`release-publish.yml`)

**Triggers**: Published releases, manual workflow dispatch

**Jobs**:
1. **Extract Version**: Extracts version from release tag
2. **Validate Release**: Validates release is from main branch
3. **Build Packages**: Builds all packages with extracted version
4. **Publish to NuGet.org**: Publishes packages to NuGet.org
5. **Publish to GitHub Packages**: Publishes packages to GitHub Packages
6. **Update Release Notes**: Updates release with publishing information

### CodeQL Security Scanning

**Integration**: Built into CI workflow

**Features**:
1. **Static Analysis**: Runs security vulnerability scanning
2. **Automated Execution**: No manual intervention required
3. **GitHub Security Integration**: Results appear in Security tab

## Configuration

### Customizing Workflows

The workflows can be customized by modifying the YAML files in `.github/workflows/`:

- **`Directory.Build.props`**: Build properties and version management
- **`coverlet.runsettings`**: Code coverage configuration

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

3. **Coverage Threshold Failures**:
   - Add more tests to increase coverage
   - Review uncovered code paths
   - Consider excluding non-critical code

4. **Package Publishing Issues**:
   - Verify API keys are correctly configured
   - Check package version conflicts
   - Ensure package metadata is complete

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
3. **Check Coverage**: Run coverage locally to ensure 90% threshold
4. **Check Dependencies**: Regularly update packages
5. **Documentation**: Keep documentation up to date

### Release Process

1. **Version Bumping**: Use semantic versioning for release tags
2. **Changelog**: Document changes for releases
3. **Testing**: Ensure all tests pass before tagging
4. **Communication**: Notify team of releases

### Security

1. **Regular Scans**: Monitor CodeQL scan results
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
- Test coverage percentage (target: 90%)
- Security scan results
- Release frequency

## Support

For issues with the CI/CD setup:

1. **Check Documentation**: Review this guide and GitHub Actions docs
2. **Review Logs**: Examine workflow execution logs
3. **Community**: Ask questions in GitHub Discussions
4. **Issues**: Create GitHub issues for bugs or feature requests    