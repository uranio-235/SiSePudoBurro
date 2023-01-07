namespace Domain.Entitities;
public class User : BaseEntity
{
    public string Username { get; set; }
    public long TelegramId { get; set; }
}
