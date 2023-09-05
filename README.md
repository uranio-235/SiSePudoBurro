El apikey es: 

```
ok
```

### Ay pero como hago migraciones?

$env:DATABASE_URL="postgres://uvufuefue:enetwetwetweufuemufem@osas.db.aws.com/hollysheet"

Usando el [Package Manager Console de Visual Studio](https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-powershell), mete esto:

```powershell
# crear una nueva migración
Add-Migration -Context WriteDbContext Initial
```

```powershell
# actualizar la base de datos
Update-Database -Context WriteDbContext
```

```Powershell
# eliminar una migración
Update-Database -Context WriteDbContext 0
Remove-Migration -Context WriteDbContext 
```

# Malas noticias

Se demora en levantar cuando arranca. Tiene que crear e inicializar la base de datos.

Postgres no va pinchar hasta que no quede alineada la api de json column con Entity framework. La capa de datos está ahí para ilustrar como convertir la DATABASE_URL en una connection string y como usar columnas json para hacer [tablas desnormalizadas](https://en.wikipedia.org/wiki/Denormalization).

Si quieres saber como se hace con la API de Npgsql, [míralo aquí:](https://www.npgsql.org/efcore/mapping/json.html?tabs=data-annotations%2Cpoco)