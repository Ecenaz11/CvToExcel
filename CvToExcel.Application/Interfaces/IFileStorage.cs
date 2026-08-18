namespace CvToExcel.Application.Interfaces;
public interface IFileStorage
{
    Task<(string StoredFileName, string FilePath)> SaveAsync(
        Stream fileStream,
        string OriginalFileName,
        CancellationToken cancellationToken = default);
}