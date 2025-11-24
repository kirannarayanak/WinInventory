# Railway Deployment - Step by Step Guide

## Prerequisites
- GitHub account (free)
- Your code in a Git repository
- OAuth credentials ready (Google & Microsoft)

---

## Step 1: Prepare Your Code for Git

### 1.1 Check if you have a Git repository
```powershell
git status
```

If you see "not a git repository", initialize one:
```powershell
git init
git add .
git commit -m "Initial commit - WinInventory app"
```

### 1.2 Create .gitignore (if not exists)
Create a `.gitignore` file to exclude sensitive files:

```
bin/
obj/
publish/
*.user
*.suo
appsettings.Development.json
.vs/
```

### 1.3 Create GitHub Repository
1. Go to https://github.com
2. Click "+" > "New repository"
3. Name: `WinInventory`
4. Description: "Windows Inventory and Mac Recommendation Tool"
5. Choose: **Private** (recommended) or Public
6. **DO NOT** initialize with README, .gitignore, or license
7. Click "Create repository"

### 1.4 Push Your Code to GitHub
```powershell
# Add your GitHub repository as remote
git remote add origin https://github.com/YOUR_USERNAME/WinInventory.git

# Push your code
git branch -M main
git push -u origin main
```

Replace `YOUR_USERNAME` with your actual GitHub username.

---

## Step 2: Sign Up for Railway

1. Go to **https://railway.app**
2. Click **"Start a New Project"** or **"Login"**
3. Choose **"Login with GitHub"**
4. Authorize Railway to access your GitHub account
5. You'll be redirected to Railway dashboard

---

## Step 3: Create New Project on Railway

1. In Railway dashboard, click **"+ New Project"**
2. Select **"Deploy from GitHub repo"**
3. You'll see a list of your GitHub repositories
4. Find and click on **"WinInventory"** (or your repo name)
5. Railway will start deploying automatically

---

## Step 4: Configure Build Settings

Railway should auto-detect .NET, but let's verify:

1. Click on your project in Railway
2. Click on the service (usually named after your repo)
3. Go to **"Settings"** tab
4. Check **"Build Command"** - should be: `dotnet publish -c Release -o ./publish`
5. Check **"Start Command"** - should be: `dotnet ./publish/WinInventory.dll`

If not set, update them:
- **Build Command:** `dotnet publish -c Release -o ./publish`
- **Start Command:** `dotnet ./publish/WinInventory.dll`

---

## Step 5: Add Environment Variables

1. In your Railway project, go to **"Variables"** tab
2. Click **"+ New Variable"**
3. Add these variables one by one:

```
GOOGLE_CLIENT_ID = YOUR_GOOGLE_CLIENT_ID_HERE
GOOGLE_CLIENT_SECRET = YOUR_GOOGLE_CLIENT_SECRET_HERE
MICROSOFT_CLIENT_ID = YOUR_MICROSOFT_CLIENT_ID_HERE
MICROSOFT_CLIENT_SECRET = YOUR_MICROSOFT_CLIENT_SECRET_HERE
```

4. Click **"Add"** for each variable
5. Railway will automatically redeploy when you add variables

---

## Step 6: Get Your Railway URL

1. In Railway, go to **"Settings"** tab
2. Scroll down to **"Domains"** section
3. You'll see a generated domain like: `wininventory-production.up.railway.app`
4. **Copy this URL** - you'll need it for OAuth redirect URIs

---

## Step 7: Update OAuth Redirect URIs

### 7.1 Update Google OAuth
1. Go to **Google Cloud Console**: https://console.cloud.google.com
2. Navigate to: **APIs & Services** > **Credentials**
3. Click on your OAuth 2.0 Client ID
4. Under **"Authorized redirect URIs"**, click **"+ ADD URI"**
5. Add: `https://YOUR-RAILWAY-URL.railway.app/signin-google`
   - Replace `YOUR-RAILWAY-URL` with your actual Railway domain
6. Click **"Save"**

### 7.2 Update Microsoft OAuth
1. Go to **Azure Portal**: https://portal.azure.com
2. Navigate to: **Azure Active Directory** > **App registrations**
3. Click on your **"WinInventory"** app
4. Go to **"Authentication"** in left menu
5. Under **"Redirect URIs"**, click **"+ Add URI"**
6. Add: `https://YOUR-RAILWAY-URL.railway.app/signin-microsoft`
7. Click **"Save"**

---

## Step 8: Wait for Deployment

1. Railway will automatically:
   - Build your application
   - Deploy it
   - Assign a URL

2. Watch the **"Deployments"** tab to see progress
3. When you see **"Deploy Succeeded"**, your app is live!

---

## Step 9: Test Your Deployment

1. Click on your Railway service
2. Go to **"Settings"** > **"Domains"**
3. Click on your domain URL (or copy it)
4. Your app should open in a new tab
5. Test:
   - Sign in with Google
   - Sign in with Microsoft
   - Check Mac recommendations

---

## Troubleshooting

### Build Fails
- Check the **"Deployments"** tab for error logs
- Make sure all NuGet packages are in `.csproj`
- Verify build command is correct

### OAuth Not Working
- Check environment variables are set correctly
- Verify redirect URIs match exactly (including https://)
- Check Railway logs for errors

### App Crashes
- Check **"Logs"** tab in Railway
- Look for error messages
- Verify all services are registered in `Program.cs`

### WMI Not Working
- Railway uses Windows containers, so WMI should work
- If issues, check Railway logs
- Verify the app is running on Windows container

---

## Next Steps After Deployment

1. ✅ Your app is live at: `https://your-app.railway.app`
2. ✅ Share the URL with users
3. ✅ Monitor usage in Railway dashboard
4. ✅ Check logs if issues occur

---

## Railway Free Tier Limits

- **$5 credit per month** (usually enough for small apps)
- **500 hours** of usage
- **Auto-sleeps** after inactivity (wakes on request)
- Upgrade to paid plan if you need more resources

---

## Useful Railway Commands

- **View Logs:** Click on service > "Logs" tab
- **Redeploy:** Click "Deploy" > "Redeploy"
- **View Metrics:** Click "Metrics" tab
- **Update Variables:** Click "Variables" tab

---

## Success Checklist

- [ ] Code pushed to GitHub
- [ ] Railway account created
- [ ] Project deployed from GitHub
- [ ] Environment variables added
- [ ] OAuth redirect URIs updated
- [ ] App is accessible via Railway URL
- [ ] Sign in works (Google & Microsoft)
- [ ] Mac recommendations work

---

**Ready to start? Let's begin with Step 1!**

