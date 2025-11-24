# Step-by-Step OAuth Setup Guide

## Part 1: Google OAuth Setup

### Step 1: Enable Google+ API
1. In Google Cloud Console, you're already on "APIs & Services" page
2. Click **"Library"** in the left menu
3. Search for **"Google+ API"** or **"Google Identity"**
4. Click on **"Google+ API"** or **"Google Identity Services API"**
5. Click the **"Enable"** button
6. Wait for it to enable (you'll see a green checkmark)

### Step 2: Create OAuth Credentials
1. Go back to **"APIs & Services"** > **"Credentials"** (in left menu)
2. Click the **"+ CREATE CREDENTIALS"** button at the top
3. Select **"OAuth client ID"**
4. If prompted, configure the OAuth consent screen first:
   - **User Type**: Choose "External" (unless you have a Google Workspace)
   - Click **"Create"**
   - **App name**: Enter "WinInventory"
   - **User support email**: Select your email
   - **Developer contact**: Enter your email
   - Click **"Save and Continue"**
   - **Scopes**: Click "Save and Continue" (default scopes are fine)
   - **Test users**: Add your email, click "Save and Continue"
   - Click **"Back to Dashboard"**

5. Now create OAuth Client ID:
   - **Application type**: Select **"Web application"**
   - **Name**: Enter "WinInventory Web Client"
   - **Authorized JavaScript origins**: 
     - Click **"+ ADD URI"**
     - Add: `http://localhost:5000`
     - Add: `http://localhost:7000`
     - Add: `https://localhost:5001` (if using HTTPS)
   - **Authorized redirect URIs**:
     - Click **"+ ADD URI"**
     - Add: `http://localhost:5000/signin-google`
     - Add: `http://localhost:7000/signin-google`
     - Add: `https://localhost:5001/signin-google` (if using HTTPS)
   - Click **"Create"**

6. **IMPORTANT**: Copy the **Client ID** and **Client Secret** immediately!
   - You'll see a popup with your credentials
   - Copy both values and save them securely

### Step 3: Add Credentials to Your App
1. Open `appsettings.json` in your project
2. Add the credentials:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID_HERE",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET_HERE"
    }
  }
}
```

3. Replace `YOUR_GOOGLE_CLIENT_ID_HERE` with your actual Client ID
4. Replace `YOUR_GOOGLE_CLIENT_SECRET_HERE` with your actual Client Secret
5. Save the file

---

## Part 2: Microsoft OAuth Setup

### Step 1: Go to Azure Portal
1. Open https://portal.azure.com
2. Sign in with your Microsoft account

### Step 2: Register Application
1. Search for **"Azure Active Directory"** in the top search bar
2. Click on **"Azure Active Directory"**
3. In the left menu, click **"App registrations"**
4. Click **"+ New registration"** button at the top

### Step 3: Configure Application
1. **Name**: Enter "WinInventory"
2. **Supported account types**: 
   - Select **"Accounts in any organizational directory and personal Microsoft accounts"**
3. **Redirect URI**:
   - Platform: Select **"Web"**
   - URI: Enter `http://localhost:5000/signin-microsoft`
   - Click **"Add"** (you can add more later)
   - Also add: `http://localhost:7000/signin-microsoft`
4. Click **"Register"** button

### Step 4: Get Credentials
1. After registration, you'll see the **"Overview"** page
2. Copy the **Application (client) ID** - this is your Client ID
3. In the left menu, click **"Certificates & secrets"**
4. Under **"Client secrets"**, click **"+ New client secret"**
5. **Description**: Enter "WinInventory Secret"
6. **Expires**: Choose "24 months" (or your preference)
7. Click **"Add"**
8. **IMPORTANT**: Copy the **Value** immediately (you won't see it again!)
   - This is your Client Secret

### Step 5: Add Credentials to Your App
1. Open `appsettings.json` in your project
2. Add Microsoft credentials to the same file:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Authentication": {
    "Google": {
      "ClientId": "YOUR_GOOGLE_CLIENT_ID_HERE",
      "ClientSecret": "YOUR_GOOGLE_CLIENT_SECRET_HERE"
    },
    "Microsoft": {
      "ClientId": "YOUR_MICROSOFT_CLIENT_ID_HERE",
      "ClientSecret": "YOUR_MICROSOFT_CLIENT_SECRET_HERE"
    }
  }
}
```

3. Replace the Microsoft placeholders with your actual values
4. Save the file

---

## Part 3: Test Authentication

1. **Restart your application**:
   ```powershell
   $env:Path += ";C:\Program Files\dotnet"
   dotnet run
   ```

2. **Open your browser** and go to `http://localhost:5000` (or the port shown)

3. **You should now see**:
   - "Sign In" button in the top right
   - Click it to see Google and Microsoft sign-in options

4. **Test Google Sign-In**:
   - Click "Continue with Google"
   - Sign in with your Google account
   - You should be redirected back and see "Signed in" + "Sign Out"

5. **Test Microsoft Sign-In**:
   - Sign out first
   - Click "Continue with Microsoft"
   - Sign in with your Microsoft account
   - You should be redirected back and see "Signed in" + "Sign Out"

---

## Troubleshooting

### Google OAuth Errors:
- **"invalid_client"**: Check that Client ID and Secret are correct in `appsettings.json`
- **"redirect_uri_mismatch"**: Make sure the redirect URI in Google Console matches exactly (including http/https and port)

### Microsoft OAuth Errors:
- **"AADSTS700016"**: Check that Client ID is correct
- **"redirect_uri_mismatch"**: Make sure the redirect URI in Azure matches exactly

### General:
- Make sure you restarted the application after adding credentials
- Check that `appsettings.json` has valid JSON syntax
- Verify credentials are in the correct format (no extra spaces or quotes)

---

## Security Notes

⚠️ **IMPORTANT**: 
- Never commit `appsettings.json` with real credentials to Git
- Add `appsettings.json` to `.gitignore` if it contains secrets
- For production, use environment variables or Azure Key Vault instead

---

## Next Steps

Once authentication is working:
1. Users must sign in before accessing Mac recommendations
2. The "Sign In" button will appear when not authenticated
3. After signing in, users can access all features
4. Users can sign out using the "Sign Out" button

