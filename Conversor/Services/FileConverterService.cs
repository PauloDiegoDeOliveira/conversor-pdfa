using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Pdfa;

namespace Conversor.Services
{
    public class FileConverterService(IWebHostEnvironment env) : IFileConverterService
    {
        private readonly string _webRootPath = env.WebRootPath;

        public async Task ConvertFileToPdfAAsync(IFormFile inputFile, string outputDirectory, string originalFileName)
        {
            string outputFile = Path.Combine(outputDirectory, Path.ChangeExtension(originalFileName, ".pdfa.pdf"));

            string? tempInputFile = Path.GetTempFileName();
            await using (FileStream? stream = new(tempInputFile, FileMode.Create))
            {
                await inputFile.CopyToAsync(stream);
            }

            if (!Path.GetExtension(inputFile.FileName).Equals(".pdf", StringComparison.CurrentCultureIgnoreCase))
            {
                string pdfFromImage = Path.Combine(outputDirectory, Path.ChangeExtension(originalFileName, ".pdf"));
                ConvertImageToPdf(tempInputFile, pdfFromImage);
                tempInputFile = pdfFromImage;
            }

            ConvertToPdfA(tempInputFile, outputFile);

            if (tempInputFile != outputFile)
            {
                File.Delete(tempInputFile);
            }
        }

        private void ConvertImageToPdf(string imagePath, string pdfPath)
        {
            using PdfWriter? writer = new(pdfPath);
            using PdfDocument? pdf = new(writer);
            Document? document = new(pdf);
            ImageData? imageData = ImageDataFactory.Create(imagePath);
            Image? image = new(imageData);
            document.Add(image);
            document.Close();
        }

        private void ConvertToPdfA(string pdfPath, string pdfAPath)
        {
            string iccProfilePath = Path.Combine(_webRootPath, "Perfil_ICC", "sRGB2014.icc");
            using FileStream? iccProfileStream = new(iccProfilePath, FileMode.Open, FileAccess.Read);
            using PdfReader? reader = new(pdfPath);
            using PdfWriter? writer = new(pdfAPath);

            PdfADocument? pdfADocument = new(writer, PdfAConformanceLevel.PDF_A_2B, new PdfOutputIntent("Custom", "", "http://www.color.org", "sRGB IEC61966-2.1", iccProfileStream));

            using PdfDocument? pdfDocument = new(reader);
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                PdfPage? originalPage = pdfDocument.GetPage(i);
                pdfADocument.AddPage(originalPage.CopyTo(pdfADocument));
            }

            pdfADocument.Close();
        }
    }
}