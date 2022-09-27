using System.Security.Cryptography.X509Certificates;
using AuthHelp;
using Ingredients.Data;
using Ingredients.Services;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;

var macOS = OperatingSystem.IsMacOS();

var builder = WebApplication.CreateBuilder(args);

if (macOS)
{
    builder.WebHost.ConfigureKestrel(k =>
    {
        k.ListenLocalhost(5002, l =>
        {
            l.Protocols = HttpProtocols.Http2;
        });
    });
}
else
{
    builder.WebHost.ConfigureKestrel(k =>
    {
        k.ConfigureHttpsDefaults(https =>
        {
            var serverCert = ServerCert.Get();
            https.ServerCertificate = serverCert;
            https.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
            https.ClientCertificateValidation = (cert, _, _) =>
                cert.Issuer == serverCert.Issuer;
        });
    });
}

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddSingleton<IToppingData, ToppingData>();
builder.Services.AddSingleton<ICrustData, CrustData>();
builder.Services.AddSingleton<IngredientsImpl>();

builder.Services.AddAuthentication(CertificateAuthenticationDefaults.AuthenticationScheme)
    .AddCertificate(o =>
    {
        o.ValidateCertificateUse = false;
        o.ValidateValidityPeriod = false;
        o.RevocationMode = X509RevocationMode.NoCheck;
        o.AllowedCertificateTypes = CertificateTypes.SelfSigned;
        o.Events = new CertificateAuthenticationEvents
        {
            OnCertificateValidated = DevelopmentModeCertificateHelper.Validate
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCertificateForwarding();
app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.

app.MapGrpcService<IngredientsImpl>();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
