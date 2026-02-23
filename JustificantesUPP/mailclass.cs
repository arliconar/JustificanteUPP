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
            string credPath = "C:\\Justificantes\\token.json"; // Donde se guardará el permiso aprobado

            using (var stream = new FileStream("C:\\Justificantes\\credentials.json", FileMode.Open, FileAccess.Read))
            {
                // El FileDataStore evita que pida loguearse en el navegador cada vez que corres el programa
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    new[] { GmailService.Scope.GmailSend },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true));
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
            var gmailMessage = new Message
            {
                Raw = base64RawMessage
            };

          

            await service.Users.Messages.Send(gmailMessage, "me").ExecuteAsync();
        }
    }
}