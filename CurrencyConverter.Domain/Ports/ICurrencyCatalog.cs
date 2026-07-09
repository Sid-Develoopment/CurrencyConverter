using CurrencyConverter.Domain.Entities;
using CurrencyConverter.Domain.ValueObjects;

namespace CurrencyConverter.Domain.Ports;

public interface ICurrencyCatalog
{
    Task<Currency?> GetCurrencyAsync(CurrencyCode code, CancellationToken ct = default);
    Task<IReadOnlyCollection<Currency>> GetAllCurrenciesAsync(CancellationToken ct = default);
    Task<bool> SupportsCurrencyAsync(CurrencyCode code, CancellationToken ct = default);
}