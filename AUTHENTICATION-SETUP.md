# Authentication Setup Guide

The WinInventory application now requires users to sign in with Google or Microsoft before accessing Mac recommendations.

## Setup OAuth Credentials

### Google OAuth Setup

1. **Go to Google Cloud Console**: https://console.cloud.google.com/
2. **Create a new project** (or select existing)
3. **Enable Google+ API**:
   - Go to "APIs & Services" > "Library"
   - Search for "Google+ API" and enable it
4. **Create OAuth 2.0 Credentials**:
   - Go to "APIs & Services" > "Credentials"
   - Click "Create Credentials" > "OAuth client ID"
   - Application type: "Web application"
   - Authorized redirect URIs: 
     - `https://your-domain.com/signin-google` (for production)
     - `http://localhost:5000/signin-google` (for local development)
   - Click "Create"
   - Copy the **Client ID** and **Client Secret**

### Microsoft OAuth Setup

1. **Go to Azure Portal**: https://portal.azure.com/
2. **Register an application**:
   - Go to "Azure Active Directory" > "App registrations"
   - Click "New registration"
   - Name: "WinInventory"
   - Supported account types: "Accounts in any organizational directory and personal Microsoft accounts"
   - Redirect URI: 
     - Platform: "Web"
     - URI: `https://your-domain.com/signin-microsoft` (for production)
     - URI: `http://localhost:5000/signin-microsoft` (for local development)
   - Click "Register"
3. **Get credentials**:
   - Copy the **Application (client) ID**
   - Go to "Certificates & secrets"
   - Create a new client secret
   - Copy the **Value** (this is your Client Secret)

## Configure the Application

### Option 1: Using appsettings.json (for local development)

Add to `appsettings.json`:

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET"
    },
    "Microsoft": {
      "ClientId": "YOUR_MICROSOFT_CLIENT_ID",
      "ClientSecret": "YOUR_MICROSOFT_CLIENT_SECRET"
    }
  }
}
```

### Option 2: Using Environment Variables (recommended for production)

Set these environment variables:

- `GOOGLE_CLIENT_ID`
- `GOOGLE_CLIENT_SECRET`
- `MICROSOFT_CLIENT_ID`
- `MICROSOFT_CLIENT_SECRET`

### Option 3: Azure App Service Configuration

1. Go to your App Service in Azure Portal
2. Navigate to "Configuration" > "Application settings"
3. Add new application settings:
   - `GOOGLE_CLIENT_ID`
   - `GOOGLE_CLIENT_SECRET`
   - `MICROSOFT_CLIENT_ID`
   - `MICROSOFT_CLIENT_SECRET`
4. Click "Save"

## Testing

1. Run the application: `dotnet run`
2. Navigate to the home page
3. Click "Sign In" button
4. Choose either Google or Microsoft
5. Complete the OAuth flow
6. You should be redirected back and see "Signed in" status
7. Now you can access Mac recommendations

## Security Notes

- Never commit credentials to source control
- Use environment variables or Azure Key Vault for production
- Keep your client secrets secure
- Regularly rotate your OAuth secrets

