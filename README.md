# SalonBook — Platformă web de programări salon

## Tehnologii folosite
- ASP.NET Core 8 MVC (C#)
- Entity Framework Core + SQLite (funcționează pe Mac, Windows, Linux)
- ASP.NET Core Identity (autentificare + roluri)

## Roluri
| Rol        | Acces                                       |
|------------|---------------------------------------------|
| Admin      | /Admin/* — toate datele                     |
| Detinator  | /Detinator/* — saloane, servicii, programari|
| Client     | /Client/* — programarile proprii            |

---

## Setup pe Mac (pas cu pas)

### 1. Instalează .NET 8 SDK
```bash
brew install dotnet
```
Sau descarcă de la: https://dotnet.microsoft.com/download/dotnet/8.0

Verifică instalarea:
```bash
dotnet --version
# trebuie sa afiseze 8.x.x
```

### 2. Instalează Entity Framework CLI tools
```bash
dotnet tool install --global dotnet-ef
```

### 3. Deschide terminalul în folderul proiectului
```bash
cd ~/Downloads/SalonBook
```

### 4. Restaurează pachetele NuGet
```bash
dotnet restore
```

### 5. Creează baza de date SQLite
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```
Se va crea fișierul `salonbook.db` în folderul proiectului.

### 6. Rulează aplicația
```bash
dotnet run
```
Deschide browserul la: **http://localhost:5000**

---

## Cont admin implicit
| Camp   | Valoare            |
|--------|--------------------|
| Email  | admin@salonbook.ro |
| Parola | Admin@1234         |

---

## Comenzi utile

Resetează baza de date:
```bash
dotnet ef database drop
dotnet ef database update
```

Rulează cu auto-reload:
```bash
dotnet watch run
```
