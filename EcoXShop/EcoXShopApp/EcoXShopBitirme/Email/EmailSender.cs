using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using System;
using System.Threading.Tasks;
using MimeKit;

namespace EcoXShopBitirme.Email
{
    public class EmailSender : IEmailSender
    {

        private int _port;
        private string _host;
        private bool _enableSSL;
        private string _password;
        private string _username;

        public EmailSender(IConfiguration configuration)
        {
            this._host = configuration["EmailSender:Host"];
            this._port = configuration.GetValue<int>("EmailSender:Port");
            this._username = configuration["EmailSender:UserName"];
            this._enableSSL = configuration.GetValue<bool>("EmailSender:EnableSSL");
            this._password = configuration["EmailSender:Password"];
        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                // Kullanıcı tarafından girilen e-posta adresini kontrol et


                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("EcoXShop", _username));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = htmlMessage;
                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_host, _port, _enableSSL);
                    await client.AuthenticateAsync(_username, _password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }
            }
            catch (FormatException ex)
            {

                throw new FormatException("Geçerli bir e-posta adresi giriniz.", ex);
            }
        }
    }
}
