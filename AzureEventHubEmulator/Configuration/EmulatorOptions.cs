namespace AzureEventHubEmulator.Configuration;

public class EmulatorOptions
{
    public string Topics { get; set; }
    public string? ServerCertificatePath { get; set; }
    public string? ServerCertificatePassword { get; set; }
}