$env:DATABASE_URL="postgres://uvufuefue:enetwetwetweufuemufemosas@peanut.db.aws.com/jonestxt"
Get-DbContext

// creare na nueva migración
Add-Migration -Context WriteDbContext Initial

// actualziar la base de datos
Update-Database -Context WriteDbContext

// eliminar una migración
Update-Database -Context WriteDbContext 0
Remove-Migration -Context WriteDbContext 