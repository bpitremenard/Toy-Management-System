## Data Dictionary

---

### **Table: User**

| Column Name | Data Type | Nullable (Yes/No) | Description | Constraints / Business Rules |
|------------|----------|-------------------|------------|------------------------------|
| UserId | int | No | Unique identifier for each user | Primary Key, Identity |
| UserType | varchar(20) | No | Defines the role of the user (Student or Teacher) | CHECK constraint (‘Student’, ‘Teacher’) |
| Oid | varchar(255) | Yes | External authentication identifier (e.g., Azure AD ID) | |

---

### **Table: Student**

| Column Name | Data Type | Nullable (Yes/No) | Description | Constraints / Business Rules |
|------------|----------|-------------------|------------|------------------------------|
| StudentId | int | No | Unique identifier for each student | Primary Key, Identity |
| UserId | int | No | Links the student to a user account | Foreign Key → User(UserId), UNIQUE |
| Placement | varchar(255) | Yes | Placement or program information for the student | |
| Year | int | Yes | Academic year of the student | |

---

### **Table: Teacher**

| Column Name | Data Type | Nullable (Yes/No) | Description | Constraints / Business Rules |
|------------|----------|-------------------|------------|------------------------------|
| TeacherId | int | No | Unique identifier for each teacher | Primary Key, Identity |
| UserId | int | No | Links the teacher to a user account | Foreign Key → User(UserId), UNIQUE |

---

### **Table: Toy**

| Column Name | Data Type | Nullable (Yes/No) | Description | Constraints / Business Rules |
|------------|----------|-------------------|------------|------------------------------|
| ToyId | int | No | Unique identifier for each toy | Primary Key, Identity |
| Name | varchar(255) | No | Name of the toy | |
| Description | text | Yes | Detailed description of the toy | |
| Category | varchar(100) | Yes | Category classification (e.g., Cognitive, Physical) | |
| Material | varchar(100) | Yes | Material the toy is made from | |
| LocationCode | varchar(10) | Yes | Storage location identifier | |
| ImagePath | varchar(255) | Yes | File path to the toy image | |
| IsAvailable | bit | No | Indicates if the toy is currently available for borrowing | Default = 1 |
| IsActive | bit | No | Indicates if the toy is active in the system | Default = 1 |
| ManagedBy | int | Yes | Teacher responsible for managing the toy | Foreign Key → Teacher(TeacherId), ON DELETE SET NULL |
| AdditionalInformation | nvarchar(500) | Yes | Additional notes or details about the toy | |

---

### **Table: ToyLoan**

| Column Name | Data Type | Nullable (Yes/No) | Description | Constraints / Business Rules |
|------------|----------|-------------------|------------|------------------------------|
| LoanId | int | No | Unique identifier for each loan record | Primary Key, Identity |
| ToyId | int | No | The toy being borrowed | Foreign Key → Toy(ToyId), ON DELETE CASCADE |
| StudentId | int | No | The student borrowing the toy | Foreign Key → Student(StudentId), ON DELETE CASCADE |
| BorrowDate | date | No | Date when the toy was borrowed | |
| DueDate | date | No | Date when the toy should be returned | |
| ReturnDate | date | Yes | Actual return date of the toy | |

---

### **Table: Notification**

| Column Name | Data Type | Nullable (Yes/No) | Description | Constraints / Business Rules |
|------------|----------|-------------------|------------|------------------------------|
| NotificationId | int | No | Unique identifier for each notification | Primary Key, Identity |
| RecipientId | int | No | User receiving the notification | Foreign Key → User(UserId), ON DELETE CASCADE |
| Message | varchar(500) | No | Content of the notification message | |
| DateSent | datetime | Yes | Date and time the notification was sent | Default = GETDATE() |
| Type | varchar(50) | Yes | Type/category of notification | |

---