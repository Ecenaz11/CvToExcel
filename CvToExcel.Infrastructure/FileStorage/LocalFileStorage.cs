using CvToExcel.Application.Interfaces;

namespace CvToExcel.Infrastructure.FileStorage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _storageRootPath;

    public LocalFileStorage()
    {
        _storageRootPath = Path.Combine(Directory.GetCurrentDirectory(),
        "storage", "cvs");
        Directory.CreateDirectory(_storageRootPath);
    }
    public async Task<(string StoredFileName, string FilePath)> SaveAsync(
        Stream fileStream, 
        string originalFileName, 
        CancellationToken cancellationToken = default)
    {
       var extension = Path.GetExtension(originalFileName);
       var storedFileName = $"{Guid.NewGuid()}{extension}";
       var filePath = Path.Combine(_storageRootPath, storedFileName);

       using var fileOutputStream = new FileStream(filePath, FileMode.Create);
      
       await fileStream.CopyToAsync(fileOutputStream, cancellationToken);

       return (storedFileName, filePath);
    }
}