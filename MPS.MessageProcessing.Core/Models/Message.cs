
namespace MPS.MessageProcessing.Core.Models;

public class Message
{
    public int Id { get; set; }
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}