# GitHub Actions CI/CD Setup

This directory contains GitHub Actions workflows for the Arcanic.Result project.

## Workflows

### 1. Build and Test Workflow (`build-and-test.yml`)

**Triggers:**
- Pull requests to `main` or `develop` branches

**What it does:**
- Builds the solution across multiple .NET versions (8, 9, 10)
- Runs all tests with code coverage collection
- Validates package creation
- Uses the `Arcanic.Result.slnx` solution file

### 2. Release Workflow (`release.yml`)

**Triggers:**
- Push of version tags (e.g., `v10.1.0`, `v10.1.0-beta.1`)
- Manual workflow dispatch with version input

**What it does:**
- Validates the version is SemVer (prereleases such as `10.1.0-beta.1` are supported) and that the commit is on `main`
- Builds the solution and runs the tests against .NET 8, 9 and 10
- Packs the `src/` projects (only those are packable), which also runs package validation against the previous release
- Uploads packages as GitHub artifacts with 90-day retention
- Publishes to GitHub Packages, then creates the GitHub release, then publishes to NuGet.org last (a NuGet version can never be replaced)
- Uses `.github/release-notes/v<version>.md` as the release body when it exists, otherwise generates notes from merged PRs; versions with a prerelease suffix are marked as prereleases

## Setup Instructions

### 1. Configure the `release` environment

The release job runs in the GitHub environment named `release`, so publishing can be gated and only this environment can obtain a NuGet key.

1. In your GitHub repository, go to Settings → Environments → **New environment** and name it `release`. (GitHub creates it automatically on the first run if it does not exist, but without any protection.)
2. Recommended protection rules:
   - **Required reviewers:** someone must approve each release before it runs
   - **Deployment branches and tags:** choose "Selected branches and tags" and allow the tag pattern `v*` (tag releases) and the branch `main` (manual releases)
3. Under the environment's **Environment secrets**, add `NUGET_USER` containing your nuget.org **profile name** (not your email address). Keeping it here instead of in repository secrets makes it available only to jobs in this environment.

### 2. Configure NuGet Trusted Publishing

The workflow publishes to NuGet.org with [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing): the job exchanges a GitHub OIDC token for a temporary API key (valid for 1 hour) via `NuGet/login`. No long-lived API key is stored in the repository.

1. Sign in to [NuGet.org](https://www.nuget.org/), click your username and choose **Trusted Publishing**
2. Add a policy owned by the account or organization that owns `Arcanic.Result`:
   - **Repository Owner:** `arcanic-kit`
   - **Repository:** `Result`
   - **Workflow File:** `release.yml` (file name only, without `.github/workflows/`)
   - **Environment:** `release`. Only jobs running in that environment can then publish.
3. Once a release has published successfully, delete the old `NUGET_API_KEY` secret and revoke that key on NuGet.org

For a private repository, a new policy is only active for 7 days until the first successful publish locks it to the repository; it can be restarted from the policy page.

Note: The workflow also publishes to GitHub Packages using the `GITHUB_TOKEN` (automatically available).

### 3. Release Process

#### Option A: Tag-based Release (Recommended)
```bash
# Create and push a version tag
git tag v10.1.0
git push origin v10.1.0
```

#### Option B: Manual Release
1. Go to your repository on GitHub
2. Navigate to Actions → Release
3. Click "Run workflow"
4. Enter the version number (e.g., `10.1.0`)
5. Click "Run workflow"

### 4. Version Management

The workflow automatically handles version information:
- For tag-based releases: Extracts version from the tag name (removes 'v' prefix)
- For manual releases: Uses the version input provided
- Updates `AssemblyVersion`, `FileVersion`, and package `Version` properties
- Creates both regular packages (.nupkg) and symbol packages (.snupkg) for debugging

### 5. Package Output

The workflow creates packages for the source projects in the solution:
- `Arcanic.Result`

Note: Sample projects, test projects, and benchmarks are excluded from packaging.

### 6. Troubleshooting

- **Build failures:** Check the CI workflow first to ensure your changes build correctly
- **Package conflicts:** The workflow uses `--skip-duplicate` to avoid conflicts with existing packages
- **Version conflicts:** Ensure you're using a new version number that hasn't been published before
- **NuGet login failures:** Check that the trusted publishing policy is active and matches the repository owner, repository, workflow file and environment (`release`) exactly, and that `NUGET_USER` (an environment secret of `release`) is the nuget.org profile name
- **Run waiting or rejected:** The `release` environment may require approval, or its deployment rules may not allow the tag or branch being released

## Best Practices

1. **Always test locally first:** Run `dotnet build`, `dotnet test`, and `dotnet pack` locally before pushing
2. **Use semantic versioning:** Follow [SemVer](https://semver.org/) for version numbers
3. **Update changelogs:** Consider maintaining a CHANGELOG.md file for release notes
4. **Review packages:** Check the generated packages in the GitHub artifacts before they're published
5. **CI/CD separation:** The build-and-test workflow runs on PRs, while release workflow only runs on tags/manual dispatch

## Security Notes

- Never commit API keys or secrets to the repository
- Use GitHub repository secrets for sensitive information
- NuGet.org publishing uses Trusted Publishing (OIDC), so there is no NuGet API key to leak or rotate
- Review workflow permissions and scope as needed (`id-token: write` is required for Trusted Publishing)
- GitHub Packages publishing uses the automatically provided `GITHUB_TOKEN`