using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.JSInterop;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.IO.Compression;
using System.Text;

namespace Cleo.Wasm.Components.Pages;

public class ConfigureCertificate
{
    public IBrowserFile BackgroundImage { get; internal set; } = default!;
    public bool Captalize { get; internal set; }
    public int PositionY { get; internal set; } = 230;
    public int FontSize { get; internal set; } = 25;
    public bool Itailc { get; internal set; }
    public bool ToUpper { get; internal set; }
    public bool Bold { get; internal set; }
    public string Name { get; internal set; } = string.Empty!;
    public string FontFamily { get; internal set; } = Fonts.TimesNewRoman!;
    public string FontColor { get; internal set; } = Colors.Black;
    public string Names { get; internal set; } = string.Empty!;
    internal byte[] Buffer { get; set; } = [];
}

public partial class Home
{
    private ConfigureCertificate Configure { get; set; } = new();

    private static IEnumerable<SelectListItem> FontNames =>
    [
        new (Fonts.Arial,Fonts.Arial),
            new (Fonts.Calibri,Fonts.Calibri),
            new (Fonts.LucidaConsole,Fonts.LucidaConsole),
            new (Fonts.TimesNewRoman,Fonts.TimesNewRoman),
            new (Fonts.Verdana,Fonts.Verdana),
            new ("Roboto","Roboto"),
            new ("DejaVu","DejaVu Sans"),

    ];

    private bool DisableGenerateTestButton => Configure == null || Configure.BackgroundImage == default || string.IsNullOrEmpty(Configure.Name);
    private bool DisableGenerateButton => string.IsNullOrEmpty(Configure.Names);
    private bool ShowGenerate = false;

    private async Task GeneratePreview()
    {
        using var ms = new MemoryStream();
        await Configure.BackgroundImage.OpenReadStream().CopyToAsync(ms);
        Configure.Buffer = ms.ToArray();

        var pdf = GetPdf(Configure.Name, Configure);
        var imageSettings = new QuestPDF.Infrastructure.ImageGenerationSettings
        {
            RasterDpi = 96
        };
        var preview = pdf.GenerateImages(imageSettings).First();

        Configure.Buffer = preview;
        ShowGenerate = true;
    }

    private void InputFile(InputFileChangeEventArgs e)
    {
        Configure.BackgroundImage = e.File;
    }

    private async Task Download()
    {
        var result = await GenerateCertificatesZip(Configure);
        await result.FlushAsync();
        result.Seek(0, SeekOrigin.Begin);
        using var streamRef = new DotNetStreamReference(stream: result);

        await JS.InvokeVoidAsync("download", $"Certificados_{DateTime.Now:yyyy-MM-dd}.zip", streamRef);
    }

    private static Document GetPdf(string name, ConfigureCertificate configure)
    {
        if (configure.Captalize)
        {
            name = Captalize(name);
        }

        if (configure.ToUpper)
        {
            name = name.ToUpper();
        }

        var document = Document.Create(container =>
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Background().Image(configure.Buffer);
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

    public static async Task<Stream> GenerateCertificatesZip(ConfigureCertificate configure)
    {
        using var ms = new MemoryStream();
        await configure.BackgroundImage.OpenReadStream().CopyToAsync(ms);
        configure.Buffer = ms.ToArray();

        var names = configure.Names
            .Split("\n")
            .Where(n => !string.IsNullOrEmpty(n))
            .Select(n => n.Trim())
            .OrderBy(x => x);

        var compressedFileStream = new MemoryStream();
        compressedFileStream.Seek(0, SeekOrigin.Begin);

        using var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, true);
        foreach (var name in names)
        {
            var zipEntry = zipArchive.CreateEntry($"{name}.pdf", CompressionLevel.Optimal);

            var pdf = GetPdf(name, configure);

            using var zipEntryStream = zipEntry.Open();

            await zipEntryStream.WriteAsync(pdf.GeneratePdf());
        }

        return compressedFileStream;
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
}
