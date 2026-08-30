# OpenAPI

Unity Client와 ASP.NET Core Server가 공유할 REST API 명세를 둡니다. 양쪽 구현 코드는 직접 공유하지 않습니다.

## 현재 상태

아직 OpenAPI YAML 또는 JSON 명세가 작성되지 않았습니다. 현재 구현된 엔드포인트는 `GET /api/health`이며, 설명은 [API 문서](../../Docs/Api.md)에 있습니다.

## 작성 원칙

- API 구현 전에 경로, 요청, 응답 및 오류 형식을 먼저 합의합니다.
- 요청·응답 필드의 타입과 필수 여부를 명시합니다.
- 성공 및 실패 상태 코드와 공통 오류 코드를 명시합니다.
- 인증이 필요한 API에는 Bearer 인증 요구사항을 표시합니다.
- Unity와 서버는 이 명세를 기준으로 각각 구현합니다.

초기 명세 파일은 이 디렉터리에 `openapi.yaml` 이름으로 추가할 예정입니다.
