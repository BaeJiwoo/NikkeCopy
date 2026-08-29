# EF Core 설정 요약

이 문서는 NikkeCopy 서버에 EF Core와 MySQL을 설정하는 순서를 정리합니다.

## 바로가기

- [1. 명령 실행 위치](#1-명령-실행-위치)
- [2. 패키지 설치](#2-패키지-설치)
- [3. Domain 엔티티 작성](#3-domain-엔티티-작성)
- [4. DbContext 작성](#4-dbcontext-작성)
- [5. 엔티티 매핑 작성](#5-엔티티-매핑-작성)
- [6. 연결 문자열 설정](#6-연결-문자열-설정)
- [7. Infrastructure 서비스 등록](#7-infrastructure-서비스-등록)
- [8. Api에서 Infrastructure 호출](#8-api에서-infrastructure-호출)
- [9. 빌드 확인](#9-빌드-확인)
- [10. dotnet-ef 설치](#10-dotnet-ef-설치)
- [11. 첫 마이그레이션 생성](#11-첫-마이그레이션-생성)
- [12. 데이터베이스 반영](#12-데이터베이스-반영)
- [전체 순서](#전체-순서)
- [자주 발생하는 오류](#자주-발생하는-오류)

## 1. 명령 실행 위치

별도 안내가 없는 모든 명령은 저장소 루트에서 실행합니다.

PowerShell:

```powershell
cd C:\Users\gunwo\Projects\NikkeCopy
```

CMD:

```cmd
cd /d C:\Users\gunwo\Projects\NikkeCopy
```

> PowerShell에서 여러 줄 명령을 작성할 때는 백틱(`` ` ``)을 사용하고, CMD에서는 캐럿(`^`)을 사용합니다. 줄바꿈 문자 뒤에는 공백을 넣지 않습니다. 한 줄로 실행하는 명령은 두 셸에서 동일하게 사용할 수 있습니다.

프로젝트 경로는 다음과 같습니다.

```text
Server/NikkeCopy.Infrastructure  EF Core와 MySQL 구현
Server/NikkeCopy.Api             서버 실행 및 환경설정
Server/NikkeCopy.Domain          데이터베이스에 저장할 도메인 엔티티
```

## 2. 패키지 설치

이 프로젝트는 `net10.0`과 EF Core 10을 사용합니다. EF Core 9용인 Pomelo 9 패키지가 설치되어 있다면 먼저 제거합니다.

PowerShell 또는 CMD:

```text
dotnet remove Server/NikkeCopy.Infrastructure package Pomelo.EntityFrameworkCore.MySql
```

EF Core 본체, 설계 도구, MySQL 공급자를 Infrastructure 프로젝트에 설치합니다.

PowerShell 또는 CMD:

```text
dotnet add Server/NikkeCopy.Infrastructure package Microsoft.EntityFrameworkCore --version 10.0.11
dotnet add Server/NikkeCopy.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 10.0.11
dotnet add Server/NikkeCopy.Infrastructure package MySql.EntityFrameworkCore --version 10.0.9
```

EF Core 도구 패키지는 실행 프로젝트인 Api에 설치합니다.

PowerShell 또는 CMD:

```text
dotnet add Server/NikkeCopy.Api package Microsoft.EntityFrameworkCore.Tools --version 10.0.11
```

패키지를 복원합니다.

PowerShell 또는 CMD:

```text
dotnet restore
```

## 3. Domain 엔티티 작성

예시 위치:

```text
Server/NikkeCopy.Domain/Player/Player.cs
```

```csharp
namespace NikkeCopy.Domain.Players;

public sealed class Player
{
    public long Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    private Player()
    {
    }

    public Player(string name)
    {
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }
}
```

## 4. DbContext 작성

파일 위치:

```text
Server/NikkeCopy.Infrastructure/Persistence/NikkeCopyDbContext.cs
```

```csharp
using Microsoft.EntityFrameworkCore;
using NikkeCopy.Domain.Players;

namespace NikkeCopy.Infrastructure.Persistence;

public sealed class NikkeCopyDbContext : DbContext
{
    public NikkeCopyDbContext(
        DbContextOptions<NikkeCopyDbContext> options)
        : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(NikkeCopyDbContext).Assembly);
    }
}
```

새로운 엔티티가 생기면 해당 `DbSet`을 추가합니다.

## 5. 엔티티 매핑 작성

예시 파일:

```text
Server/NikkeCopy.Infrastructure/Persistence/Configurations/PlayerConfiguration.cs
```

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NikkeCopy.Domain.Players;

namespace NikkeCopy.Infrastructure.Persistence.Configurations;

public sealed class PlayerConfiguration : IEntityTypeConfiguration<Player>
{
    public void Configure(EntityTypeBuilder<Player> builder)
    {
        builder.ToTable("players");

        builder.HasKey(player => player.Id);

        builder.Property(player => player.Id)
            .ValueGeneratedOnAdd();

        builder.Property(player => player.Name)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(player => player.CreatedAt)
            .IsRequired();

        builder.HasIndex(player => player.Name);
    }
}
```

## 6. 연결 문자열 설정

개발 환경의 연결 문자열은 Api 프로젝트에서 관리합니다.

파일 위치:

```text
Server/NikkeCopy.Api/appsettings.Development.json
```

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=nikke_copy;User=root;Password=개발용비밀번호"
  }
}
```

실제 비밀번호를 Git에 올리지 않으려면 User Secrets를 사용합니다.

PowerShell:

```powershell
dotnet user-secrets init --project Server/NikkeCopy.Api

dotnet user-secrets set `
  "ConnectionStrings:DefaultConnection" `
  "Server=localhost;Port=3306;Database=nikke_copy;User=root;Password=실제비밀번호" `
  --project Server/NikkeCopy.Api
```

CMD:

```cmd
dotnet user-secrets init --project Server\NikkeCopy.Api

dotnet user-secrets set ^
  "ConnectionStrings:DefaultConnection" ^
  "Server=localhost;Port=3306;Database=nikke_copy;User=root;Password=실제비밀번호" ^
  --project Server\NikkeCopy.Api
```

CMD에서 한 줄로 실행해도 됩니다.

```cmd
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=nikke_copy;User=root;Password=실제비밀번호" --project Server\NikkeCopy.Api
```

## 7. Infrastructure 서비스 등록

파일 위치:

```text
Server/NikkeCopy.Infrastructure/DependencyInjection.cs
```

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NikkeCopy.Infrastructure.Persistence;

namespace NikkeCopy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "DefaultConnection 연결 문자열이 없습니다.");

        services.AddDbContext<NikkeCopyDbContext>(options =>
            options.UseMySQL(connectionString));

        return services;
    }
}
```

`MySql.EntityFrameworkCore`에서는 `UseMySQL`을 사용합니다.

## 8. Api에서 Infrastructure 호출

`Server/NikkeCopy.Api/Program.cs`에 Infrastructure를 등록합니다.

```csharp
using NikkeCopy.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapControllers();

app.Run();
```

## 9. 빌드 확인

마이그레이션을 만들기 전에 반드시 빌드가 성공하는지 확인합니다.

PowerShell 또는 CMD:

```text
dotnet build
```

코드 자동완성이 동작하지 않는 경우에도 먼저 빌드 오류를 해결한 뒤 에디터를 다시 불러옵니다.

## 10. dotnet-ef 설치

`dotnet-ef`는 프로젝트 패키지가 아니라 컴퓨터에서 사용하는 명령줄 도구입니다.

처음 설치하는 경우:

PowerShell 또는 CMD:

```text
dotnet tool install --global dotnet-ef --version 10.0.11
```

이미 설치된 경우:

PowerShell 또는 CMD:

```text
dotnet tool update --global dotnet-ef --version 10.0.11
```

설치 확인:

PowerShell 또는 CMD:

```text
dotnet ef --version
```

## 11. 첫 마이그레이션 생성

저장소 루트에서 실행합니다.

PowerShell:

```powershell
dotnet ef migrations add InitialCreate `
  --project Server/NikkeCopy.Infrastructure `
  --startup-project Server/NikkeCopy.Api `
  --output-dir Persistence/Migrations
```

CMD:

```cmd
dotnet ef migrations add InitialCreate ^
  --project Server\NikkeCopy.Infrastructure ^
  --startup-project Server\NikkeCopy.Api ^
  --output-dir Persistence\Migrations
```

CMD에서 한 줄로 실행:

```cmd
dotnet ef migrations add InitialCreate --project Server\NikkeCopy.Infrastructure --startup-project Server\NikkeCopy.Api --output-dir Persistence\Migrations
```

- `--project`: DbContext와 마이그레이션이 들어가는 Infrastructure 프로젝트
- `--startup-project`: 연결 문자열과 실행 설정을 제공하는 Api 프로젝트
- `--output-dir`: Infrastructure 프로젝트를 기준으로 한 마이그레이션 출력 위치

생성 결과는 다음 위치에 들어갑니다.

```text
Server/NikkeCopy.Infrastructure/Persistence/Migrations/
```

## 12. 데이터베이스 반영

MySQL 서버가 실행 중인지 확인한 다음 저장소 루트에서 실행합니다.

PowerShell:

```powershell
dotnet ef database update `
  --project Server/NikkeCopy.Infrastructure `
  --startup-project Server/NikkeCopy.Api
```

CMD:

```cmd
dotnet ef database update ^
  --project Server\NikkeCopy.Infrastructure ^
  --startup-project Server\NikkeCopy.Api
```

CMD에서 한 줄로 실행:

```cmd
dotnet ef database update --project Server\NikkeCopy.Infrastructure --startup-project Server\NikkeCopy.Api
```

## 전체 순서

```text
1. 저장소 루트로 이동
2. EF Core와 MySQL 패키지 설치
3. Domain 엔티티 작성
4. DbContext 작성
5. 엔티티 매핑 작성
6. 연결 문자열 설정
7. DependencyInjection 작성
8. Program.cs에서 AddInfrastructure 호출
9. dotnet build
10. 마이그레이션 생성
11. 데이터베이스 반영
```

## 자주 발생하는 오류

### `DbContext`를 찾지 못하는 경우

`NikkeCopyDbContext`가 자기 자신이 아니라 EF Core의 `DbContext`를 상속하는지 확인합니다.

```csharp
public sealed class NikkeCopyDbContext : DbContext
```

### `Player`를 찾지 못하는 경우

클래스에 선언된 실제 namespace와 `using`이 일치하는지 확인합니다.

```csharp
using NikkeCopy.Domain.Players;
```

### `ApplyConfigurationsFromAssembly`가 보이지 않는 경우

메서드 이름의 `Configurations`가 복수형인지 확인합니다.

```csharp
modelBuilder.ApplyConfigurationsFromAssembly(
    typeof(NikkeCopyDbContext).Assembly);
```

### 마이그레이션 명령이 실패하는 경우

다음 항목을 순서대로 확인합니다.

1. `dotnet build`가 성공하는지 확인합니다.
2. MySQL 서버가 실행 중인지 확인합니다.
3. `DefaultConnection`이 설정되었는지 확인합니다.
4. `--project`와 `--startup-project` 경로가 올바른지 확인합니다.
5. EF Core 패키지와 `dotnet-ef`의 주 버전이 같은지 확인합니다.
