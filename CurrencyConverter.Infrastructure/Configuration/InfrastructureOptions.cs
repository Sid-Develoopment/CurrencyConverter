namespace CurrencyConverter.Infrastructure.Configuration;

public class CacheOptions
{
    public const string Section = "Infrastructure:Cache";
    public int AbsoluteExpirationMinutes { get; set; } = 60;
    public int SlidingExpirationMinutes { get; set; } = 15;
    public int SizeLimit { get; set; } = 50;
}

public class ProviderOptions
{
    public const string Section = "Infrastructure:Providers:Cbr";
    public string BaseUrl { get; set; } = "https://www.cbr-xml-daily.ru/";
    public int TimeoutSeconds { get; set; } = 30;
    public bool Enabled { get; set; } = true;
}
