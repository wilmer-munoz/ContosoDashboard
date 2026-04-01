# Tasks: Document Upload and Management

**Input**: Design documents from `/specs/002-document-upload-management/`
**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **Single project**: `ContosoDashboard/` at repository root
- Paths shown below follow the project structure from plan.md

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [ ] T001 Add xUnit testing package to ContosoDashboard.csproj
- [ ] T002 Create uploads directory outside wwwroot for secure file storage
- [ ] T003 Configure file upload size limits in Program.cs

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T004 [P] Create Document.cs model in Data/Models/Document.cs
- [ ] T005 [P] Create DocumentShare.cs model in Data/Models/DocumentShare.cs
- [ ] T006 Update ApplicationDbContext.cs to include Document and DocumentShare DbSets
- [ ] T007 Create EF Core migration for document tables
- [ ] T008 [P] Implement IFileStorageService.cs in Services/IFileStorageService.cs
- [ ] T009 [P] Implement FileStorageService.cs in Services/FileStorageService.cs
- [ ] T010 [P] Implement IDocumentService.cs in Services/IDocumentService.cs
- [ ] T011 [P] Implement DocumentService.cs in Services/DocumentService.cs
- [ ] T012 Register services in Program.cs dependency injection

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Upload Documents (Priority: P1) 🎯 MVP

**Goal**: Enable employees to upload documents to the dashboard with metadata capture and validation

**Independent Test**: Can upload a single document and verify it appears in the user's document list

### Implementation for User Story 1

- [ ] T013 [P] [US1] Create FileUpload.razor component in Shared/Components/FileUpload.razor
- [ ] T014 [P] [US1] Create DocumentUpload.razor page in Pages/DocumentUpload.razor
- [ ] T015 [US1] Implement upload logic in DocumentService.cs with validation and file storage
- [ ] T016 [US1] Add file type and size validation in upload workflow
- [ ] T017 [US1] Add progress indicator to upload component
- [ ] T018 [US1] Add success/error message handling for uploads

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Browse and Search Documents (Priority: P2)

**Goal**: Allow employees to browse and search their documents with filtering options

**Independent Test**: Can view list of uploaded documents and search by title

### Implementation for User Story 2

- [ ] T019 [P] [US2] Create Documents.razor page in Pages/Documents.razor
- [ ] T020 [P] [US2] Create DocumentList.razor component in Shared/Components/DocumentList.razor
- [ ] T021 [US2] Implement document listing in DocumentService.cs with pagination
- [ ] T022 [US2] Add search by title, description, tags in DocumentService.cs
- [ ] T023 [US2] Add filtering by category, project, date range in Documents.razor
- [ ] T024 [US2] Add sorting by title, upload date, category, file size
- [ ] T025 [US2] Implement document download functionality

**Checkpoint**: At this point, User Story 2 should be fully functional and testable independently

---

## Phase 5: User Story 3 - Manage Document Access (Priority: P3)

**Goal**: Enable project managers and owners to manage document sharing and access permissions

**Independent Test**: Can upload document to project and team member can view it

### Implementation for User Story 3

- [ ] T026 [P] [US3] Create DocumentDetails.razor page in Pages/DocumentDetails.razor
- [ ] T027 [US3] Implement document sharing logic in DocumentService.cs
- [ ] T028 [US3] Add role-based access control for document operations
- [ ] T029 [US3] Implement sharing UI in DocumentDetails.razor
- [ ] T030 [US3] Add notification sending for shared documents
- [ ] T031 [US3] Implement document deletion with permission checks

**Checkpoint**: At this point, User Story 3 should be fully functional and testable independently

---

## Final Phase: Polish & Cross-Cutting Concerns

**Purpose**: Integration, optimization, and quality improvements

- [ ] T032 Add Documents navigation link to NavMenu.razor
- [ ] T033 Integrate Recent Documents widget to Index.razor
- [ ] T034 Add audit logging for document operations
- [ ] T035 Implement preview for common file types (PDF, images)
- [ ] T036 Add metadata editing functionality
- [ ] T037 Performance optimization for search and listing
- [ ] T038 Add comprehensive error handling and user feedback

---

## Dependencies

**User Story Completion Order**:
1. US1 (Upload) → Foundation for all document operations
2. US2 (Browse/Search) → Depends on upload functionality
3. US3 (Access Management) → Depends on basic document management

**Parallel Execution Examples**:
- **Per User Story**: Implement all tasks within US1 in parallel, then move to US2
- **Foundation Phase**: Tasks T004-T012 can run in parallel
- **Cross-Story**: Model creation (T004, T005) can be done before service implementation

**Implementation Strategy**: MVP First - Complete US1 for basic upload capability, then incrementally add US2 and US3. Each user story delivers independent value.

---

## Validation

**Task Completeness**: Each user story has all needed tasks (models, services, UI, validation)
**Independent Testability**: Each story can be tested without others being complete
**File Path Accuracy**: All tasks include exact file paths for implementation