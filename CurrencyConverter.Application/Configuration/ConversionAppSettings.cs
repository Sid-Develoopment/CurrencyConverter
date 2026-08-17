namespace CurrencyConverter.Application.Configuration;

/// <summary>
/// Настройки, специфичные для уровня приложения
/// </summary>
public class ConversionAppSettings
{
    public const string Section = "Conversion";
    
    /// <summary>
    /// Точность по умолчанию для конвертации
    /// </summary>
    public int DefaultPrecision { get; set; } = 4;
    
    /// <summary>
    /// Максимальная длина цепочки конвертации (триангуляция)
    /// </summary>
    public int MaxConversionChainLength { get; set; } = 3;
    
    /// <summary>
    /// Разрешать ли автоматическую триангуляцию
    /// </summary>
    public bool AllowTriangulation { get; set; } = true;
}