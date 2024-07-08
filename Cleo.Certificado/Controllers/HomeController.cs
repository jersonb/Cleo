using System.Diagnostics;
using System.IO.Compression;
using System.Net.Mime;
using System.Text;
using Cleo.Certificado.Models;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;

namespace Cleo.Certificado.Controllers;

public class HomeController : Controller
{
    public IActionResult Index(ConfigureModel? configure = null)
    {
        configure ??= new ConfigureModel();

        ViewBag.FontNames = ConfigureModel.FontNames;

        return View(nameof(Index), configure);
    }

    [HttpPost]
    public IActionResult Post(ConfigureModel configure)
    {
        var isInvalid = (string.IsNullOrWhiteSpace(configure.Name)
            && string.IsNullOrWhiteSpace(configure.Names))
            || configure.File == default;

        if (isInvalid)
            return Index(configure);

        if (string.IsNullOrEmpty(configure.Names))
            return GeneratePreview(configure);

        return GenerateCertificatesZip(configure);
    }

    private IActionResult GenerateCertificatesZip(ConfigureModel configure)
    {
        var names = configure.Names
            .Split("\n")
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n.Trim())
            .OrderBy(x => x);

        if (!names.Any())
            return Index(configure);

        var compressedFileStream = new MemoryStream();
        compressedFileStream.Seek(0, SeekOrigin.Begin);

        using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, true))
        {
            foreach (var name in names)
            {
                var zipEntry = zipArchive.CreateEntry($"{name}.pdf", CompressionLevel.Optimal);

                var pdf = GetPdf(configure, name).GeneratePdf();

                using var zipEntryStream = zipEntry.Open();

                zipEntryStream.Write(pdf);
            }
        }
        return File(compressedFileStream.ToArray(), MediaTypeNames.Application.Zip, $"Certificados_{DateTime.Now:yyyy-MM-dd}.zip", true);
    }

    private IActionResult GeneratePreview(ConfigureModel configure)
    {
        var pdf = GetPdf(configure, configure.Name);
        var preview = pdf.GenerateImages().First();

        configure.Data = preview;
        return Index(configure);
    }

    private static Document GetPdf(ConfigureModel configure, string name)
    {
        if (configure.Captalize)
            name = Captalize(name);

        if (configure.ToUpper)
            name = name.ToUpper();

        var image = configure.File!.OpenReadStream();

        var document = Document.Create(container =>
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Background().Image(image);
                    page.Content().Column(text =>
                    {
                        var item = text.Item()
                        .TranslateY(configure.PositionY)
                        .AlignCenter()
                        .Text(name)
                        .FontFamily(configure.FontFamily)
                        .FontSize(configure.FontSize)
                        .FontColor(configure.FontColor);

                        if (configure.Itailc)
                            item.Italic();

                        if (configure.Bold)
                            item.Bold();
                    });
                }));

        return document;
    }

    private static string Captalize(string name)
    {
        name = name.ToLower().Trim();

        var splited = name.Split(" ");

        var newString = new StringBuilder();

        var nonCaptalize = new string[] { "de", "da", "e", "dos", "das" };

        foreach (var word in splited)
        {
            if (string.IsNullOrWhiteSpace(word)) continue;

            if (nonCaptalize.Contains(word))
            {
                newString.Append(word + " ");
                continue;
            }
            newString.Append(char.ToUpper(word[0]) + word[1..] + " ");
        }
        return newString.ToString();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}