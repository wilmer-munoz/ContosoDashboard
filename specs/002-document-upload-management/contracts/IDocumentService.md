# IDocumentService Contract

## Interface Definition

```csharp
public interface IDocumentService
{
    Task<Document> UploadDocumentAsync(IFormFile file, DocumentMetadata metadata, string userId);
    Task<IEnumerable<Document>> GetUserDocumentsAsync(string userId);
    Task<IEnumerable<Document>> GetProjectDocumentsAsync(int projectId, string userId);
    Task<Document> GetDocumentAsync(int documentId, string userId);
    Task UpdateDocumentMetadataAsync(int documentId, DocumentMetadata metadata, string userId);
    Task DeleteDocumentAsync(int documentId, string userId);
    Task ShareDocumentAsync(int documentId, string shareWithUserId, string permissions, string sharedByUserId);
    Task<IEnumerable<Document>> SearchDocumentsAsync(string searchTerm, string userId);
}
```

## Contract Details

- **UploadDocumentAsync**: Validates file, stores it, saves metadata. Returns created Document.
- **GetUserDocumentsAsync**: Returns documents uploaded by user.
- **GetProjectDocumentsAsync**: Returns project documents if user has access.
- **GetDocumentAsync**: Returns document if user has access.
- **UpdateDocumentMetadataAsync**: Updates metadata if user is owner.
- **DeleteDocumentAsync**: Soft deletes if user has permission.
- **ShareDocumentAsync**: Creates share record.
- **SearchDocumentsAsync**: Searches by title, description, tags.

## Authorization

- Owner: Full access
- Project Manager: Delete project docs
- Shared users: Read/download based on permissions