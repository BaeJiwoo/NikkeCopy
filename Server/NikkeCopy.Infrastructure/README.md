# NikkeCopy.Infrastructure

`NikkeCopy.Infrastructure`는 데이터베이스, 캐시, 인증, 외부 API처럼 애플리케이션 외부의 시스템과 연동하는 구현을 담당합니다.

Application 계층에 정의된 인터페이스를 구현하며, Domain과 Application 계층은 Infrastructure의 구체적인 구현을 알지 않아야 합니다.

## 디렉터리 구조

필요한 기능만 단계적으로 추가합니다. 초기에는 `Persistence`, `Repositories`, `DependencyInjection.cs`만으로 시작해도 충분합니다.

```text
NikkeCopy.Infrastructure/
├─ Persistence/
│  ├─ NikkeCopyDbContext.cs
│  ├─ Configurations/
│  │  ├─ PlayerConfiguration.cs
│  │  ├─ NikkeConfiguration.cs
│  │  └─ InventoryItemConfiguration.cs
│  └─ Migrations/
├─ Repositories/
│  ├─ PlayerRepository.cs
│  ├─ InventoryRepository.cs
│  └─ SquadRepository.cs
├─ Authentication/
│  ├─ JwtTokenService.cs
│  └─ PasswordHasher.cs
├─ Caching/
│  └─ RedisCacheService.cs
├─ ExternalServices/
│  └─ ExternalApiClient.cs
└─ DependencyInjection.cs
```

## 구성요소 설명

### `Persistence/`

EF Core와 데이터베이스 연결에 필요한 코드를 둡니다.

- `NikkeCopyDbContext.cs`: EF Core의 데이터베이스 세션과 엔티티 집합을 정의합니다.
- `Configurations/`: 테이블명, 기본 키, 컬럼 길이, 인덱스, 엔티티 관계 등을 엔티티별로 설정합니다.
- `Migrations/`: 데이터베이스 스키마 변경 이력을 보관합니다. EF Core 마이그레이션 명령으로 자동 생성합니다.

### `Repositories/`

Application 계층에 선언된 Repository 인터페이스를 구현합니다. EF Core를 이용한 조회와 저장 로직은 이곳에 둡니다.

예를 들어 Application의 `IPlayerRepository`는 Infrastructure의 `PlayerRepository`가 구현합니다.

```text
IPlayerRepository
        ↓ 구현
PlayerRepository
        ↓ 사용
NikkeCopyDbContext
        ↓
Database
```

Repository는 테이블마다 무조건 만들지 않고, `Player`, `Inventory`, `Squad`처럼 기능과 도메인의 중심 단위를 기준으로 만듭니다.

### `Authentication/`

인증과 보안에 관한 외부 기술 구현을 둡니다.

- JWT 발급 및 검증
- 비밀번호 해시 처리
- Google, Apple 등의 외부 로그인 연동

인증 기능을 실제로 도입할 때 생성합니다.

### `Caching/`

Redis 같은 캐시 시스템의 구현을 둡니다.

- 자주 조회하는 데이터 캐시
- 세션 관리
- 분산 잠금

캐시 시스템을 도입하기 전에는 만들 필요가 없습니다.

### `ExternalServices/`

결제, 푸시 알림, 외부 게임 서비스 등 외부 API 클라이언트의 구현을 둡니다.

외부 서비스의 인터페이스는 Application에 선언하고, 실제 HTTP 통신 구현은 이 디렉터리에 둡니다.

### `DependencyInjection.cs`

Infrastructure가 제공하는 구현체를 ASP.NET Core의 의존성 주입 컨테이너에 등록합니다.

주로 다음 항목을 등록합니다.

- `NikkeCopyDbContext`
- Repository 구현체
- 인증 서비스
- Redis 및 캐시 서비스
- 외부 API 클라이언트

API의 `Program.cs`에서는 다음과 같이 한 번에 등록하는 것을 목표로 합니다.

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
```

## 의존 관계

Infrastructure는 Application과 Domain을 참조할 수 있습니다.

```text
NikkeCopy.Api
       ↓ 구성 및 실행
NikkeCopy.Infrastructure
       ↓ 구현                 ↓ 사용
NikkeCopy.Application → NikkeCopy.Domain
```

- Domain은 Infrastructure를 참조하지 않습니다.
- Application은 Infrastructure를 참조하지 않습니다.
- Application은 필요한 기능을 인터페이스로 선언합니다.
- Infrastructure는 해당 인터페이스를 EF Core나 외부 라이브러리로 구현합니다.
- Api는 Application과 Infrastructure를 조립하고 실행합니다.

## 배치 기준

다음 코드는 Infrastructure에 둡니다.

- EF Core `DbContext`와 엔티티 매핑
- 데이터베이스 조회 및 저장 구현
- MySQL, Redis 등의 공급자별 코드
- JWT와 비밀번호 해시 구현
- 외부 API를 실제로 호출하는 코드

다음 코드는 Infrastructure에 두지 않습니다.

- 게임의 핵심 모델과 규칙: `NikkeCopy.Domain`
- 로그인, 레벨업, 모집 같은 유스케이스: `NikkeCopy.Application`
- Controller와 HTTP 요청·응답 DTO: `NikkeCopy.Api`

## 현재 상태

현재는 외부 패키지나 데이터베이스가 연결되어 있지 않습니다. 영속성 요구사항과 스키마가 정해지면 EF Core와 MySQL 구현을 추가합니다.
