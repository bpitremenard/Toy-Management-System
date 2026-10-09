## Relationships Explained

The data model is centered around the **User** entity, which acts as the base for both Students and Teachers. Each user is associated with exactly one role-specific record (Student or Teacher), enforced through a one-to-one relationship and automatically handled by a **database trigger** (check user table for detail).

A **Student** can borrow multiple toys through the **ToyLoan** table, creating a one-to-many relationship between Student and ToyLoan. Similarly, each **Toy** can appear in multiple loan records over time. This structure resolves the many-to-many relationship between Students and Toys by using ToyLoan as a junction table that also stores important business data such as borrow dates, due dates, and return dates.

The **Toy** table optionally links to a **Teacher** through the ManagedBy field, representing which teacher is responsible for managing a toy. This relationship uses a "SET NULL" delete rule, meaning that if a teacher is removed, the toy remains in the system but is no longer assigned to a manager. This prevents accidental data loss of inventory.

The **Notification** table is linked to the User table in a one-to-many relationship, allowing each user to receive multiple notifications. A cascade delete rule ensures that when a user is deleted, all associated notifications are also removed, maintaining data consistency.

Cascade delete rules are used strategically across the system. For example, deleting a Student or Toy will automatically remove related ToyLoan records, ensuring there are no orphaned records. These decisions reflect the business logic that loan records cannot exist without both a valid student and a valid toy.