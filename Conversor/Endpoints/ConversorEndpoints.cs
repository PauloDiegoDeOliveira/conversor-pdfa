using Conversor.Services;
using System.IO.Compression;

namespace Conversor.Endpoints
{
    public static class ConversorEndpoints
    {
        public static void RegisterConversorEndpoints(this WebApplication app)
        {
            app.MapPost("/conversor", async (IFormFileCollection arquivos, IFileConverterService converterService, ILogger<Program> logger) =>
            {
                if (arquivos == null || arquivos.Count == 0)
                {
                    return Results.BadRequest("Nenhum arquivo foi enviado.");
                }

                string tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(tempDirectory);

                try
                {
                    foreach (IFormFile arquivo in arquivos)
                    {
                        await converterService.ConvertFileToPdfAAsync(arquivo, tempDirectory, arquivo.FileName);
                    }

                    string zipPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
                    ZipFile.CreateFromDirectory(tempDirectory, zipPath);
                    Directory.Delete(tempDirectory, true);

                    FileStream? zipStream = new(zipPath, FileMode.Open, FileAccess.Read);
                    return Results.File(zipStream, "application/zip", "PDFs_Convertidos.zip");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Erro ao converter para PDF/A");
                    Directory.Delete(tempDirectory, true);
                    return Results.Problem("Ocorreu um erro durante o processo de conversão.");
                }
            })
            .WithName("ConvertePdfParaPdfA")
            .Accepts<IFormFileCollection>("multipart/form-data")
            .DisableAntiforgery();
        }
    }
}