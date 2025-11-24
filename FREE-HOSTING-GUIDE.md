# Free Hosting Guide for WinInventory

## Best Free Hosting Options (Excluding Azure)

### Option 1: Railway (Recommended) ⭐
**Best for:** Easy deployment, Windows support, free tier

**Why Railway:**
- ✅ Free tier with $5 credit/month
- ✅ Supports Windows containers (needed for WMI)
- ✅ Easy Git-based deployment
- ✅ Automatic HTTPS
- ✅ Good performance

**Steps:**
1. Go to https://railway.app
2. Sign up with GitHub
3. Click "New Project" > "Deploy from GitHub repo"
4. Select your WinInventory repository
5. Railway will auto-detect .NET
6. Add environment variables for OAuth:
   - `GOOGLE_CLIENT_ID`
   - `GOOGLE_CLIENT_SECRET`
   - `MICROSOFT_CLIENT_ID`
   - `MICROSOFT_CLIENT_SECRET`
7. Update OAuth redirect URIs to your Railway URL

**Note:** Railway uses Windows containers, so WMI will work!

---

### Option 2: Render
**Best for:** Simple deployment, good free tier

**Why Render:**
- ✅ Free tier available
- ✅ Easy setup
- ⚠️ Linux only (WMI won't work, but app will run)
- ✅ Automatic HTTPS

**Steps:**
1. Go to https://render.com
2. Sign up with GitHub
3. Click "New" > "Web Service"
4. Connect your GitHub repository
5. Settings:
   - **Name:** wininventory
   - **Environment:** .NET
   - **Build Command:** `dotnet publish -c Release -o ./publish`
   - **Start Command:** `dotnet ./publish/WinInventory.dll`
6. Add environment variables for OAuth
7. Click "Create Web Service"

**Limitation:** Render uses Linux, so WMI calls won't work. The app will run, but machine detection features won't work.

---

### Option 3: Fly.io
**Best for:** Global deployment, good performance

**Why Fly.io:**
- ✅ Free tier (3 shared VMs)
- ✅ Global edge network
- ⚠️ Linux only
- ✅ Good documentation

**Steps:**
1. Install Fly CLI: `iwr https://fly.io/install.ps1 -useb | iex`
2. Sign up: `fly auth signup`
3. In your project: `fly launch`
4. Follow prompts
5. Add secrets: `fly secrets set GOOGLE_CLIENT_ID=...` etc.
6. Deploy: `fly deploy`

---

### Option 4: Self-Hosted (Free if you have Windows)
**Best for:** Full control, Windows support

**Requirements:**
- Windows Server or Windows 10/11 Pro
- IIS installed
- .NET 8.0 Hosting Bundle installed

**Steps:**
1. Publish your app: `dotnet publish -c Release -o ./publish`
2. Copy `publish` folder to your server
3. Create IIS website pointing to the folder
4. Configure app pool for .NET
5. Set environment variables in IIS
6. Configure firewall/port forwarding

**Pros:** Free, full Windows support, WMI works
**Cons:** Requires your own server, need to manage security

---

### Option 5: Oracle Cloud Always Free
**Best for:** Free VM with full control

**Why Oracle Cloud:**
- ✅ Always Free tier (2 VMs)
- ✅ Full Windows Server VM available
- ✅ 200GB storage
- ✅ Good for learning

**Steps:**
1. Sign up at https://cloud.oracle.com
2. Create a Windows Server VM (Always Free eligible)
3. RDP into the VM
4. Install .NET 8.0 Hosting Bundle
5. Deploy your app
6. Configure firewall rules

**Note:** Free tier has limitations, but good for testing.

---

## Recommended: Railway

**Why Railway is best for WinInventory:**
1. ✅ **Windows Support** - Uses Windows containers, so WMI works
2. ✅ **Easy Setup** - Just connect GitHub and deploy
3. ✅ **Free Tier** - $5 credit/month (usually enough for small apps)
4. ✅ **Automatic HTTPS** - SSL certificates included
5. ✅ **Environment Variables** - Easy OAuth configuration
6. ✅ **Auto-deploy** - Deploys on every Git push

---

## Deployment Steps for Railway

### 1. Prepare Your Code
```powershell
# Make sure your code is in a Git repository
git add .
git commit -m "Ready for deployment"
git push origin main
```

### 2. Create Railway Account
1. Go to https://railway.app
2. Sign up with GitHub
3. Authorize Railway to access your repos

### 3. Deploy
1. Click "New Project"
2. Select "Deploy from GitHub repo"
3. Choose your WinInventory repository
4. Railway will auto-detect it's a .NET app

### 4. Configure Environment Variables
In Railway dashboard, go to your project > Variables tab, add:
- `GOOGLE_CLIENT_ID` = your Google client ID
- `GOOGLE_CLIENT_SECRET` = your Google client secret
- `MICROSOFT_CLIENT_ID` = your Microsoft client ID
- `MICROSOFT_CLIENT_SECRET` = your Microsoft client secret

### 5. Update OAuth Redirect URIs
Update your OAuth providers with Railway URL:
- Google Cloud Console: Add `https://your-app.railway.app/signin-google`
- Azure Portal: Add `https://your-app.railway.app/signin-microsoft`

### 6. Deploy!
Railway will automatically:
- Build your app
- Deploy it
- Give you a URL like: `https://wininventory-production.up.railway.app`

---

## Important Notes

### For WMI to Work:
- **Railway (Windows containers)** ✅ - WMI will work
- **Self-hosted Windows** ✅ - WMI will work
- **Oracle Cloud Windows VM** ✅ - WMI will work
- **Render/Fly.io (Linux)** ❌ - WMI won't work, but app will run

### OAuth Redirect URIs:
After deployment, you MUST update:
1. **Google Cloud Console** - Add your production URL
2. **Azure Portal** - Add your production URL

### Environment Variables:
Never commit secrets to Git! Use:
- Railway: Project > Variables
- Render: Environment tab
- Fly.io: `fly secrets set`

---

## Quick Comparison

| Platform | Free Tier | Windows Support | WMI Works | Ease of Use |
|----------|-----------|-----------------|-----------|-------------|
| **Railway** | ✅ $5/month | ✅ Yes | ✅ Yes | ⭐⭐⭐⭐⭐ |
| **Render** | ✅ Yes | ❌ Linux only | ❌ No | ⭐⭐⭐⭐ |
| **Fly.io** | ✅ 3 VMs | ❌ Linux only | ❌ No | ⭐⭐⭐ |
| **Self-Hosted** | ✅ Free | ✅ Yes | ✅ Yes | ⭐⭐ |
| **Oracle Cloud** | ✅ Always Free | ✅ Yes | ✅ Yes | ⭐⭐⭐ |

---

## Recommendation

**Use Railway** - It's the easiest and supports Windows, so your WMI features will work perfectly!

