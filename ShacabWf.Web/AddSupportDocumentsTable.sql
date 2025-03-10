CREATE TABLE SupportDocuments (
    Id INT PRIMARY KEY IDENTITY(1,1),
    FileName NVARCHAR(200) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    Description NVARCHAR(500) NULL,
    ContentType NVARCHAR(MAX) NOT NULL,
    FileSize BIGINT NOT NULL,
    UploadedAt DATETIME2 NOT NULL,
    UploadedById INT NOT NULL,
    CONSTRAINT FK_SupportDocuments_Users_UploadedById FOREIGN KEY (UploadedById) REFERENCES Users(Id) ON DELETE NO ACTION
);

CREATE INDEX IX_SupportDocuments_UploadedById ON SupportDocuments(UploadedById); 