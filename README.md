El apikey es: ok

$env:DATABASE_URL="postgres://uvufuefue:enetwetwetweufuemufem@osas.db.aws.com/hollysheet"
Get-DbContext

```powershell
# creare na nueva migraci�n
Add-Migration -Context WriteDbContext Initial
```

```powershell
# actualziar la base de datos
Update-Database -Context WriteDbContext
```

```Powershell
# eliminar una migraci�n
Update-Database -Context WriteDbContext 0
Remove-Migration -Context WriteDbContext 
```