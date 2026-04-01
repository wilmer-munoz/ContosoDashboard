# Data Model: Document Upload and Management

## Entities

### Document
- **Id**: int (Primary Key)
- **Title**: string (Required, max 255)
- **Description**: string (Optional, max 1000)
- **Category**: string (Required, max 100)
- **ProjectId**: int? (Foreign Key to Project)
- **Tags**: string (Optional, comma-separated, max 500)
- **UploadDate**: DateTime (Required)
- **UploaderId**: string (Foreign Key to User, Required)
- **FileSize**: long (Required)
- **FileType**: string (Required, MIME type, max 255)
- **FilePath**: string (Required, unique path)
- **IsDeleted**: bool (Default false)

**Relationships**:
- Belongs to User (Uploader)
- Belongs to Project (Optional)
- Has many DocumentShares

**Validation Rules**:
- Title: Required, 1-255 chars
- FileSize: <= 25MB
- FileType: In allowed list (PDF, Office, text, images)
- FilePath: Unique

### DocumentShare
- **Id**: int (Primary Key)
- **DocumentId**: int (Foreign Key to Document)
- **SharedWithUserId**: string (Foreign Key to User)
- **SharedByUserId**: string (Foreign Key to User)
- **SharedDate**: DateTime (Required)
- **Permissions**: string (e.g., "read", "download")

**Relationships**:
- Belongs to Document
- SharedWith User
- SharedBy User

**Validation Rules**:
- DocumentId: Exists and accessible
- SharedWithUserId: Valid user
- Permissions: In allowed values

## State Transitions

### Document Lifecycle
1. **Created**: Uploaded, metadata saved, file stored
2. **Active**: Accessible by owner/project members
3. **Shared**: Additional users have access
4. **Deleted**: Soft delete, file remains but not accessible