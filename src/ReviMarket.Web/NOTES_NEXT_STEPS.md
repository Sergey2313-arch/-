# ReviMarket next setup

## Database

Current startup still supports local demo database creation so the app does not break during development.

Tomorrow switch fully to EF migrations:

```powershell
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Then replace database startup with `Database.MigrateAsync()`.

## Local cleanup

Do not commit local build output or database files:

```powershell
Remove-Item bin -Recurse -Force
Remove-Item obj -Recurse -Force
Remove-Item revimarket.db -Force
```

Old static frontend files in repository root can be removed after the ASP.NET project is confirmed working:

```powershell
git rm index.html admin.html styles.css app.js stable-ui.js
git commit -m "Remove old static frontend"
git push
```

Main app path:

```text
src/ReviMarket.Web
```
