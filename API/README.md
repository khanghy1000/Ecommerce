#### 1. (Development only) Copy secrets.json to `Ecommerce.API` folder.

#### 2. (Optional) Create a new database in PostgreSQL and update the connection string in `Ecommerce.API/secrets.json` file.

#### 3. Create Vietnamese text search configuration in PostgreSQL. [Guide](https://gist.github.com/anhtran/de0691f848e115d841822baa6ee9f693)

#### 4. Install [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).

#### 5. Install EF Core tools:
```bash
dotnet tool install --global dotnet-ef
```

#### 6. Create/update the database:
```bash
dotnet ef database update -p Ecommerce.Persistence -s Ecommerce.API
```

#### 7. Run the application:
```bash
dotnet run --project Ecommerce.API
```
