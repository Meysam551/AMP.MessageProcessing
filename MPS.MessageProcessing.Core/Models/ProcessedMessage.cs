
namespace MPS.MessageProcessing.Core.Models;

public class ProcessedMessage
{
    public int Id { get; set; }
    public string Engine { get; set; } = string.Empty;
    public int MessageLength { get; set; }
    public bool IsValid { get; set; } = true;
    public Dictionary<string, bool> RegexResults { get; set; } = new();
}