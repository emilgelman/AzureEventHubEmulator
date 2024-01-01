using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Amqp;
using Amqp.Listener;
using AzureEventHubEmulator.Azure;
using AzureEventHubEmulator.Configuration;

namespace AzureEventHubEmulator.AMQP;

public class ContainerHostFactory
{
    private readonly AzureHandler _azureHandler;
    private readonly AzureSaslProfile _azureSaslProfile;
    private readonly EmulatorOptions _emulatorOptions;

    public ContainerHostFactory(AzureHandler azureHandler, AzureSaslProfile azureSaslProfile, EmulatorOptions emulatorOptions)
    {
        _azureHandler = azureHandler;
        _azureSaslProfile = azureSaslProfile;
        _emulatorOptions = emulatorOptions;
    }

    public IContainerHost Create()
    {
        X509Certificate2? cert;
        if (_emulatorOptions.ServerCertificatePath is null)
        {
            cert = buildSelfSignedServerCertificate("localhost");
            using X509Store store = new X509Store(StoreName.AuthRoot, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadWrite);
            store.Add(cert);
            store.Close();
        }
        else
        {
            cert = new X509Certificate2(_emulatorOptions.ServerCertificatePath, _emulatorOptions.ServerCertificatePassword, X509KeyStorageFlags.Exportable);
        }

        Address address = new("amqps://localhost:5671");
        ContainerHost host = new(new[] { address }, cert);
        host.Listeners[0].HandlerFactory = _ => _azureHandler;
        host.Listeners[0].SASL.EnableMechanism(_azureSaslProfile.Mechanism, _azureSaslProfile);
        host.Listeners[0].SASL.EnableExternalMechanism = true;
        host.Listeners[0].SASL.EnableAnonymousMechanism = true;
        host.Listeners[0].SSL.ClientCertificateRequired = true;
        host.Listeners[0].SSL.RemoteCertificateValidationCallback = (_, _, _, errors) => errors == SslPolicyErrors.RemoteCertificateNotAvailable;


        return host;
    }


    private X509Certificate2 buildSelfSignedServerCertificate(string certificateName)
    {
        SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();
        sanBuilder.AddIpAddress(IPAddress.Loopback);
        sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
        sanBuilder.AddDnsName("localhost");
        sanBuilder.AddDnsName(Environment.MachineName);

        X500DistinguishedName distinguishedName = new X500DistinguishedName($"CN={certificateName}");

        using (RSA rsa = RSA.Create(2048))
        {
            var request = new CertificateRequest(distinguishedName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            request.CertificateExtensions.Add(
                new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));


            request.CertificateExtensions.Add(
                new X509EnhancedKeyUsageExtension(
                    new OidCollection { new Oid("1.3.6.1.5.5.7.3.1") }, false));

            request.CertificateExtensions.Add(sanBuilder.Build());

            var certificate = request.CreateSelfSigned(new DateTimeOffset(DateTime.UtcNow.AddDays(-1)), new DateTimeOffset(DateTime.UtcNow.AddDays(3650)));
            // certificate.FriendlyName = certificateName;
            return new X509Certificate2(certificate.Export(X509ContentType.Pfx, "password"), "password", X509KeyStorageFlags.MachineKeySet);
        }
    }
}