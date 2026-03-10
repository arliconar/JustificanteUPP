using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store; // Necesario para FileDataStore
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks; // Necesario para Task
using MimeKit;
namespace JustificantesUPP
{
    public class MailClass
    {
        // Cambiado a public, async Task y nombre en PascalCase
        public MailClass()
        {

        }
        public async Task EnviarAsync(MimeMessage mensaje)
        {
            UserCredential credential;
            string credPath = "C:\\Justificantes\\token.json";
            string credentialsFile = "C:\\Justificantes\\credentials.json";

            try
            {
                credential = await ObtenerCredenciales(credentialsFile, credPath);

                // Verificamos si el token ha caducado y tratamos de refrescarlo manualmente
                // por si la librería no lo hizo automáticamente antes de enviar.
                if (credential.Token.IsStale)
                {
                    if (!await credential.RefreshTokenAsync(CancellationToken.None))
                    {
                        // Si no se pudo refrescar, lanzamos excepción para forzar re-autorización
                        throw new Exception("No se pudo refrescar el token.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Si hay error de autenticación, borramos la carpeta del token y reintentamos una vez más
                if (Directory.Exists(credPath))
                {
                    Directory.Delete(credPath, true);
                }
                credential = await ObtenerCredenciales(credentialsFile, credPath);
            }

            var service = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "Justificantes UPP",
            });

            string base64RawMessage;
            using (var stream = new MemoryStream())
            {
                mensaje.WriteTo(stream);
                base64RawMessage = Convert.ToBase64String(stream.ToArray())
                    .Replace('+', '-')
                    .Replace('/', '_')
                    .Replace("=", "");
            }

            var gmailMessage = new Message { Raw = base64RawMessage };
            await service.Users.Messages.Send(gmailMessage, "me").ExecuteAsync();
        }

        // Método auxiliar para centralizar la petición de tokens
        private async Task<UserCredential> ObtenerCredenciales(string clientSecretsPath, string tokenPath)
        {
            using (var stream = new FileStream(clientSecretsPath, FileMode.Open, FileAccess.Read))
            {
                return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { GmailService.Scope.GmailSend },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(tokenPath, true));
            }
        }
    }
}