# Toy Management System User Manual

**K50 Team 2**

---

# Table of Contents
[[_TOC_]]

---

# Introduction

The Toy Management System is a website used to manage toys that can be borrowed and returned. It is designed to make it easy to keep track of which toys are currently available and which are not. Users can also see which toys students have borrowed and their current status.

This system is mainly used by:

* **Students**, who borrow and return toys
* **Teachers**, who manage the toys and monitor activity

### Problem Solved

Before this system, everything was tracked on paper. This made it:

* Time-consuming
* Easy to lose information
* Prone to mistakes

This application replaces that process with a digital system, making it easier to:

* Organize data
* Update records
* Access information quickly

---

# Getting Started

## Account Creation

To set up your account:

1. Open the application:
   [https://team2app-csprodwww.cegep-heritage.qc.ca/Home/Login](https://team2app-csprodwww.cegep-heritage.qc.ca/Home/Login)
2. Click **Sign In With Student Account**
![image.png](/.attachments/image-0ac09ba2-b473-4648-8691-858547e4788b.png)
3. Click **Create one!**
![image.png](/.attachments/image-f8936e5b-a264-48ce-9d89-0eacd278555e.png)
4. Sign up using your school Microsoft account
5. Accept permission requests
![image.png](/.attachments/image-73365e57-ca37-4ac2-9b86-f6ca78c7dad8.png)
6. Enter your personal details
![image.png](/.attachments/image-5a099e6a-170b-469b-b7b7-220e09c1ee90.png)
7. Contact a team member to activate your account


---

## Log In

To start using the system:

1. Open the application
2. Enter your school account credentials
3. Complete authentication using the authenticator app
4. Choose whether to stay signed in
5. You will be redirected to your dashboard
![image.png](/.attachments/image-8cf3fb26-cfad-485b-8a08-963f9a66a114.png)

---

## What to Do if Login Fails

If you are unable to log in:

* Double-check your username and password
* Make sure there are no typing errors
* Refresh the page
* Contact support if the issue continues

---

## User Roles

### Student

Students can:

* View available toys
* Borrow toys
* Return toys
* Search and filter toys
* View toy details

Students cannot:

* Add or remove toys
* Edit toy information

---

### Teacher

Teachers can:

* Add new toys
* Edit toy details
* Delete toys
* View student borrowing history
* View and export reports
* Backup and restore the database
* Search and filter toys

Teachers have full control over the system.

---

# Student Features

## Dashboard Overview
https://team2app-csprodwww.cegep-heritage.qc.ca/Home/Login
After logging in, students will see the dashboard. This is where they can access:

* Available toys
* Borrow toys
* Return toys

The dashboard is designed to be simple and easy to use.
![image.png](/.attachments/image-a7ba5d39-8ae5-42a1-baa4-5c845858d646.png)
---

## Viewing Available Toys

### Steps:

1. Go to the Main page
2. View the list of toys
3. Each toy shows its current status


### Expected Result:

* All toys are displayed
* Available toys are clearly shown
* Borrowed toys are visually different

---

## Searching and Filtering Toys

### Steps:

1. Use the search bar or filters
2. Enter keywords or select a category


### Expected Result:

* Only matching toys are displayed

---

## Borrowing a Toy

### Steps:

1. Go to the Main page
2. Select an available toy
3. Click **Reserve**
4. Confirm the action

![image.png](/.attachments/image-e523af5c-30e2-4b6a-aa1c-b5df235c2d1d.png)
### Expected Result:

* The toy is marked as **Reserved**
* It appears in your borrowed list
* Other users cannot select it

---

## Returning a Toy

### Steps:

1. Go to the Main page
2. Find the toy you want to return
3. Click **Return**


### Expected Result:

* The toy becomes available again
* It is removed from your borrowed list
* Other users can now borrow it

---

# Teacher Features

## Dashboard Overview
https://team2app-csprodwww.cegep-heritage.qc.ca/Teacher/Home/Login

The teacher dashboard provides:

* All toys in the system
* System activity
* Quick access to management features

---

## Managing Toys

Teachers are responsible for:

* Adding toys
* Updating toys
* Removing toys

---

## Adding a Toy

### Steps:

1. Go to the Main page
2. Click **Add Toy**
3. Fill in the toy details
4. Click **Add Toy**

![image.png](/.attachments/image-bfd80b12-c115-4652-be5a-3ec85bf790a8.png)
### Expected Result:

* The toy appears in the list
* It is available for students

---

## Editing a Toy

### Steps:

1. Go to the Main page
2. Click **Edit** on a toy
3. Update the information
4. Click **Save Changes**
![image.png](/.attachments/image-65e8126d-9c0c-4793-a830-1201275db705.png)
### Expected Result:

* Toy information is updated

---

## Deleting a Toy

### Steps:

1. Go to the Main page
2. Click **Delete**
3. Confirm the action
![image.png](/.attachments/image-cb9c23cb-c2da-4a04-b79b-b3499761c024.png)

### Expected Result:

* The toy is removed from the system

---

## Searching and Filtering Toys

### Steps:

1. Use search or filter options
2. Enter keywords or select category

### Expected Result:

* Filtered results are displayed

---

## View Student Reports

### Steps:

1. Go to the Reports page
![image.png](/.attachments/image-031234e7-ddad-4f31-8878-3d2c6ae804b2.png)
2. Select a student
![image.png](/.attachments/image-8ffe3651-7f32-47bf-9942-9b147a2095fd.png)
3. View borrowing history
4. Click **Export PDF**
![image.png](/.attachments/image-84c4bc2b-d6cd-481b-a60a-8f5876e2a965.png)


### Expected Result:

* Borrow history is displayed
* PDF report can be downloaded

---

## Database Backup and Restore

### Backup Steps:

1. Navigate to Backup page
2. Click **Create Backup**
![image.png](/.attachments/image-4b808d7a-ad01-4a8e-a376-7643b3e48c11.png)

### Restore Steps:

1. Click **Select a Backup**
2. Choose a backup version
3. Click **Restore Selected Backup**
![image.png](/.attachments/image-f688ccaa-faaf-44d6-815d-e7855682697a.png)


### Expected Result:

* Backup is created
* Data can be restored

---

# Error Messages

| Message                          | Meaning                      | What to Do                         |
| -------------------------------- | ---------------------------- | ---------------------------------- |
| “" is required                    | Required information missing | Fill all fields                    |
| Unable to load reports right now | Data not loading             | Try again later or contact support |
| Unable to save toy to API        | System error                 | Contact support                    |


---


# Frequently Asked Questions

### How do I borrow a toy?

Go to the home page and click **Borrow** on an available toy.

### Why can’t I borrow a toy?

It is already borrowed by someone else.

### How do I return a toy?

Go to the home page and click **Return** on the toy you would like to return.

### Can students add toys?

No, only teachers can manage toys.

### What happens if I return a toy late?

It may be marked red in the system.

### Why don’t I see any toys in the system?

You may not have access yet, or there may be no toys added. Contact a teacher or administrator if the issue continues.

### Why does my account not work after signing up?

Your account may not be activated yet. You must contact a team member to be granted access.

### What does “Reserved” mean?

It means the toy has already been taken by another student and cannot be selected.

### Can I borrow more than one toy at a time?

This depends on system rules set by the teacher. If allowed, you can borrow multiple toys.

### Why is a toy highlighted or shown differently?

Different colors or labels are used to show whether a toy is available, reserved, or returned.

### How do I search for a specific toy?

Use the search bar or filters on the main page to find toys by name or category.

### What happens if I accidentally click Reserve?

The toy will be reserved under your account. You can return it immediately if it was a mistake.

### Why can’t I see teacher features?

You are logged in as a student. Only teachers have access to management features.

### How do teachers generate reports?

Go to the **Reports** page, select a student, and export the report as a PDF.

### What should I do if something doesn’t load?

Try refreshing the page. If the issue continues, contact support.

---

# Contact and Support

If you need help, contact one of the people listed below.

## First Point of Contact

- Rodrigo Nogueira de Melo — RNogueiradeMelo@cegep-heritage.qc.ca

## Second Point of Contact

- Gil Audrey Nangue Tsotsop — GTsotsop@cegep-heritage.qc.ca

## Other Points of Contact

- Joshua Mina-Loaiza — joshua282006@gmail.com
- Jacob Luu — jacob.luu2006@gmail.com
- Matt Nolan — mattnolan688@gmail.com
- Manuela Nsen Yimga — m.nyimga@gmail.com
- Daniel Desjardins — ddesjardins292@gmail.com
- Barbara-Ann Pitre-Menard — bpitremenard@gmail.com

## When Asking for Help

Please include:

- A description of the issue
- A screenshot, if possible

This will help resolve the problem faster.


