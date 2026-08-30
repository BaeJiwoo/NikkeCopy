# API

## Health Check

```http
GET http://localhost:5000/api/health
```

성공 응답은 HTTP 200과 다음 형태의 JSON입니다.

```json
{
  "status": "ok",
  "serverTime": "2026-01-01T00:00:00+00:00"
}
```

REST API 계약은 `Contracts/OpenAPI`에서 관리합니다.

## Authentication

### 계정 생성

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "player01",
  "password": "password123"
}
```

### 로그인

```http
POST /api/auth/login
Content-Type: application/json

{
  "username": "player01",
  "password": "password123"
}
```

성공 시 `accessToken`, `expiresAt`, `accountId`, `username`을 반환합니다.

### 현재 계정 확인

```http
GET /api/auth/me
Authorization: Bearer {accessToken}
```

JWT 서명 키는 설정 파일에 커밋하지 않습니다. 로컬 실행 전 다음 중 한 방식으로 32자 이상의 키를 설정합니다.

```powershell
$env:Jwt__Secret = "replace-with-a-local-secret-at-least-32-characters"
```

인증 테이블을 추가하려면 데이터베이스 연결 후 다음 마이그레이션을 적용합니다.

```text
dotnet ef database update --project Server/NikkeCopy.Infrastructure --startup-project Server/NikkeCopy.Api
```
