# TMS API Documentation

You must authenticate before accessing any API endpoint. 

![image.png](/.attachments/image-49486fcc-4b83-4cdc-b7bd-3976024b26c0.png)
![image.png](/.attachments/image-4b2f6af7-6503-4377-9739-019095303f1e.png)
![image.png](/.attachments/image-c0fa3f9d-0c95-43ab-a8ca-800e6769c6b3.png)

Once you check the scope, you can authorize and if you haven't before, it will take you through SSO.
Once authenticated, you can execute any endpoints for the rest of the session.

## Students

### GET /Students

**Purpose:**  
Returns a list of all students registered in the system. 



**Request Parameters:**  
None.

**Request Body:**  
None.

**Response — 200 OK**

| Field | Type | Description |
|---|---|---|
| `studentId` | `int` | Unique identifier for the student |
| `name` | `string` | Full name of the student |
| `oid` | `string` | External identity provider object ID |
| `teacherId` | `int` | ID of the assigned teacher |

**Success Response Example (200 OK)**
```json
[
  {
    "studentId": 1,
    "name": "Alice Smith",
    "oid": "auth0|abc123",
    "teacherId": 2
  },
  {
    "studentId": 2,
    "name": "Bob Jones",
    "oid": "auth0|def456",
    "teacherId": 2
  }
]
```

**Error Response Example (500 Internal Server Error)**
```json
{
  "status": 500,
  "title": "An error occurred while processing your request."
}
```

![image.png](/.attachments/image-1c9fbbdb-ca05-4ef3-98d0-0d7ce3296eb2.png)
---

### GET /Students/{id}

**Purpose:**  
Retrieves a single student by their integer primary key. Used when displaying a student's profile or detail page.

**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `int` | ✅ Yes | The unique student ID |

**Request Body:**  
None.

**Response — 200 OK**

| Field | Type | Description |
|---|---|---|
| `studentId` | `int` | Unique identifier |
| `name` | `string` | Full name |
| `oid` | `string` | Identity provider OID |
| `teacherId` | `int` | Assigned teacher ID |

**Success Response Example (200 OK)**
```json
{
  "studentId": 1,
  "name": "Alice Smith",
  "oid": "auth0|abc123",
  "teacherId": 2
}
```

**Error Response Example (404 Not Found)**
```json
{
  "status": 404,
  "title": "Not Found"
}
```

![image.png](/.attachments/image-41ca115f-17aa-4248-b758-79a127f02166.png)

---

### GET /Students/ByOID/{id}

**Purpose:**  
Looks up a student using their external identity provider Object ID (OID). Used during authentication flows to resolve the internal student record from an external identity token.

**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `string` | ✅ Yes | The external OID (e.g. from Azure AD / Auth0) |

**Request Body:**  
None.

**Response — 200 OK**

Returns the student's internal integer ID.

| Field | Type | Description |
|---|---|---|
| *(root value)* | `int` | The internal student ID matching the given OID |

**Success Response Example (200 OK)**
```json
3
```

**Error Response Example (404 Not Found)**
```json
{
  "status": 404,
  "title": "Not Found"
}
```

![image.png](/.attachments/image-ce9e50a9-618c-4b3a-aa7a-c7bfaebb4b15.png)
---

### PUT /Students/BorrowToy/{id}

**Purpose:**  
Records a toy loan for a specific student. The toy must exist and be currently available. Used when a student checks out a toy from the library.

**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `int` | ✅ Yes | The student's ID |
| `toyId` | Query | `int` | ✅ Yes | The ID of the toy to borrow |

**Request Body:**  
None.

**Response — 204 No Content**  
Returned on success. No response body.

**Success Response Example (204 No Content)**
```
HTTP/1.1 204 No Content
```

**Error Response Examples**

Student not found (404):
```json
{
  "status": 404,
  "title": "Not Found"
}
```

Toy not found (400):
```json
"Toy not found"
```

Toy already borrowed (400):
```json
"Toy is not available"
```

![image.png](/.attachments/image-dd0821e7-2869-4e24-a6e0-9188005cdcd9.png)
---

### PUT /Students/ReturnToy/{id}

**Purpose:**  
Marks a toy as returned by a student. Validates that the student currently has an active loan for the specified toy. Used when a student returns a toy.


**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `int` | ✅ Yes | The student's ID |
| `toyId` | Query | `int` | ✅ Yes | The ID of the toy being returned |

**Request Body:**  
None.

**Response — 204 No Content**  
Returned on success. No response body.

**Success Response Example (204 No Content)**
```
HTTP/1.1 204 No Content
```

**Error Response Examples**

Student not found (404):
```json
{
  "status": 404,
  "title": "Not Found"
}
```

No active loan found for this student and toy (400):
```json
"Could not find a current toy loan for this student and toy"
```

![image.png](/.attachments/image-01086fac-36a8-40e2-a552-6190ec7c2625.png)

---

### GET /Students/{id}/BorrowHistory

**Purpose:**  
Returns the full toy loan history for a student, including past and current loans. Useful for reports and reviewing a student's borrowing activity.


**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `int` | ✅ Yes | The student's ID |

**Request Body:**  
None.

**Response — 200 OK**

Returns an array of `ToyLoanDTO` objects.

| Field | Type | Description |
|---|---|---|
| `toyLoanId` | `int` | Unique loan record ID |
| `studentId` | `int` | ID of the borrowing student |
| `toyId` | `int` | ID of the borrowed toy |
| `borrowDate` | `string (datetime)` | Date and time of borrowing |
| `returnDate` | `string (datetime) \| null` | Date returned, or `null` if still on loan |

**Success Response Example (200 OK)**
```json
[
  {
    "toyLoanId": 10,
    "studentId": 1,
    "toyId": 5,
    "borrowDate": "2025-03-01T09:00:00",
    "returnDate": "2025-03-05T14:30:00"
  },
  {
    "toyLoanId": 15,
    "studentId": 1,
    "toyId": 8,
    "borrowDate": "2025-04-10T10:00:00",
    "returnDate": null
  }
]
```

**Error Response Example (404 Not Found)**
```json
"Student not found"
```

![image.png](/.attachments/image-441b430b-9f65-4e2f-b490-d2cd00487a57.png)
---

### GET /Students/{id}/CurrentLoans

**Purpose:**  
Returns only the active (unreturned) loans for a student. Used to display what toys a student currently has checked out.


**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `int` | ✅ Yes | The student's ID |

**Request Body:**  
None.

**Response — 200 OK**

Returns an array of `ToyLoanDTO` objects where `returnDate` is `null`.

| Field | Type | Description |
|---|---|---|
| `toyLoanId` | `int` | Unique loan record ID |
| `studentId` | `int` | ID of the borrowing student |
| `toyId` | `int` | ID of the borrowed toy |
| `borrowDate` | `string (datetime)` | Date and time of borrowing |
| `returnDate` | `null` | Always `null` for current loans |

**Success Response Example (200 OK)**
```json
[
  {
    "toyLoanId": 15,
    "studentId": 1,
    "toyId": 8,
    "borrowDate": "2025-04-10T10:00:00",
    "returnDate": null
  }
]
```

**Error Response Example (404 Not Found)**
```json
"Student not found"
```

![image.png](/.attachments/image-97b7012d-7de7-44da-8947-f0da8d5929a7.png)
---

## Teachers

### GET /Teachers

**Purpose:**  
Returns a list of all teachers in the system. Used to populate teacher management screens or assignment dropdowns.

**Request Parameters:**  
None.

**Request Body:**  
None.

**Response — 200 OK**

| Field | Type | Description |
|---|---|---|
| `teacherId` | `int` | Unique identifier |
| `name` | `string` | Full name of the teacher |
| `email` | `string` | Teacher's email address |

**Success Response Example (200 OK)**
```json
[
  {
    "teacherId": 1,
    "name": "Ms. Johnson",
    "email": "johnson@school.edu"
  },
  {
    "teacherId": 2,
    "name": "Mr. Patel",
    "email": "patel@school.edu"
  }
]
```

**Error Response Example (500 Internal Server Error)**
```json
{
  "status": 500,
  "title": "An error occurred while processing your request."
}
```
![image.png](/.attachments/image-ad044c29-3cf1-42fb-b182-93ae597420c8.png)
---

### GET /Teachers/{id}

**Purpose:**  
Retrieves a single teacher by their primary key. Used when viewing or editing an individual teacher's profile.


**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `int` | ✅ Yes | The teacher's ID |

**Request Body:**  
None.

**Response — 200 OK**

| Field | Type | Description |
|---|---|---|
| `teacherId` | `int` | Unique identifier |
| `name` | `string` | Full name |
| `email` | `string` | Email address |

**Success Response Example (200 OK)**
```json
{
  "teacherId": 1,
  "name": "Ms. Johnson",
  "email": "johnson@school.edu"
}
```

**Error Response Example (404 Not Found)**
```json
{
  "status": 404,
  "title": "Not Found"
}
```

![image.png](/.attachments/image-8fc7cb0e-f5fa-4180-9223-c07afb4b8fe9.png)

---
## Toys

### GET /Toys

**Purpose:**  
Returns all toys in the system regardless of availability. Used to display the full toy catalogue to staff or administrators.


**Request Parameters:**  
None.

**Request Body:**  
None.

**Response — 200 OK**

| Field | Type | Description |
|---|---|---|
| `toyId` | `int` | Unique identifier |
| `name` | `string` | Toy name |
| `material` | `string` | Primary material (e.g. "Plastic", "Wood") |
| `category` | `string` | Category label |
| `isAvailable` | `bool` | Whether the toy is currently available to borrow |

**Success Response Example (200 OK)**
```json
[
  {
    "toyId": 1,
    "name": "Wooden Blocks",
    "material": "Wood",
    "category": "Construction",
    "isAvailable": true
  },
  {
    "toyId": 2,
    "name": "Plastic Car",
    "material": "Plastic",
    "category": "Vehicles",
    "isAvailable": false
  }
]
```

**Error Response Example (500 Internal Server Error)**
```json
{
  "status": 500,
  "title": "An error occurred while processing your request."
}
```

> 📸 ![image.png](/.attachments/image-6f946f05-3c62-4b61-8eb3-d6538a7f9ddd.png)
---

### POST /Toys

**Purpose:**  
Creates a new toy and adds it to the catalogue. Used by staff when registering a newly acquired toy.

**Request Parameters:**  
None.

**Request Body**

| Field | Type | Required | Constraints |
|---|---|---|---|
| `name` | `string` | ✅ Yes | Non-empty string |
| `material` | `string` | ✅ Yes | Non-empty string |
| `category` | `string` | ✅ Yes | Non-empty string |
| `isAvailable` | `bool` | ✅ Yes | Typically `true` for a new toy |

**Request Body Example**
```json
{
  "name": "Foam Puzzle Mat",
  "material": "Foam",
  "category": "Educational",
  "isAvailable": true
}
```

**Response — 201 Created**

Returns the newly created toy including its server-assigned `toyId`.

| Field | Type | Description |
|---|---|---|
| `toyId` | `int` | The newly assigned ID |
| `name` | `string` | Toy name |
| `material` | `string` | Material |
| `category` | `string` | Category |
| `isAvailable` | `bool` | Availability flag |

**Success Response Example (201 Created)**
```json
{
  "toyId": 10,
  "name": "Foam Puzzle Mat",
  "material": "Foam",
  "category": "Educational",
  "isAvailable": true
}
```

**Error Response Example (400 Bad Request)**
```json
{
  "errors": {
    "name": ["The Name field is required."]
  },
  "status": 400,
  "title": "One or more validation errors occurred."
}
```

> 📸 ![image.png](/.attachments/image-6b03ab87-f241-4883-beb7-265fc53e6cea.png)

---

### GET /Toys/filter

**Purpose:**  
Returns toys filtered by one or more materials and/or categories. If no filters are supplied, all toys are returned. Used by the toy browsing interface to narrow results.


**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `materials` | Query | `string[]` | ❌ Optional | One or more material values to filter by (e.g. `materials=Wood&materials=Plastic`) |
| `categories` | Query | `string[]` | ❌ Optional | One or more category values to filter by |

**Request Body:**  
None.

**Response — 200 OK**

Returns an array of `ToyDTO` objects matching the filter criteria.

| Field | Type | Description |
|---|---|---|
| `toyId` | `int` | Unique identifier |
| `name` | `string` | Toy name |
| `material` | `string` | Material |
| `category` | `string` | Category |
| `isAvailable` | `bool` | Availability flag |

**Success Response Example (200 OK)**

Request: `GET /Toys/filter?materials=Wood&categories=Construction`

```json
[
  {
    "toyId": 1,
    "name": "Wooden Blocks",
    "material": "Wood",
    "category": "Construction",
    "isAvailable": true
  }
]
```

**Error Response Example — No Matching Results (200 OK, empty array)**
```json
[]
```

![image.png](/.attachments/image-dfda9a3d-8cf7-4dfd-905d-ea66168e7aed.png)
---

### GET /Toys/{id}

**Purpose:**  
Retrieves a single toy by its primary key. Used when viewing or editing a toy's detail record.

**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `int` | ✅ Yes | The toy's ID |

**Request Body:**  
None.

**Response — 200 OK**

| Field | Type | Description |
|---|---|---|
| `toyId` | `int` | Unique identifier |
| `name` | `string` | Toy name |
| `material` | `string` | Material |
| `category` | `string` | Category |
| `isAvailable` | `bool` | Availability flag |

**Success Response Example (200 OK)**
```json
{
  "toyId": 1,
  "name": "Wooden Blocks",
  "material": "Wood",
  "category": "Construction",
  "isAvailable": true
}
```

**Error Response Example (404 Not Found)**
```json
{
  "status": 404,
  "title": "Not Found"
}
```

![image.png](/.attachments/image-7e060d30-d52a-4a1a-815e-72c230d9acb9.png)

---

### PUT /Toys/{id}

**Purpose:**  
Updates the details of an existing toy. The `toyId` in the URL and in the body must match. Used when editing a toy's name, material, category, or availability status.


**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `int` | ✅ Yes | Must match `toyId` in the request body |

**Request Body**

| Field | Type | Required | Constraints |
|---|---|---|---|
| `toyId` | `int` | ✅ Yes | Must match the route `id` |
| `name` | `string` | ✅ Yes | Non-empty |
| `material` | `string` | ✅ Yes | Non-empty |
| `category` | `string` | ✅ Yes | Non-empty |
| `isAvailable` | `bool` | ✅ Yes | Current availability |

**Request Body Example**
```json
{
  "toyId": 1,
  "name": "Wooden Blocks (Large Set)",
  "material": "Wood",
  "category": "Construction",
  "isAvailable": true
}
```

**Response — 204 No Content**  
Returned on success. No response body.

**Success Response Example (204 No Content)**
```
HTTP/1.1 204 No Content
```

**Error Response Examples**

ID mismatch (400):
```json
{
  "status": 400,
  "title": "Bad Request"
}
```

Toy not found (404):
```json
{
  "status": 404,
  "title": "Not Found"
}
```

> 📸 ![image.png](/.attachments/image-fcfd7b6a-a5b9-4969-b3fb-67ff3427a8de.png)

---

### DELETE /Toys/{id}

**Purpose:**  
Removes a toy from the system. A toy cannot be deleted if it is currently borrowed. Used by administrators when retiring a toy from the catalogue.


**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `int` | ✅ Yes | The toy's ID |

**Request Body:**  
None.

**Response — 204 No Content**  
Returned on success. No response body.

**Success Response Example (204 No Content)**
```
HTTP/1.1 204 No Content
```

**Error Response Examples**

Toy not found (404):
```json
{
  "status": 404,
  "title": "Not Found"
}
```

Toy currently borrowed (400):
```json
"Cannot delete this toy because it is currently borrowed."
```

![image.png](/.attachments/image-9bf709b9-1755-4a3d-952c-71cb0559a85e.png)
---

## Users

### GET /Users

**Purpose:**  
Returns a list of all users (application accounts) in the system. Used by administrative interfaces to manage user accounts.


**Request Parameters:**  
None.

**Request Body:**  
None.

**Response — 200 OK**

| Field | Type | Description |
|---|---|---|
| `userId` | `int` | Unique identifier |
| `oid` | `string` | Identity provider OID |
| `email` | `string` | User's email address |
| `role` | `string` | Role assigned to the user |

**Success Response Example (200 OK)**
```json
[
  {
    "userId": 1,
    "oid": "auth0|abc123",
    "email": "alice@school.edu",
    "role": "Student"
  },
  {
    "userId": 2,
    "oid": "auth0|xyz789",
    "email": "johnson@school.edu",
    "role": "Teacher"
  }
]
```

**Error Response Example (500 Internal Server Error)**
```json
{
  "status": 500,
  "title": "An error occurred while processing your request."
}
```

![image.png](/.attachments/image-50386e5c-3e84-47cc-91c5-cd8fc342d85f.png)
---

### GET /Users/{id}

**Purpose:**  
Retrieves a single user by their integer primary key. Used to load a user's account details.


**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `int` | ✅ Yes | The user's internal ID |

**Request Body:**  
None.

**Response — 200 OK**

| Field | Type | Description |
|---|---|---|
| `userId` | `int` | Unique identifier |
| `oid` | `string` | Identity provider OID |
| `email` | `string` | Email address |
| `role` | `string` | Assigned role |

**Success Response Example (200 OK)**
```json
{
  "userId": 1,
  "oid": "auth0|abc123",
  "email": "alice@school.edu",
  "role": "Student"
}
```

**Error Response Example (404 Not Found)**
```json
{
  "status": 404,
  "title": "Not Found"
}
```

![image.png](/.attachments/image-9eb40d12-4989-49da-af49-73255ec83b86.png)
---

### GET /Users/oid/{oid}

**Purpose:**  
Looks up a user by their external identity provider OID. Used during login flows to resolve the internal application user from an external identity token (e.g. Azure AD, Auth0).


**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `oid` | Route | `string` | ✅ Yes | The external OID string from the identity provider |

**Request Body:**  
None.

**Response — 200 OK**

| Field | Type | Description |
|---|---|---|
| `userId` | `int` | Unique identifier |
| `oid` | `string` | Identity provider OID |
| `email` | `string` | Email address |
| `role` | `string` | Assigned role |

**Success Response Example (200 OK)**
```json
{
  "userId": 1,
  "oid": "auth0|abc123",
  "email": "alice@school.edu",
  "role": "Student"
}
```

**Error Response Example (404 Not Found)**
```json
{
  "status": 404,
  "title": "Not Found"
}
```

![image.png](/.attachments/image-b32a0a7c-039a-4bd4-908b-f3ceda0ab797.png)
---

### POST /Users

**Purpose:**  
Creates a new user account. Called during user provisioning — typically immediately after a new identity is created in the external identity provider.

**Request Parameters:**  
None.

**Request Body**

| Field | Type | Required | Constraints |
|---|---|---|---|
| `oid` | `string` | ✅ Yes | Non-empty; must match identity provider OID |
| `email` | `string` | ✅ Yes | Valid email format |
| `role` | `string` | ✅ Yes | e.g. `"Student"`, `"Teacher"`, `"Admin"` |

**Request Body Example**
```json
{
  "oid": "auth0|newuser999",
  "email": "newuser@school.edu",
  "role": "Student"
}
```

**Response — 201 Created**

Returns the newly created user including the server-assigned `userId`. The `Location` response header points to `GET /Users/{id}`.

| Field | Type | Description |
|---|---|---|
| `userId` | `int` | The newly assigned ID |
| `oid` | `string` | Identity provider OID |
| `email` | `string` | Email address |
| `role` | `string` | Assigned role |

**Success Response Example (201 Created)**
```json
{
  "userId": 5,
  "oid": "auth0|newuser999",
  "email": "newuser@school.edu",
  "role": "Student"
}
```

**Error Response Example (400 Bad Request)**
```json
"User data is required"
```

> 📸![image.png](/.attachments/image-5038b716-85d8-4476-a1db-f1ffe710e56b.png)

---

## Notifications

### GET /Notifications

**Purpose:**  
Returns all notifications stored in the system. Used by administrators to review all sent and pending notifications.

**Request Parameters:**  
None.

**Request Body:**  
None.

**Response — 200 OK**

| Field | Type | Description |
|---|---|---|
| `notificationId` | `int` | Unique identifier |
| `recipientId` | `int` | ID of the intended recipient user |
| `message` | `string` | Notification text content |
| `dateSent` | `string (datetime)` | When the notification was created |
| `isRead` | `bool` | Whether the recipient has read it |

**Success Response Example (200 OK)**
```json
[
  {
    "notificationId": 1,
    "recipientId": 3,
    "message": "Your toy loan is due back tomorrow.",
    "dateSent": "2025-04-20T09:00:00",
    "isRead": false
  },
  {
    "notificationId": 2,
    "recipientId": 5,
    "message": "The toy you requested is now available.",
    "dateSent": "2025-04-21T11:30:00",
    "isRead": true
  }
]
```

**Error Response Example (500 Internal Server Error)**
```json
{
  "status": 500,
  "title": "An error occurred while processing your request."
}
```

![image.png](/.attachments/image-9c0ddd05-84d7-4184-82fa-f29840210cef.png)
---

### GET /Notifications/{id}

**Purpose:**  
Retrieves a single notification by its primary key. Used when displaying or processing an individual notification record.

**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `int` | ✅ Yes | The notification's ID |

**Request Body:**  
None.

**Response — 200 OK**

| Field | Type | Description |
|---|---|---|
| `notificationId` | `int` | Unique identifier |
| `recipientId` | `int` | ID of the recipient user |
| `message` | `string` | Notification text |
| `dateSent` | `string (datetime)` | Sent timestamp |
| `isRead` | `bool` | Read status |

**Success Response Example (200 OK)**
```json
{
  "notificationId": 1,
  "recipientId": 3,
  "message": "Your toy loan is due back tomorrow.",
  "dateSent": "2025-04-20T09:00:00",
  "isRead": false
}
```

**Error Response Example (404 Not Found)**
```json
{
  "status": 404,
  "title": "Not Found"
}
```

![image.png](/.attachments/image-8d1314cf-5d81-40c6-9151-1b5b8343ab39.png)
---

### GET /Notifications/ByRecipient/{id}

**Purpose:**  
Returns all notifications for a specific recipient user. Used to populate an in-app notification inbox for a logged-in user.

**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `recipientId` | Route | `int` | ✅ Yes | The recipient user's ID |

**Request Body:**  
None.

**Response — 200 OK**

Returns an array of notification objects for the given recipient. Returns an empty array if no notifications exist for that user.

| Field | Type | Description |
|---|---|---|
| `notificationId` | `int` | Unique identifier |
| `recipientId` | `int` | ID of the recipient |
| `message` | `string` | Notification text |
| `dateSent` | `string (datetime)` | Sent timestamp |
| `isRead` | `bool` | Read status |

**Success Response Example (200 OK)**
```json
[
  {
    "notificationId": 1,
    "recipientId": 3,
    "message": "Your toy loan is due back tomorrow.",
    "dateSent": "2025-04-20T09:00:00",
    "isRead": false
  }
]
```

**Error Response Example — No Notifications (200 OK, empty array)**
```json
[]
```
![image.png](/.attachments/image-688845bc-03a4-471a-9bed-fbbcbc3a8674.png)
---

### POST /Notifications

**Purpose:**  
Creates and stores a new notification for a recipient. The server automatically sets `dateSent` to the current server time. Used when the system needs to alert a user of an event.

**Request Parameters:**  
None.

**Request Body**

| Field | Type | Required | Constraints |
|---|---|---|---|
| `recipientId` | `int` | ✅ Yes | Must be a valid user ID |
| `message` | `string` | ✅ Yes | Non-empty string |
| `isRead` | `bool` | ❌ Optional | Defaults to `false` |

> **Note:** `dateSent` is set server-side and should not be included in the request body.

**Request Body Example**
```json
{
  "recipientId": 3,
  "message": "Reminder: your borrowed toy is due back on Friday.",
  "isRead": false
}
```

**Response — 201 Created**

Returns the created notification including server-assigned `notificationId` and `dateSent`. The `Location` header points to `GET /Notifications/{id}`.

| Field | Type | Description |
|---|---|---|
| `notificationId` | `int` | Newly assigned ID |
| `recipientId` | `int` | Recipient user ID |
| `message` | `string` | Notification message |
| `dateSent` | `string (datetime)` | Server-set creation timestamp |
| `isRead` | `bool` | Read status |

**Success Response Example (201 Created)**
```json
{
  "notificationId": 7,
  "recipientId": 3,
  "message": "Reminder: your borrowed toy is due back on Friday.",
  "dateSent": "2025-04-22T10:15:00",
  "isRead": false
}
```

**Error Response Example (400 Bad Request)**
```json
"Notification data is required"
```
![image.png](/.attachments/image-09c6d3e8-69d1-427c-ab37-c6ada24b5da0.png)
---

### PUT /Notifications/{id}

**Purpose:**  
Updates an existing notification — typically used to mark a notification as read, or to correct its content. The `notificationId` in the URL must match the one in the body.

**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `int` | ✅ Yes | Must match `notificationId` in the body |

**Request Body**

| Field | Type | Required | Constraints |
|---|---|---|---|
| `notificationId` | `int` | ✅ Yes | Must match route `id` |
| `recipientId` | `int` | ✅ Yes | Valid user ID |
| `message` | `string` | ✅ Yes | Non-empty |
| `dateSent` | `string (datetime)` | ✅ Yes | Original sent date |
| `isRead` | `bool` | ✅ Yes | Updated read status |

**Request Body Example**
```json
{
  "notificationId": 1,
  "recipientId": 3,
  "message": "Your toy loan is due back tomorrow.",
  "dateSent": "2025-04-20T09:00:00",
  "isRead": true
}
```

**Response — 204 No Content**  
Returned on success. No response body.

**Success Response Example (204 No Content)**
```
HTTP/1.1 204 No Content
```

**Error Response Examples**

Null body or ID mismatch (400):
```json
"Invalid notification data"
```

Notification not found (404):
```json
{
  "status": 404,
  "title": "Not Found"
}
```
![image.png](/.attachments/image-0885c2cb-48e8-4930-bb78-0cf63789578d.png)
---

### DELETE /Notifications/{id}

**Purpose:**  
Permanently removes a notification record. Used to clean up old or dismissed notifications.

**Request Parameters**

| Name | Location | Type | Required | Description |
|---|---|---|---|---|
| `id` | Route | `int` | ✅ Yes | The notification's ID |

**Request Body:**  
None.

**Response — 204 No Content**  
Returned on success. No response body.

**Success Response Example (204 No Content)**
```
HTTP/1.1 204 No Content
```

**Error Response Example (404 Not Found)**
```json
{
  "status": 404,
  "title": "Not Found"
}
```
![image.png](/.attachments/image-0dca6493-1a29-4fe7-899c-83ad6be5d7a0.png)