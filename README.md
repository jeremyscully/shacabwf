# ShacabWf

A React web application with ASP.NET Core backend and Active Directory authentication.

## Prerequisites

- [.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)
- [Node.js](https://nodejs.org/) (for React development)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) or [Visual Studio Code](https://code.visualstudio.com/)
- An Azure Active Directory tenant for authentication

## Setup

### Azure Active Directory Configuration

1. Register a new application in Azure Active Directory:
   - Go to the [Azure Portal](https://portal.azure.com)
   - Navigate to "Azure Active Directory" > "App registrations" > "New registration"
   - Enter a name for your application (e.g., "ShacabWf")
   - Set the redirect URI to `https://localhost:44462/signin-oidc` (adjust port if needed)
   - Click "Register"

2. Note the following information from your registered app:
   - Application (client) ID
   - Directory (tenant) ID
   - Domain name (e.g., yourdomain.onmicrosoft.com)

3. Update the `appsettings.json` file with your Azure AD information:
   ```json
   "AzureAd": {
     "Instance": "https://login.microsoftonline.com/",
     "Domain": "your-domain.onmicrosoft.com",
     "TenantId": "your-tenant-id",
     "ClientId": "your-client-id",
     "CallbackPath": "/signin-oidc",
     "SignedOutCallbackPath": "/signout-callback-oidc"
   }
   ```

## Running the Application

### Using Visual Studio

1. Open the `ShacabWf.sln` solution file in Visual Studio
2. Set the `ShacabWf.Web` project as the startup project
3. Press F5 to build and run the application

### Using Command Line

1. Navigate to the project directory
2. Run the following commands:
   ```
   cd ShacabWf.Web
   dotnet run
   ```

3. Open your browser and navigate to `https://localhost:44462` (or the port specified in your launchSettings.json)

## Features

- Active Directory authentication
- Simple welcome page after login
- React-based frontend
- ASP.NET Core backend

## Project Structure

- `ShacabWf.Web` - Main web application with React frontend and ASP.NET Core backend
  - `ClientApp/` - React application
  - `Controllers/` - ASP.NET Core controllers
  - `Views/` - MVC views for server-rendered pages

## License

This project is licensed under the MIT License. 