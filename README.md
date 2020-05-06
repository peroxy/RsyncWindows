# RsyncWindows
Backs up data on your Windows machine running DeltaCopy to specified host via SSH and rsync protocol.

## How to run

1. Clone project.
2. Make sure you have DeltaCopy server running on your Windows machine (as a Windows service) and virtual directories setup. Aliases of those directories are used in secrets.json project file.
2. Open project in Visual Studio and set user secrets.json, should look like this:
```
{
  "BackupHostName": "domain.net",
  "Username": "user",
  "Password": "password",
  "DeltaCopyFolderAliases": ["pictures", "videos", "documents"]
}
 ```   
4. Build and run.
