using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MLS.Project.Extensions;
using MLS.Protocol.Execution;

namespace MLS.Project.Generators
{
    public static class BufferGenerator
    {
        public static IEnumerable<Workspace.Buffer> CreateFromFile(Workspace.File file)
        {
            var viewPorts = file.ExtractViewPorts().ToList();
            if (viewPorts.Count > 0)
            {
                foreach (var viewport in viewPorts)
                {
                    yield return CreateBuffer(viewport.Region.ToString(), viewport.BufferId);
                }

            }
            else
            {
                yield return CreateBuffer(file.Text, file.Name);
            }
        }

        public static Workspace.Buffer CreateBuffer(string content, BufferId id)
        {
            MarkupTestFile.GetPosition(content.EnforceLF(), out var output, out var position);

            return new Workspace.Buffer(
                id,
                output,
                position ?? 0);
        }

        private static readonly Random RandomGenerator = new Random();

        public static Workspace.Buffer ScriptCode(string mainContent = @"Console.WriteLine(Sample.Method());
$$")
        {
            var input = $@"{ProcessCode(mainContent, string.Empty)}
".EnforceLF();

            MarkupTestFile.GetPosition(input, out var output, out var position);

            return new Workspace.Buffer(
                 string.Empty,
                 output,

                 position ?? 0);

        }

        public static Workspace.Buffer EntryPointCode(string mainContent = @"Console.WriteLine(Sample.Method());
$$", string bufferId = "EntrypointCode.cs")
        {
            var input = $@"
using System;
using System.Linq;

namespace Example
{{
    public class Program
    {{
        public static void Main()
        {{
{ProcessCode(mainContent, "            ")}
        }}       
    }}
}}";
            return CreateBuffer(mainContent, new BufferId(bufferId));
        }

        private static string ProcessCode(string code, string indent)
        {
            return string.Join(Environment.NewLine, code.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Select(line => $"{indent}{line}"));
        }
        public static Workspace.Buffer ViewportCode(string methodContent, bool addPaddingCode = false, string bufferName = "ViewportCode.cs")
        {

            var input = $@"
using System.Collections.Generic;
using System;

namespace Example
{{
    public static class Sample
    {{
        public static object Method()
        {{
{GeneratePaddingCode("            ", addPaddingCode)}
#region viewport
{GeneratePaddingCode("            ", addPaddingCode)}
{ProcessCode(methodContent, "            ")}
{GeneratePaddingCode("            ", addPaddingCode)}
#endregion
{GeneratePaddingCode("            ", addPaddingCode)}
        return null;
        }}
    }}
}}".EnforceLF();

            return CreateBuffer(input, new BufferId(bufferName));
        }

        private static string GeneratePaddingCode(string indent, bool addPaddingCode)
        {
            if (!addPaddingCode)
            {
                return string.Empty;
            }

            var buffer = new StringBuilder();


            for (var i = 0; i < RandomGenerator.Next(0, 20); i++)
            {
                var randomText = string.Join(" and ", Enumerable.Range(0, RandomGenerator.Next(1, 4)).Select(_ => Guid.NewGuid().ToString()));
                var start = RandomGenerator.Next(0, randomText.Length / 2);
                var length = Math.Min(RandomGenerator.Next(5, randomText.Length - start), randomText.Length - start);
                buffer.AppendLine(
                    RandomGenerator.NextDouble() > 0.7
                        ? $"{indent ?? string.Empty}// code comment {randomText.Substring(RandomGenerator.Next(0, length))}"
                        : $"{indent ?? string.Empty}Console.WriteLine(\"{randomText.Substring(RandomGenerator.Next(0, length))}\");");
            }

            buffer.AppendLine();

            return buffer.ToString();
        }
    }
}
