using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuestPDF.Helpers;

namespace Cleo.Certificado.Models;

public class ConfigureModel
{
    [Display(Name = "Imagem")]
    public IFormFile BackGroundImgage { get; set; } = default!;

    [Display(Name = "Nome de teste")]
    public string Name { get; set; } = default!;

    [Display(Name = "Nomes")]
    public string Names { get; set; } = default!;

    [Display(Name = "Nome da fonte")]
    public string FontFamily { get; set; } = Fonts.TimesNewRoman!;

    [Display(Name = "Cor da Fonte")]
    public string FontColor { get; set; } = Colors.Black;

    [Display(Name = "Tamanho da fonte")]
    public int FontSize { get; set; } = 25;

    [Range(0, 560)]
    [Display(Name = "Posição do vertical do nome")]
    public int PositionY { get; set; } = 230;

    [Display(Name = "Em itálico")]
    public bool Itailc { get; set; }

    [Display(Name = "Em negrito")]
    public bool Bold { get; set; }

    [Display(Name = "Normalize")]
    public bool Captalize { get; set; }

    [Display(Name = "Caixa Alta")]
    public bool ToUpper { get; set; }

    public byte[] Preview { get; set; } = [];

    public static IEnumerable<SelectListItem> FontNames =>
    [
        new (Fonts.Arial,Fonts.Arial),
        new (Fonts.Calibri,Fonts.Calibri),
        new (Fonts.LucidaConsole,Fonts.LucidaConsole),
        new (Fonts.TimesNewRoman,Fonts.TimesNewRoman),
        new (Fonts.Verdana,Fonts.Verdana),
        new ("Roboto","Roboto"),
        new ("DejaVu","DejaVu Sans"),
        new ("Amaranth","Amaranth"),
    ];
}