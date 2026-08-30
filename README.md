# NikkeCopy

Unity + ASP.NET Core 기반 수집형 RPG 모작 프로젝트입니다. Client와 Server를 하나의 Git 저장소에서 관리합니다.

## 현재 상태

- Unity 클라이언트 기본 프로젝트와 공통 API 클라이언트 구성
- ASP.NET Core 계층형 서버 골격 구성
- Health API 구현
- EF Core와 MySQL 공급자 구성
- `Player` 엔티티와 최초 마이그레이션 작성
- GitHub Actions 서버 빌드 구성

현재 실제로 제공되는 API는 Health Check뿐입니다. 계정, 인증 및 게임 유스케이스 API는 문서화 단계이며 구현 예정입니다.

## Repository Structure

```text
Client/       Unity 클라이언트
Server/       ASP.NET Core 서버와 계층별 프로젝트
Contracts/    OpenAPI 및 향후 Protocol Buffers 계약
Docs/         아키텍처, API, 배포 및 유즈케이스 문서
```

상세 안내:

- [서버 구조와 실행 안내](Server/README.md)
- [Unity 클라이언트 안내](Client/README.md)
- [유즈케이스 목록](Docs/UseCases/README.md)
- [아키텍처](Docs/Architecture.md)
- [API 현황](Docs/Api.md)
- [목표 배포 구성](Docs/Deployment.md)
- [EF Core 설정](Server/NikkeCopy.Infrastructure/EFCORE_SETUP.md)

## Server Requirements

- .NET 10 SDK

## Server 실행 방법

아래 명령은 저장소 루트에서 실행합니다.

```bash
dotnet restore
dotnet build
dotnet run --project Server/NikkeCopy.Api --launch-profile http
```

MySQL을 사용하는 기능을 실행하려면 `DefaultConnection` 설정과 MySQL 서버가 필요합니다. DB 비밀번호는 저장소의 설정 파일에 기록하지 말고 User Secrets 또는 환경변수로 관리합니다.

### VS Code

저장소 루트를 VS Code로 연 뒤 권장 확장을 설치합니다.

- `Ctrl+Shift+B`: 전체 서버 복원 및 빌드
- `F5`: `NikkeCopy.Api`를 디버그 모드로 실행하고 Health API 열기
- `Terminal → Run Task → server: run`: 디버거 없이 서버 실행
- 실행 및 디버그 목록의 `Client: Attach to Unity`: 실행 중인 Unity Editor에 디버거 연결

## Health Check

```http
GET http://localhost:5000/api/health
```

## Unity

Unity Hub에서 다음 경로를 엽니다.

```text
Client/NikkeCopy.Unity
```

Unity 6 URP 프로젝트가 생성되어 있습니다. Network 계층의 `ApiClient`를 통해 서버에 접근하며 UI에서 `UnityWebRequest`를 직접 호출하지 않습니다.

서버 연결 확인용 `ServerHealthTest` 컴포넌트를 GameObject에 추가한 뒤 Play하면 Unity Console에서 Health API 응답을 확인할 수 있습니다.

## Infrastructure

### Docker

서버 및 의존 서비스를 위한 Docker 구성은 배포 요구사항이 정해진 뒤 추가합니다.

### MySQL / EF Core

EF Core와 MySQL 공급자, `NikkeCopyDbContext`, `Player` 매핑 및 최초 마이그레이션이 구성되어 있습니다. Repository와 실제 데이터 API는 아직 구현되지 않았습니다.

설치와 마이그레이션 절차는 [EF Core 설정 문서](Server/NikkeCopy.Infrastructure/EFCORE_SETUP.md)를 참고합니다.
