using Microsoft.AspNetCore.Mvc;

namespace MLS.Agent.Controllers
{
    public class ClientConfigurationController : Controller
    {
        [HttpPost]
        [Route("/clientConfiguration")]
        public IActionResult Configuration() =>
            Content(@"{
    ""versionId"": ""0SVN7/Ds8dgNvTokmNLgz4A1WdDDE5UAnbeCnVish+U="",
    ""defaultTimeoutMs"": 15000,
    ""_links"": {
        ""_self"": {
            ""timeoutMs"": 15000,
            ""href"": ""/clientConfiguration"",
            ""templated"": false,
            ""properties"": [],
            ""method"": ""POST"",
            ""body"": ""{}""
        },
        ""configuration"": {
            ""timeoutMs"": 15000,
            ""href"": ""/clientConfiguration"",
            ""templated"": false,
            ""properties"": [],
            ""method"": ""POST""
        },
        ""completion"": {
            ""timeoutMs"": 600000,
            ""href"": ""/workspace/completion"",
            ""templated"": false,
            ""properties"": [
                {
                    ""name"": ""completionProvider""
                }
            ],
            ""method"": ""POST""
        },
        ""acceptCompletion"": {
            ""timeoutMs"": 15000,
            ""href"": ""{acceptanceUri}"",
            ""templated"": true,
            ""properties"": [],
            ""method"": ""POST""
        },
        ""loadFromGist"": {
            ""timeoutMs"": 15000,
            ""href"": ""/workspace/fromgist/{gistId}/{commitHash?}"",
            ""templated"": true,
            ""properties"": [
                {
                    ""name"": ""workspaceType""
                },
                {
                    ""name"": ""extractBuffers""
                }
            ],
            ""method"": ""GET""
        },
        ""diagnostics"": {
            ""timeoutMs"": 600000,
            ""href"": ""/workspace/diagnostics"",
            ""templated"": false,
            ""properties"": [],
            ""method"": ""POST""
        },
        ""signatureHelp"": {
            ""timeoutMs"": 600000,
            ""href"": ""/workspace/signatureHelp"",
            ""templated"": false,
            ""properties"": [],
            ""method"": ""POST""
        },
        ""run"": {
            ""timeoutMs"": 600000,
            ""href"": ""/workspace/run"",
            ""templated"": false,
            ""properties"": [],
            ""method"": ""POST""
        },
        ""snippet"": {
            ""timeoutMs"": 15000,
            ""href"": ""/snippet"",
            ""templated"": false,
            ""properties"": [
                {
                    ""name"": ""from""
                }
            ],
            ""method"": ""GET""
        },
        ""version"": {
            ""timeoutMs"": 15000,
            ""href"": ""/sensors/version"",
            ""templated"": false,
            ""properties"": [],
            ""method"": ""GET""
        },
        ""compile"": {
            ""timeoutMs"": 600000,
            ""href"": ""/workspace/compile"",
            ""templated"": false,
            ""properties"": [],
            ""method"": ""POST""
        },
        ""projectFromGist"": {
            ""timeoutMs"": 15000,
            ""href"": ""/project/fromGist"",
            ""templated"": false,
            ""properties"": [],
            ""method"": ""POST""
        },
        ""regionsFromFiles"": {
            ""timeoutMs"": 15000,
            ""href"": ""/project/files/regions"",
            ""templated"": false,
            ""properties"": [],
            ""method"": ""POST""
        }
    },
    ""applicationInsightsKey"": """"
}", "application/json");
    }
}