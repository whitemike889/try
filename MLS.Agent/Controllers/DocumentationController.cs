using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace MLS.Agent.Controllers
{
    public class DocumentationController : Controller
    {
        private readonly MarkdownProject _markdownProject;

        public DocumentationController(MarkdownProject markdownProject, StartupOptions startupOptions)
        {
            _markdownProject = markdownProject ??
                               throw new ArgumentNullException(nameof(markdownProject));
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
                                     $@"<li ><a class=""code-example"" href=""{f.Path.Value.HtmlAttributeEncode()}""><span class=""icon is-small""><i class=""source-file""></i></span><span>{f.Path.Value}</span></a></li>"));

                return Content(Index(links), "text/html");
            }

            var relativeFilePath = new RelativeFilePath(path);

            if (!_markdownProject.TryGetMarkdownFile(relativeFilePath, out var markdownFile))
            {
                return NotFound();
            }

            var hostUrl = Request.GetUri();

            return Content(
                Scaffold($"{hostUrl.Scheme}://{hostUrl.Authority}",
                         markdownFile), "text/html");
        }

        private static IHtmlContent SessionControlsHtml(MarkdownFile markdownFile)
        {
            var sessions= markdownFile
                   .GetCodeLinkBlocks()
                   .Select(b => b.Session)
                   .Distinct();

            var sb = new StringBuilder();

            foreach (var session in sessions)
            {
                sb.AppendLine($@"<button class=""run-button"" data-trydotnet-mode=""run"" data-trydotnet-session-id=""{session}"">{session}</button>");
                sb.AppendLine($@"<div class=""output-panel"" data-trydotnet-mode=""runResult"" data-trydotnet-session-id=""{session}""></div>");
            }

            return new HtmlString(sb.ToString());
        }

        private string Scaffold(string hostUrl, MarkdownFile markdownFile) =>
            $@"
<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8""></meta>
    <script src=""/api/trydotnet.min.js""></script>
    <link rel=""stylesheet"" href=""/css/trydotnet.css"">
</head>

<body>
    <div class=""content"">
    <div class=""documentation-container"">
        <div class=""code-column"">
            {markdownFile.ToHtmlContent()}
        </div>
        <div class=""control-column"">
            {SessionControlsHtml(markdownFile)}
        </div>
    </div>
    </div>
    <script>trydotnet.autoEnable(new URL(""{hostUrl}""));</script>
</body>

</html>";

        private string Index(string html) =>
            $@"
<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8""></meta>
    <link rel=""stylesheet"" href=""/css/trydotnet.css"">
</head>

<body>
    <div class=""content"">
        <ul class=""code-example-list"">
            {html}
        </ul>
    </div>
</body>

</html>";
    }
}