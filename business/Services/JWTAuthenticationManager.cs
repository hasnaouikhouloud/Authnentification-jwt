using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MimeKit;
using Modele;
using ORM;
using ORM.ModelEmail;
using ORM.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace business.Services
{
    public class JWTAuthenticationManager : IJWTAuthenticationManager
    {
        
        private readonly ApplicationDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly EmailConfiguration _emailConfig;

        public JWTAuthenticationManager(ApplicationDbContext context , IOptions<AppSettings> appSettings , EmailConfiguration emailConfig)
        {
            _context = context;
            _appSettings = appSettings.Value;
            _emailConfig = emailConfig;
        }

        public AuthenticateResponse Authenticate(AuthenticateRequest model)
        {
            var user = _context.Users.SingleOrDefault(x => (x.UserName == model.Username || x.Email == model.Username) && x.PasswordHash == model.Password);
           
            var role = _context.Users.Include(u => u.Roles).ToList();
            // return null if user not found
           
            if (user == null )
            {
                return null;
            }
            // authentication successful so generate jwt 
            var jwtToken = generateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken , role);
        }

        private string generateJwtToken(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(120),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public IEnumerable<ApplicationUser> GetAll()
        {
            return _context.Users.Include(u => u.Roles);
        }

        public ApplicationUser GetById(string id)
        {
            return _context.Users.Find(id);
        }

        [Obsolete]
        public void SendEmail(Message message)
        {
            var emailMessage = CreateEmailMessage(message);

            Send(emailMessage);
        }

       

       
        private MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_emailConfig.From));
            emailMessage.To.AddRange(message.To);
            emailMessage.Subject = message.Subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = string.Format("<h0 style='color:noir;'>{0}</h0>", message.Content) };

            
            emailMessage.Body = bodyBuilder.ToMessageBody();
            return emailMessage;
        }


        private void Send(MimeMessage mailMessage)
        {
            using (var client = new SmtpClient())
            {
                try
                {
                    client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
                    client.AuthenticationMechanisms.Remove("XOAUTH2");
                    client.Authenticate(_emailConfig.UserName, _emailConfig.Password);

                    client.Send(mailMessage);
                }
                catch
                {
                    //log an error message or throw an exception, or both.
                    throw;
                }
                finally
                {
                    client.Disconnect(true);
                    client.Dispose();
                }
            }
        }

        
    }
}
