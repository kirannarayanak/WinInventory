# Microsoft/Azure OAuth Setup - Quick Guide

## Step-by-Step Instructions

### Step 1: Go to Azure Portal
1. Open your browser and go to: **https://portal.azure.com**
2. Sign in with your Microsoft account

### Step 2: Find Azure Active Directory
1. In the top search bar (where it says "Search resources, services, and docs"), type: **"Azure Active Directory"**
2. Click on **"Azure Active Directory"** from the results
3. You should see the Azure AD overview page

### Step 3: Register Your Application
1. In the left menu, look for **"App registrations"** (under "Manage" section)
2. Click on **"App registrations"**
3. Click the **"+ New registration"** button at the top of the page

### Step 4: Configure Application Details
1. **Name**: Type `WinInventory`
2. **Supported account types**: 
   - Select the option: **"Accounts in any organizational directory and personal Microsoft accounts"**
   - (This allows both work/school accounts and personal Microsoft accounts)
3. **Redirect URI** section:
   - **Platform**: Click the dropdown and select **"Web"**
   - **URI**: Type `http://localhost:5000/signin-microsoft`
   - Click **"Add"** button
   - Add another one: `http://localhost:7000/signin-microsoft`
   - Click **"Add"** again
4. Click the **"Register"** button at the bottom

### Step 5: Copy Your Client ID
1. After registration, you'll be on the **"Overview"** page
2. Look for **"Application (client) ID"** - this is a long GUID (like: `12345678-1234-1234-1234-123456789abc`)
3. **Copy this value** - this is your Microsoft Client ID
4. Save it somewhere safe (you'll need it in a moment)

### Step 6: Create Client Secret
1. In the left menu, click **"Certificates & secrets"** (under "Manage" section)
2. Under the **"Client secrets"** tab, click **"+ New client secret"**
3. **Description**: Type `WinInventory Secret`
4. **Expires**: Select **"24 months"** (or your preference)
5. Click **"Add"** button
6. **IMPORTANT**: A new secret will appear in the table
   - Find the row with your secret
   - Look at the **"Value"** column - this is your Client Secret
   - **Copy this value IMMEDIATELY** - you won't be able to see it again!
   - It looks like: `abc123~XYZ456...` (a long string)

### Step 7: Add Credentials to Your App
1. Open `appsettings.json` in your project
2. Add the Microsoft credentials to the existing Authentication section
3. The file should look like this:

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

4. Replace `YOUR_MICROSOFT_CLIENT_ID_HERE` with the Application (client) ID you copied
5. Replace `YOUR_MICROSOFT_CLIENT_SECRET_HERE` with the secret Value you copied
6. Save the file

### Step 8: Test It
1. Restart your application
2. Go to your app in the browser
3. Click "Sign In"
4. You should now see both "Continue with Google" and "Continue with Microsoft" buttons
5. Click "Continue with Microsoft" to test

---

## Quick Checklist

- [ ] Opened Azure Portal (portal.azure.com)
- [ ] Found Azure Active Directory
- [ ] Clicked "App registrations" > "New registration"
- [ ] Named it "WinInventory"
- [ ] Selected "Accounts in any organizational directory and personal Microsoft accounts"
- [ ] Added redirect URIs: `http://localhost:5000/signin-microsoft` and `http://localhost:7000/signin-microsoft`
- [ ] Clicked "Register"
- [ ] Copied the Application (client) ID
- [ ] Created a client secret and copied the Value
- [ ] Added both to appsettings.json
- [ ] Restarted the application
- [ ] Tested Microsoft sign-in

---

## Troubleshooting

**Error: "AADSTS700016: Application was not found"**
- Make sure the Client ID in appsettings.json matches exactly (no extra spaces)

**Error: "redirect_uri_mismatch"**
- Check that the redirect URIs in Azure match exactly what's in your app
- Make sure you included both `http://localhost:5000/signin-microsoft` and `http://localhost:7000/signin-microsoft`

**Can't see the secret value after creating it**
- The secret value is only shown once when created
- If you missed it, delete the old secret and create a new one

