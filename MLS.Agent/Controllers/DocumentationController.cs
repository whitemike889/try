using Microsoft.AspNetCore.Mvc;

namespace MLS.Agent.Controllers
{
    public class DocumentationController : Controller
    {
        private readonly IMarkdownProject _markdownProject;

        public DocumentationController(IMarkdownProject markdownProject)
        {
            _markdownProject = markdownProject;
        }

        [HttpGet]
        [Route("{*path}")]
        public IActionResult ShowMarkdownFile(string path)
        {
            if (!_markdownProject.TryGetHtmlContent(path, out string htmlBody))
            {
                return NotFound();
            }

            return Content(Scaffold(htmlBody), "text/html");
        }

        private string Scaffold(string html) =>
            $@"
<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8""></meta>
    <script src=""//trydotnet.microsoft.com/api/trydotnet.min.js""></script>
</head>

<body>
    {html}

    <script>trydotnet.autoEnable(new URL(""http://localhost:5000/""));</script>
</body>

</html>";
    }
}