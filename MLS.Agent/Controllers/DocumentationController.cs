using System;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace MLS.Agent.Controllers
{
    public class DocumentationController : Controller
    {
        private readonly IMarkdownProject _markdownProject;

        public DocumentationController(IMarkdownProject markdownProject)
        {
            _markdownProject = markdownProject ?? 
                               throw new ArgumentNullException(nameof(markdownProject));
        }

        [HttpGet]
        [Route("{*path}")]
        public IActionResult ShowMarkdownFile(string path)
        {
            if (!_markdownProject.TryGetHtmlContent(path, out string htmlBody))
            {
                return NotFound("No markdowns here...");
            }

            var hostUrl = Request.GetUri();

            return Content(
                Scaffold(htmlBody,
                         $"{hostUrl.Scheme}://{hostUrl.Authority}"), "text/html");
        }

        private string Scaffold(string html, string hostUrl)
        {
            return $@"
<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8""></meta>
    <script src=""//trydotnet.microsoft.com/api/trydotnet.min.js""></script>
</head>

<body>
    {html}

    <script>trydotnet.autoEnable(new URL(""{hostUrl}""));</script>
</body>

</html>";
        }
    }
}