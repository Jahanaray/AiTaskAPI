name: Bug Report
about: Report a bug or unexpected behavior
title: "bug: "
labels: "bug"
body:
  - type: markdown
    attributes:
      value: |
        Thanks for taking the time to fill out this bug report!
  - type: input
    id: version
    attributes:
      label: Version
      description: What version are you using?
      placeholder: e.g., 0.1.0
    validations:
      required: true
  - type: textarea
    id: description
    attributes:
      label: Describe the bug
      description: A clear and concise description of what the bug is.
    validations:
      required: true
  - type: textarea
    id: steps
    attributes:
      label: Steps to Reproduce
      description: How do you trigger this bug?
      placeholder: |
        1. Step 1
        2. Step 2
        3. Step 3
    validations:
      required: true
  - type: textarea
    id: expected
    attributes:
      label: Expected behavior
      description: What did you expect to happen?
    validations:
      required: true
  - type: textarea
    id: actual
    attributes:
      label: Actual behavior
      description: What actually happened?
    validations:
      required: true
  - type: textarea
    id: environment
    attributes:
      label: Environment
      description: Provide details about your environment.
      placeholder: |
        - OS: Windows 11
        - .NET Version: 10.0
        - Database: PostgreSQL 16
        - Browser (if frontend issue): Chrome 120
    validations:
      required: true
  - type: textarea
    id: logs
    attributes:
      label: Relevant log output
      description: Please copy and paste any relevant log output.
      render: shell
  - type: checkboxes
    id: terms
    attributes:
      label: Code of Conduct
      description: By submitting this issue, you agree to follow our Code of Conduct.
      options:
        - label: I agree to follow this project's Code of Conduct
          required: true
