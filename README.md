# JiraSubtaskGenerator

A C# .NET Core console application that reads a Markdown file and creates Jira subtasks under a specified parent issue (Story/Task).

## 🛠 Features

- Parses markdown-formatted task lists
- Generates Jira subtasks
- Supports `--dry-run` mode
- Logs progress and errors

## 📦 Prerequisites

- .NET SDK 10.0+
- A Jira account and [API token](https://id.atlassian.com/manage/api-tokens)

## 🚀 Usage

### Build

```bash
dotnet build
```

### Run

#### Dry Run
```bash
dotnet run --file input.md --dry-run
```

#### Real Jira Creation

```bash
dotnet run --file input.md
```

> ⚠️ Make sure to configure your Jira URL, email, and API token as environment variables:

##### Environment Variables
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
