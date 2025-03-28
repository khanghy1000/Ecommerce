#### 1. (Development only) Copy secrets.json to ```Ecommerce.API``` folder.

#### 2. Install [.NET 9.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0).

#### 3. Install EF Core tools:
```bash
dotnet tool install --global dotnet-ef
```

#### 4. Create/update the database:
```bash
dotnet ef database update -p Ecommerce.Persistence -s Ecommerce.API
```

#### 5. Run the application:
```bash
dotnet run --project Ecommerce.API
```
