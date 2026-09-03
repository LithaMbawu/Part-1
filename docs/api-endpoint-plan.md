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