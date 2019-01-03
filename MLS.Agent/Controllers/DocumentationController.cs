using Microsoft.AspNetCore.Mvc;

namespace MLS.Agent.Controllers
{
    public class DocumentationController : Controller
    {
        private IMarkdownProject _markdownProject;

        public DocumentationController(IMarkdownProject markdownProject)
        {
            _markdownProject = markdownProject;
        }

        [HttpGet]
        [Route("{*path}")]
        public IActionResult ShowMarkdownFile(string path)
        {
            if (!_markdownProject.TryGetHtmlContent(path, out string html))
            {
                return NotFound();
            }
                
            return Content(html, "text/html");
        }
    }
}