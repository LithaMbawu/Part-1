# RaceDay API Endpoint Plan

Base URL: `/api/v1`  
Authentication: bearer JWT in `Authorization: Bearer <token>` unless marked Public.  
IDs are integers. Dates use ISO 8601 UTC. Error responses use `{ "message": "...", "errors": [] }`.

| HTTP Method | Route | Description | Role Required | Request Body (if any) | Expected Response |
|---|---|---|---|---|---|
| POST | `/auth/register` | Register a participant account | Public | `{ firstName, lastName, email, password, phone }` | `201 Created`, user profile without password |
| POST | `/auth/login` | Authenticate and issue a JWT | Public | `{ email, password }` | `200 OK`, `{ token, expiresAt, user }` |
| GET | `/users/me` | Get the authenticated user's profile | Authenticated | None | `200 OK`, profile |
| PUT | `/users/me` | Update the authenticated user's profile | Authenticated | `{ firstName, lastName, phone }` | `200 OK`, updated profile |
| GET | `/events` | Browse events with optional status, date, and category filters | Public | None; query parameters optional | `200 OK`, paged event summaries |
| GET | `/events/{eventId}` | View event details, categories, and enrolment count | Public | None | `200 OK`, event detail; `404` if absent |
| POST | `/events` | Create an event owned by the current organiser | Organiser | `{ name, description, eventDate, venue, capacity, categoryIds }` | `201 Created`, event detail and `Location` header |
| PUT | `/events/{eventId}` | Edit an event before it starts | Organiser (owner) | Event fields and `categoryIds` | `200 OK`, updated event |
| DELETE | `/events/{eventId}` | Cancel an event without deleting history | Organiser (owner) or Admin | None | `204 No Content` |
| GET | `/categories` | List available race categories | Public | None | `200 OK`, category collection |
| GET | `/categories/{categoryId}` | View a category and its active events | Public | None | `200 OK`, category detail; `404` if absent |
| POST | `/categories` | Add a reusable category | Admin | `{ name, description }` | `201 Created`, category |
| PUT | `/categories/{categoryId}` | Update a category label or description | Admin | `{ name, description }` | `200 OK`, category |
| DELETE | `/categories/{categoryId}` | Remove an unused category | Admin | None | `204 No Content`; `409` if used by an event |
| GET | `/events/{eventId}/enrolments` | List enrolments for event management | Organiser (owner) or Admin | None | `200 OK`, participant/category/status list |
| POST | `/events/{eventId}/enrolments` | Enrol the current participant in an offered category | Participant | `{ categoryId }` | `201 Created`, enrolment; `409` if duplicate/full |
| GET | `/users/me/enrolments` | List the current participant's enrolments | Participant | None | `200 OK`, enrolment collection |
| DELETE | `/enrolments/{enrolmentId}` | Withdraw before the event cutoff | Participant (owner) or Admin | None | `204 No Content`; `409` after cutoff |
| PATCH | `/enrolments/{enrolmentId}/status` | Confirm or update an enrolment status | Organiser (event owner) or Admin | `{ status }` (`Pending`, `Confirmed`, `Withdrawn`) | `200 OK`, updated enrolment |
| GET | `/events/{eventId}/results` | View published results ordered by position | Public | None | `200 OK`, result collection |
| POST | `/enrolments/{enrolmentId}/results` | Record a result for a completed enrolment | Organiser (event owner) or Admin | `{ position, finishTimeSeconds, status }` | `201 Created`, result |
| PUT | `/results/{resultId}` | Correct a recorded result | Organiser (event owner) or Admin | `{ position, finishTimeSeconds, status }` | `200 OK`, updated result |
| DELETE | `/results/{resultId}` | Remove an incorrectly recorded result | Organiser (event owner) or Admin | None | `204 No Content` |

## Cross-cutting responses

`400 Bad Request` indicates validation failure, `401 Unauthorized` indicates a missing/invalid token, `403 Forbidden` indicates an insufficient role or ownership failure, `404 Not Found` indicates a missing resource, and `409 Conflict` indicates a duplicate, capacity, or state conflict.

## Request and response conventions

All request property names use camelCase. Successful collection responses use `{ "items": [], "page": 1, "pageSize": 20, "total": 0 }` unless the endpoint returns a simple category collection. Successful resource responses use the resource object directly. Validation errors use `{ "message": "Validation failed", "errors": [{ "field": "email", "message": "A valid email is required" }] }`.

The `GET /events` query parameters are `status`, `fromDate`, `toDate`, `categoryId`, `page`, and `pageSize`. `page` defaults to `1`; `pageSize` defaults to `20` and is limited to `100`.

Registration accepts `firstName`, `lastName`, `email`, `password`, and optional `phone`. Event creation and update accept `name`, optional `description`, `eventDate`, `venue`, positive `capacity`, and one or more `categoryIds`. Result creation and update accept optional positive `position`, optional positive `finishTimeSeconds`, and `status` from `Pending`, `Published`, or `Disqualified`.

## Route-specific failure cases

| Route(s) | Failure case | Response |
|---|---|---|
| `POST /auth/register` | Invalid fields or an email that already exists | `400 Bad Request` or `409 Conflict` |
| `POST /auth/login` | Unknown email or incorrect password | `401 Unauthorized` |
| `GET/PUT /users/me` | Missing or invalid token; invalid profile fields | `401 Unauthorized` or `400 Bad Request` |
| `GET /events`, `GET /categories` | Invalid filter, date, or pagination value | `400 Bad Request` |
| `GET /events/{eventId}`, `GET /categories/{categoryId}` | Resource does not exist | `404 Not Found` |
| `POST /events` | Non-organiser, invalid category IDs, or invalid event data | `403 Forbidden` or `400 Bad Request` |
| `PUT/DELETE /events/{eventId}` | Missing event, non-owner, or event has already started | `404 Not Found`, `403 Forbidden`, or `409 Conflict` |
| `POST/PUT/DELETE /categories...` | Non-admin, duplicate name, or category is in use | `403 Forbidden`, `409 Conflict`, or `404 Not Found` |
| `GET /events/{eventId}/enrolments` | Missing event or caller is not its organiser/admin | `404 Not Found` or `403 Forbidden` |
| `POST /events/{eventId}/enrolments` | Event/category unavailable, duplicate enrolment, or capacity reached | `400 Bad Request`, `404 Not Found`, or `409 Conflict` |
| `GET /users/me/enrolments`, `DELETE /enrolments/{enrolmentId}` | Missing token, enrolment does not belong to caller, or cutoff passed | `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, or `409 Conflict` |
| `PATCH /enrolments/{enrolmentId}/status` | Invalid status transition or caller lacks event ownership | `400 Bad Request`, `403 Forbidden`, `404 Not Found`, or `409 Conflict` |
| `GET /events/{eventId}/results` | Event does not exist | `404 Not Found` |
| `POST /enrolments/{enrolmentId}/results` | Missing enrolment, duplicate result, invalid result, or event is not complete | `400 Bad Request`, `404 Not Found`, or `409 Conflict` |
| `PUT/DELETE /results/{resultId}` | Missing result or caller lacks event ownership | `403 Forbidden`, `404 Not Found`, or `409 Conflict` |