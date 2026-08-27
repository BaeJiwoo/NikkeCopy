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
