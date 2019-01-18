using System;
using System.Linq;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using WorkspaceServer.Servers.Roslyn;

namespace MLS.Agent.Controllers
{
    public class DocumentationController : Controller
    {
        private readonly MarkdownProject _markdownProject;
        private readonly StartupOptions startupOptions;

        public DocumentationController(MarkdownProject markdownProject, StartupOptions startupOptions)
        {
            _markdownProject = markdownProject ??
                               throw new ArgumentNullException(nameof(markdownProject));
            this.startupOptions = startupOptions;
        }

        [HttpGet]
        [Route("{*path}")]
        public IActionResult ShowMarkdownFile(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                var links = string.Join(
                    "\n",
                    _markdownProject.GetAllMarkdownFiles()
                                    .Select(f =>
                                     $@"<a href=""{f.Value.HtmlAttributeEncode()}"">{f.Value}</a>"));

                return Content(Index(links), "text/html");
            }

            //to do: If the path contains invalid characters , so the relative path will throw an exception
            // we should handle that
            var relativeFilePath = new RelativeFilePath(path);

            if (!_markdownProject.TryGetHtmlContent(relativeFilePath, out var htmlBody))
            {
                return NotFound();
            }

            var hostUrl = Request.GetUri();

            return Content(
                Scaffold(htmlBody,
                         $"{hostUrl.Scheme}://{hostUrl.Authority}"), "text/html");
        }

        private string Scaffold(string html, string hostUrl) =>
            $@"
<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8""></meta>
    <script src=""/api/trydotnet.min.js""></script>
</head>

<body>
    {html}

    <script>trydotnet.autoEnable(new URL(""{hostUrl}""));</script>
</body>

</html>";

        private string Index(string html) =>
            $@"
<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8""></meta>
</head>

<body>
    {html}
</body>

</html>";
    }
}