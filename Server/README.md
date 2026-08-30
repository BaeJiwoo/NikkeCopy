# NikkeCopy Server

ASP.NET Core와 .NET 10 기반 게임 서버입니다.

## 프로젝트 구조

```text
Server/
├─ NikkeCopy.Api/             HTTP 요청과 응답, 서버 실행
├─ NikkeCopy.Application/     게임 유스케이스 조정
├─ NikkeCopy.Domain/          핵심 모델과 게임 규칙
└─ NikkeCopy.Infrastructure/  DB와 외부 시스템 구현
```

의존 방향은 `Api → Application → Domain`이며, Infrastructure는 Application에 선언된 인터페이스를 구현합니다. Domain과 Application은 Infrastructure를 참조하지 않습니다.

## 실행

저장소 루트에서 실행합니다.

```text
dotnet restore
dotnet build
dotnet run --project Server/NikkeCopy.Api --launch-profile http
```

서버 기본 주소는 `http://localhost:5000`입니다.

```http
GET http://localhost:5000/api/health
```

## 데이터베이스

EF Core와 MySQL이 Infrastructure에 구성되어 있습니다. 연결 문자열과 마이그레이션 설정은 [EF Core 설정 문서](NikkeCopy.Infrastructure/EFCORE_SETUP.md)를 참고합니다.

비밀번호와 인증 키는 커밋되는 설정 파일에 기록하지 않고 User Secrets 또는 환경변수로 관리합니다.

## 현재 상태

- 서버 빌드 성공
- Health API 구현
- Player 엔티티와 최초 DB 마이그레이션 구성
- Application 유스케이스, Repository 및 인증 API는 미구현

## 관련 문서

- [Api 계층](NikkeCopy.Api/README.md)
- [Application 계층](NikkeCopy.Application/README.md)
- [Domain 계층](NikkeCopy.Domain/README.md)
- [Infrastructure 계층](NikkeCopy.Infrastructure/README.md)
- [아키텍처](../Docs/Architecture.md)
- [유즈케이스](../Docs/UseCases/README.md)
