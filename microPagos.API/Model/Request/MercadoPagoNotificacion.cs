public class MercadoPagoNotification
{
    public string Id { get; set; }
    public bool LiveMode { get; set; }
    public string Type { get; set; }
    public string DateCreated { get; set; }
    public string ApplicationId { get; set; }
    public int UserId { get; set; }
    public string Version { get; set; }
    public string ApiVersion { get; set; }
    public string Action { get; set; }
    public NotificationData Data { get; set; }
}

public class NotificationData
{
    public long Id { get; set; }
}
