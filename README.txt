El apikey es: ok

$env:DATABASE_URL="postgres://uvufuefue:enetwetwetweufuemufemosas@peanut.db.aws.com/jonestxt"
Get-DbContext

// creare na nueva migraci�n
Add-Migration -Context WriteDbContext Initial

// actualziar la base de datos
Update-Database -Context WriteDbContext

// eliminar una migraci�n
Update-Database -Context WriteDbContext 0
Remove-Migration -Context WriteDbContext 

// https://devblogs.microsoft.com/dotnet/announcing-ef7-release-candidate-2/