namespace Conversor.Services
{
    public interface IFileConverterService
    {
        Task ConvertFileToPdfAAsync(IFormFile inputFile, string outputDirectory, string originalFileName);
    }
}