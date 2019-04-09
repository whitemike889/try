using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using Microsoft.DotNet.Try.Project.Extensions;
using Microsoft.DotNet.Try.Protocol.ClientApi;
using Pocket;
using static Pocket.Logger<MLS.Agent.Controllers.ProjectController>;


namespace MLS.Agent.Controllers
{
    public class ProjectController : Controller
    {
        private const string RegionsFromFilesRoute = "/project/files/regions";

        [HttpPost(RegionsFromFilesRoute)]
        public IActionResult GenerateRegionsFromFiles([FromBody] CreateRegionsFromFilesRequest request)
        {
            using (var operation = Log.OnEnterAndConfirmOnExit())
            {
                var regions = request.Files.SelectMany(ExtractRegions);
                var response = new CreateRegionsFromFilesResponse(request.RequestId, regions.ToArray());

                IActionResult result = Ok(response);
                operation.Succeed();

                return result;
            }
        }

        private static IEnumerable<SourceFileRegion> ExtractRegions(SourceFile sourceFile)
        {
            var sc = SourceText.From(sourceFile.Content);
            var regions = sc.ExtractRegions(sourceFile.Name).Select(region => new SourceFileRegion(region.id, region.content)).ToArray();
            return regions;
        }

    }
}
