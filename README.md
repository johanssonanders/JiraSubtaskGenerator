# JiraSubtaskGenerator

A C# .NET Core console application that reads a Markdown file and creates Jira subtasks under a specified parent issue (Story/Task).

## ✨Features

- Parses markdown-formatted task lists
- Generates Jira subtasks
- Supports `--dry-run` mode
- Logs progress and errors

## 📦 Prerequisites

- .NET SDK 10.0+
- A Jira account and [API token](https://id.atlassian.com/manage/api-tokens)


## 📘 Usage

### Installing the tool locally

If you have received the `.nupkg` file for **JiraSubtaskGenerator**, you can install it as a local .NET tool without needing any external package feed.

#### 1. Place the `.nupkg` file somewhere on your machine

For example:

- `C:\local-tools` on Windows  
- `~/local-tools` on Linux or macOS  

Make sure the `.nupkg` file is inside that folder.

#### 2. Install the tool using the local source

##### Windows

```bash
dotnet tool install --global JiraSubtaskGenerator \
  --version 1.0.0 \
  --add-source "C:\local-tools"
```
##### Linux / macOS

```bash
dotnet tool install --global JiraSubtaskGenerator \
  --version 1.0.0 \
  --add-source "$HOME/local-tools"
```

##### Linux / macOS

```bash
dotnet tool install --global JiraSubtaskGenerator \
  --version 1.0.0 \
  --add-source "$HOME/local-tools"
```

#### 3. Updating the tool
If you replace the .nupkg file with a newer version in the same folder, update using:
```bash
dotnet tool update --global JiraSubtaskGenerator \
  --add-source "C:\local-tools"
```
### 🚀 Run
If installed as tool from a nuget package to executable can be called with `subtasks` subtasks command, the examples below is using that syntax.

If the executable is used directly, call it as per usual with `JiraSubtaskGenerator.exe`.

#### Help
```bash
subtasks --help
```

#### Dry Run
```bash
subtasks --file input.md --dry-run
```

#### Real Jira Creation

```bash
subtasks --file input.md
```

> ⚠️ Make sure to configure your Jira URL, email, and API token as environment variables:

## 🛠️ Environment Variables
On Windows, you can set environment variables in PowerShell like this:
```powershell
$env:JIRA_URL="https://yourdomain.atlassian.net"
$env:JIRA_EMAIL="you@example.com"
$env:JIRA_TOKEN="your_token"
```

On Linux or macOS, you can set them in the terminal:
```bash
export JIRA_URL="https://yourdomain.atlassian.net"
export JIRA_EMAIL="you@example.com"
export JIRA_TOKEN="your_token"
```  

## 📄 Markdown Format

```markdown
@PROJECTKEY:PARENT-ISSUE-KEY
## Subtask Title || EstimatedHours
Description
```

### 📄 Markdown Example

```markdown
@PROJ123:STORY-456

## Subtask Title One || 2
This is the description
for the first subtask.

It can span multiple lines.

## Subtask Title Two || 4
Second subtask's description goes here.
```

## 🔒 Security

Do **not** hardcode credentials in code. Use environment variables or secure vaults.

## ✅ License

MIT
