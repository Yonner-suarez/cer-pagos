public class WompiEventoRequest
{
    public WompiData data { get; set; }
    public string @event { get; set; }
    public string environment { get; set; }
    public WompiSignature signature { get; set; }
    public long timestamp { get; set; }
}

public class WompiData
{
    public WompiTransaction transaction { get; set; }
}

public class WompiTransaction
{
    public string id { get; set; }
    public string status { get; set; }
    public string currency { get; set; }
    public string reference { get; set; }
    public int amount_in_cents { get; set; }
}

public class WompiSignature
{
    public string checksum { get; set; }
    public List<string> properties { get; set; }
}
