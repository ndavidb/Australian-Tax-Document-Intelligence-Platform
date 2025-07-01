# Australian Tax Document Intelligence Platform

![Build Status](https://github.com/yourusername/TaxDocumentProcessor/workflows/Deploy%20Tax%20Document%20Processor/badge.svg)
![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)
![Azure Functions](https://img.shields.io/badge/Azure-Functions-blue)
![License](https://img.shields.io/badge/license-MIT-green)

A serverless document processing system that helps Australian small business owners automate tax expense tracking using Azure Functions, Blob Storage, and AI services.

## 🚀 Features

- **Automatic Receipt Processing**: Upload receipts to automatically extract expense information
- **AI-Powered Data Extraction**: Uses Azure Form Recognizer to extract vendor, amount, date, and items
- **Australian Tax Categorization**: Automatically categorizes expenses according to ATO guidelines
- **Quarterly Summaries**: Generates tax-ready expense summaries
- **RESTful API**: Track document processing status and retrieve results
- **Infrastructure as Code**: Fully automated deployment using Bicep

## 📋 Architecture

[Architecture diagram will be added in Stage 5]

## 🛠️ Tech Stack

- **Runtime**: .NET 8 / C#
- **Cloud**: Azure Functions (Consumption Plan)
- **Storage**: Azure Blob Storage
- **AI**: Azure Form Recognizer
- **IaC**: Bicep
- **CI/CD**: GitHub Actions

## 🚦 Getting Started

### Prerequisites
- .NET 8 SDK
- Azure CLI
- Azure Functions Core Tools v4
- Azure subscription

### Local Development Setup

1. Clone the repository:
   \`\`\`bash
   git clone https://github.com/yourusername/TaxDocumentProcessor.git
   cd TaxDocumentProcessor
   \`\`\`

2. Install dependencies:
   \`\`\`bash
   dotnet restore
   \`\`\`

3. Configure local settings:
   \`\`\`bash
   cp src/TaxDocumentProcessor.Functions/local.settings.sample.json src/TaxDocumentProcessor.Functions/local.settings.json
   # Edit local.settings.json with your Azure Storage connection string
   \`\`\`

4. Run locally:
   \`\`\`bash
   cd src/TaxDocumentProcessor.Functions
   func start
   \`\`\`

## 📚 Documentation

- [Architecture Overview](docs/architecture.md)
- [API Reference](docs/api-reference.md)
- [Deployment Guide](docs/deployment.md)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.