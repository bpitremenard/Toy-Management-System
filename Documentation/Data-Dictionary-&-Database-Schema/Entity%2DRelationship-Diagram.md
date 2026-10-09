#Tables included:
- User
- Student
- Teacher
- Toy
- ToyLoan
- Notification

#Key Relationships (high-level):
- User → Student (1:1)
- User → Teacher (1:1)
- Teacher → Toy (1:M)
- Student → ToyLoan (1:M)
- Toy → ToyLoan (1:M)
- User → Notification (1:M)
![image.png](/.attachments/image-8df01ef8-bbda-4b68-9cce-5f9e5aca239b.png)