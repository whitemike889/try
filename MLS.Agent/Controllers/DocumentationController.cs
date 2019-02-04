using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public async Task<IActionResult> ShowMarkdownFile(string path)
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
                await Scaffold($"{hostUrl.Scheme}://{hostUrl.Authority}",
                         markdownFile), "text/html");
        }

        private static async Task<IHtmlContent> SessionControlsHtml(MarkdownFile markdownFile)
        {
            var sessions= (await markdownFile
                   .GetCodeLinkBlocks())
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

        private async Task<string> Scaffold(string hostUrl, MarkdownFile markdownFile) =>
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

    {Header()}

    <div class=""documentation-container"">
        <div class=""code-column"">
            {await markdownFile.ToHtmlContentAsync()}
        </div>
        <div class=""control-column"">
            {await SessionControlsHtml(markdownFile)}
        </div>
    </div>

    </div>

    {Footer()}

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
    
    {Header()}

        <ul class=""code-example-list"">
            {html}
        </ul>
    </div>

    {Footer()}

</body>

</html>";

        private string Header() => @"
<header class=""header"">
    <div class=""shimmer"">
        <a href=""https://dotnet.microsoft.com/platform/try-dotnet"">
            Powered by Try .NET
        </a>
    </div>
</header>";

        private string Footer() => @"
<footer class=""footer"">
  <div class=""content has-text-centered"">
    <ul>
        <li>
            <a href=""https://github.com/dotnet/try/issues"">Report a Bug</a>
        </li>
        <li>
            <a href=""https://dotnet.microsoft.com/platform/support-policy"">Support Policy</a>
        </li>
        <li>
            <a href=""https://go.microsoft.com/fwlink/?LinkId=521839"">Privacy &amp; Cookies</a>
        </li>
        <li>
            <a href=""https://go.microsoft.com/fwlink/?LinkID=206977"">Terms of Use</a>
        </li>
        <li>
            <a href=""https://www.microsoft.com/trademarks"">Trademarks</a>
        </li>
        <li>
            © Microsoft 2019
        </li>
    </ul>
  </div>
</footer>";
    }
}