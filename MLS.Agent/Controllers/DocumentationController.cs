using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using MLS.Agent.CommandLine;
using MLS.Agent.Markdown;

namespace MLS.Agent.Controllers
{
    public class DocumentationController : Controller
    {
        private readonly MarkdownProject _markdownProject;
        private readonly StartupOptions _startupOptions;

        public DocumentationController(MarkdownProject markdownProject, StartupOptions startupOptions)
        {
            _markdownProject = markdownProject ??
                               throw new ArgumentNullException(nameof(markdownProject));
            _startupOptions = startupOptions;
        }

        [HttpGet]
        [Route("{*path}")]
        public async Task<IActionResult> ShowMarkdownFile(string path)
        {
            if (_startupOptions.IsInHostedMode)
            {
                return Ok();
            }

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
            var pipeline = _markdownProject.GetMarkdownPipelineFor(markdownFile.Path);
            var extension = pipeline.Extensions.FindExact<CodeLinkExtension>();
            

            var blocks = await markdownFile.GetCodeLinkBlocks();
            var maxEditorPerSession = blocks
                .GroupBy(b => b.Session)
                .Max(editors => editors.Count());

            if (extension != null)
            {
                extension.InlineControls = maxEditorPerSession <= 1;
                extension.EnablePreviewFeatures = _startupOptions.EnablePreviewFeatures;
            }


            if (maxEditorPerSession <= 1)
            {
                return Content(
                    await OneColumnLayoutScaffold($"{hostUrl.Scheme}://{hostUrl.Authority}",
                        markdownFile), "text/html");
            }
            else
            {
                return Content(
                    await TwoColumnLayoutScaffold($"{hostUrl.Scheme}://{hostUrl.Authority}",
                        markdownFile), "text/html");
            }
        }

        public static async Task<IHtmlContent> SessionControlsHtml(MarkdownFile markdownFile, bool enablePreviewFeatures = false)
        {
            var sessions= (await markdownFile
                   .GetCodeLinkBlocks())
                   .GroupBy(b => b.Session);

            var sb = new StringBuilder();

            foreach (var session in sessions)
            {
                sb.AppendLine($@"<button class=""run-button"" data-trydotnet-mode=""run"" data-trydotnet-session-id=""{session.Key}"" data-trydotnet-run-args=""{session.First().RunArgs.HtmlAttributeEncode()}"">{session.Key}</button>");
                if (enablePreviewFeatures)
                {
                    sb.AppendLine($@"<div class=""output-panel"" data-trydotnet-mode=""runResult"" data-trydotnet-output-type=""terminal"" data-trydotnet-session-id=""{session.Key}""></div>");
                }
                else
                {
                    sb.AppendLine($@"<div class=""output-panel"" data-trydotnet-mode=""runResult"" data-trydotnet-session-id=""{session.Key}""></div>");
                }
             
            }

            return new HtmlString(sb.ToString());
        }

        private async Task<string> OneColumnLayoutScaffold(string hostUrl, MarkdownFile markdownFile) =>
            $@"
<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"">
    <script src=""/api/trydotnet.min.js""></script>
    <script src=""/api/trydotnet-layout.min.js""></script>
    <link rel=""stylesheet"" href=""/css/trydotnet.css"">
    <title>dotnet try - {markdownFile.Path.Value.HtmlEncode()}</title>
</head>

<body>

    <div class=""content"">

        {Header()}

        <div class=""documentation-container"">
            <div id=""documentation-container"" class=""code-single-column"">
                {await markdownFile.ToHtmlContentAsync()}
            </div>       
        </div>

    </div>

    {Footer()}

    <script>
        trydotnet.autoEnable(new URL(""{hostUrl}""));
        trydotnetLayout.trackTopmostSession(document.getElementById(""documentation-container""), function (e){{ console.log(e); }});
    </script>
</body>

</html>";
        private async Task<string> TwoColumnLayoutScaffold(string hostUrl, MarkdownFile markdownFile) =>
            $@"
<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"">
    <script src=""/api/trydotnet.min.js""></script>
    <script src=""/api/trydotnet-layout.min.js""></script>
    <link rel=""stylesheet"" href=""/css/trydotnet.css"">
    <title>dotnet try - {markdownFile.Path.Value.HtmlEncode()}</title>
</head>

<body>

    <div class=""content"">

        {Header()}

        <div class=""documentation-container"">
            <div id=""documentation-container"" class=""code-column"">
                {await markdownFile.ToHtmlContentAsync()}
            </div>
            <div class=""control-column"">
                {await SessionControlsHtml(markdownFile, _startupOptions.EnablePreviewFeatures)}
            </div>
        </div>

    </div>

    {Footer()}

    <script>
        trydotnet.autoEnable(new URL(""{hostUrl}""));
        trydotnetLayout.trackTopmostSession(document.getElementById(""documentation-container""), function (e){{ console.log(e); }});
    </script>
</body>

</html>";

        private string Index(string html) =>
            $@"
<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"">
    <link rel=""stylesheet"" href=""/css/trydotnet.css"">
    <title>dotnet try - {_startupOptions.RootDirectory.FullName.HtmlEncode()}</title>
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

        private string Header() => $@"
<header class=""header"">
    <div class=""shimmer"">
        <a href=""https://dotnet.microsoft.com/platform/try-dotnet"">
            Powered by Try .NET
        </a>
    </div>
    <div>{_startupOptions.RootDirectory.FullName.HtmlEncode()}</div>
</header>";

        private string Footer() => @"
<footer class=""footer"">
  <div class=""content has-text-centered"">
    <ul>
        <li>
            <a href=""https://teams.microsoft.com/l/channel/19%3a32c2f8c34d4b4136b4adf554308363fc%40thread.skype/Try%2520.NET?groupId=fdff90ed-0b3b-4caa-a30a-efb4dd47665f&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47"">Ask a question or tell us about a bug</a>
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