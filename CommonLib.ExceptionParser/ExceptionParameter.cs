namespace CommonLib.ExceptionParser;

public record ExceptionParameter
{
    public string? Name { get; set; }
    public string Type { get; set; } = null!;
    public string[] GenericParameters { get; set; } = null!;
}
