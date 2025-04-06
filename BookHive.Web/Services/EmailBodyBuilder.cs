using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;

namespace BookHive.Web.Services
{
    public class EmailBodyBuilder : IEmailBodyBuilder
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public EmailBodyBuilder(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public string GetEmailBody(string imageUrl, string header,string url,string linkTitle, string body)
        {
            var filepath = $"{_webHostEnvironment.WebRootPath}/Templates/email.html";
            StreamReader sr = new StreamReader(filepath);
           var _body = sr.ReadToEnd();
            sr.Close();
            _body =_body
                     .Replace("[imageUrl]", imageUrl)
                     .Replace("[header]", header)
                     .Replace("[url]", url)
                     .Replace("[linkTitle]", linkTitle)
                     .Replace("[body]", body);
            return _body;
        }
    }
}
