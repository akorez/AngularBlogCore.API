using AngularBlogCore.API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Mail;

namespace AngularBlogCore.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class HelperController : ControllerBase
    {
        [HttpPost]
        public IActionResult SendContactEmail(Contact contact)
        {
            try
            {
                MailMessage mailMessage = new MailMessage();
                SmtpClient smtpClient = new SmtpClient("mail.teknohub.com");

                mailMessage.From = new MailAddress("ornek@teknohub.net");
                mailMessage.To.Add("ornek@outlook.com");
                mailMessage.Subject = contact.Subject;
                mailMessage.Body = contact.Message;
                mailMessage.IsBodyHtml = true;
                smtpClient.Port = 587;
                smtpClient.Credentials = new System.Net.NetworkCredential("ornek@teknohub.net", "ornekOrnek1");

                smtpClient.Send(mailMessage);

                return Ok();
            }
            catch (Exception ex)
            {

                return BadRequest(ex.Message);
            }

        }


    }
}
