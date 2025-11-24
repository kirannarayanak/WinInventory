# Fix: Get the Correct Microsoft Client Secret Value

## The Problem
You copied the **Secret ID** (GUID), but you need the **Secret Value** (the actual password string).

## How to Get the Correct Secret Value

### Option 1: If You Just Created the Secret (Easiest)
1. Go to Azure Portal: https://portal.azure.com
2. Navigate to: **Azure Active Directory** > **App registrations**
3. Click on your **"WinInventory"** app
4. Go to **"Certificates & secrets"** in the left menu
5. Look at the **"Client secrets"** table
6. Find the secret you just created
7. Look at the **"Value"** column (NOT the "Secret ID" column)
8. The Value should be a long string like: `abc123~XYZ456-def789...`
9. Click the **copy icon** next to the Value to copy it
10. If you don't see the Value column, the secret might have expired or been hidden

### Option 2: If You Can't See the Value (Create a New Secret)
The secret value is only shown **once** when created. If you missed it:

1. Go to **"Certificates & secrets"** in your app
2. Find your existing secret in the table
3. Click the **three dots (...)** on the right
4. Click **"Delete"** to remove the old secret
5. Click **"+ New client secret"**
6. **Description**: Enter "WinInventory Secret 2"
7. **Expires**: Choose "24 months"
8. Click **"Add"**
9. **IMMEDIATELY** copy the **Value** that appears (it looks like: `abc123~XYZ456...`)
   - This is shown only once!
   - Copy it right away before the page refreshes

### What the Secret Value Looks Like
- ✅ **Correct**: `abc123~XYZ456-def789-ghi012-jkl345` (long string with special characters)
- ❌ **Wrong**: `2479a836-90cd-40dc-b21e-fdd117129b90` (GUID format - this is the Secret ID)

## After Getting the Correct Value
1. Open `appsettings.json`
2. Replace the Microsoft ClientSecret with the actual Value you just copied
3. Save the file
4. Restart your application

The secret value should be a long string, not a GUID!

