using System;
using Assent;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Recipes;
using WorkspaceServer.Models.Execution;
using Xunit;

namespace WorkspaceServer.Tests
{
    public class ContractTests
    {
        private readonly Configuration configuration;

        public ContractTests()
        {
            configuration = new Configuration()
                .UsingExtension("json");

#if !DEBUG
            configuration = configuration.SetInteractive(false);
#endif
        }

        [Fact]
        public void RunResult_with_no_features()
        {
            var runResult = new RunResult(
                true,
                new[] { "output one", "output two", "output three" },
                diagnostics: new[]
                {
                    new SerializableDiagnostic(
                        start: 1,
                        end: 4,
                        message: "oops",
                        severity: DiagnosticSeverity.Error,
                        id: "CS1234")
                });

            var serialized = Serialize(runResult);

            this.Assent(serialized, configuration);
        }

        private static string Serialize(RunResult runResult) =>
            runResult.ToJson(new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
    }
}
