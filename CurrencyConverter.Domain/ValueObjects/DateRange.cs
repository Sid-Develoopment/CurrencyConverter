namespace CurrencyConverter.Domain.ValueObjects;

public sealed class DateRange : IEquatable<DateRange>
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset? End { get; }
    public bool IsSinglePoint => End is null;

    private DateRange(DateTimeOffset start, DateTimeOffset? end)
    {
        if (end < start)
            throw new ArgumentException("End date cannot be before start date");
            
        Start = start;
        End = end;
    }

    public static DateRange At(DateTimeOffset instant) => 
        new(instant, null);

    public static DateRange Between(DateTimeOffset start, DateTimeOffset end) => 
        new(start, end);

    public bool Contains(DateTimeOffset point) => 
        point >= Start && (!End.HasValue || point <= End);

    public bool OverlapsWith(DateRange? other)
    {
        if (other is null)
            return false;
        if (IsSinglePoint && other.IsSinglePoint) 
            return Start == other.Start;
        if (IsSinglePoint) 
            return other.Contains(Start);
        if (other.IsSinglePoint) 
            return Contains(other.Start);
        
        return Start <= (other.End ?? Start) && (End ?? other.Start) >= other.Start;
    }

    public bool Equals(DateRange? other) => 
        other is not null && Start == other.Start && End == other.End;

    public override bool Equals(object? obj) => Equals(obj as DateRange);
    public override int GetHashCode() => HashCode.Combine(Start, End);
}