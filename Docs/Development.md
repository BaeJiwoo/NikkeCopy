# Development

## Server

필수 환경은 .NET 10 SDK입니다.

```bash
dotnet restore
dotnet build
dotnet run --project Server/NikkeCopy.Api --launch-profile http
```

로컬 서버는 `http://localhost:5000`에서 실행되며 Unity 테스트를 위해 HTTPS 강제 리다이렉션을 사용하지 않습니다.

## Unity

Unity Hub에서 `Client/NikkeCopy.Unity`를 엽니다. 프로젝트 생성 후 `Assets`, `Packages`, `ProjectSettings`와 모든 `.meta` 파일을 Git에 포함합니다.
