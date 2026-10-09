# Part H — Testing Documentation

## Overview
This document describes the final state of the test suite for the Toy Management System (TMS). It explains what is tested, how to run the tests, how to interpret results, and how future developers can extend the test coverage.

---

## Test Suite Summary


|File |Component Test  |Test Type  |Status  |
|--|--|--|--|
|Controller  |Audit Log  |Unit  |Pass  |
|Controller  |Student  |Unit  |Pass  |
|Controller  |Teacher  |Unit  |Pass  |
|Controller  |Toy CRUD / Reserve / Return |Unit  |Pass  |
|Controller  |Upload  |Unit  |Pass  |
|Controller  |Toy Search  |Unit  |Pass  |
|Controller  |Authorization  |Unit  |Pass  |
|Controller  |Home Access  |Unit  |Pass  |
|Controller  |Download |Unit  |Pass  |
|Model  |Audit Log Entry  |Unit  |Pass  |
|Model  |Notification  |Unit  |Pass  |
|Model  |Student  |Unit  |Pass  |
|Model  |Teacher  |Unit  |Pass  |
|Model  |Toy  |Unit  |Pass  |
|Service  |Audit Helper  |Unit  |Pass  |
|Service  |Overdue Toy  |Unit  |Pass  |
|Service  |Toy Repo Delete  |Unit  |Pass  |
|E2E  |Accessing Navigation Links  |E2E  |Pass  |
|E2E  |Authentication  |E2E  |Pass  |
|E2E  |Backup and Restore Data  |E2E  |Pass  |
|E2E  |Borrow History  |E2E  |Pass  |
|E2E  |Global Setup  |E2E  |Pass  |
|E2E  |Reports Download  |E2E  |Pass  |
|E2E  |Search Filter Return Toy  |E2E  |Pass  |
|E2E  |Teacher Toy  |E2E  |Pass  |
|E2E  |Theme and Filter  |E2E  |Pass  |





> Status reflects the final state of the repository at project closure.

---
##Known Gaps and Limitations

While the test suite covers core functionality, some areas are not fully tested:

- UI styling and responsiveness are not tested
- No performance or load testing
- No time out tests

These gaps exist due to time constraints and prioritization of core features such as toy management and reservations. Future work should expand coverage in these areas.