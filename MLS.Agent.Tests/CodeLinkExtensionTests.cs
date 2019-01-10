using FluentAssertions;
using Markdig;
using MLS.Agent.Markdown;
using MLS.Project.Generators;
using Xunit;

namespace MLS.Agent.Tests
{
    public class CodeLinkExtensionTests
    {
        [Theory]
        [InlineData("cs")]
        [InlineData("csharp")]
        [InlineData("c#")]
        [InlineData("CS")]
        [InlineData("CSHARP")]
        [InlineData("C#")]
        public void Inserts_code_when_an_existing_file_is_linked(string language)
        {
            var testDir = TestAssets.SampleConsole;
            var fileContent = @"using System;

namespace BasicConsoleApp
    {
        class Program
        {
            static void MyProgram(string[] args)
            {
                Console.WriteLine(""Hello World!"");
            }
        }
    }".EnforceLF();
            var options = new InMemoryDirectoryAccessor(testDir)
            {
                 ("Program.cs", fileContent)
            };
            var pipeline = new MarkdownPipelineBuilder().UseCodeLinks(options).Build();
            var document = 
$@"```{language} Program.cs
```";
            string html = Markdig.Markdown.ToHtml(document, pipeline).EnforceLF();
            html.Should().Contain(fileContent.HtmlEncode());
        }

        [Fact]
        public void Does_not_insert_code_when_specified_language_is_not_csharp()
        {
            string expectedValue =
@"<pre><code class=""language-js"">console.log(&quot;Hello World&quot;);
</code></pre>
".EnforceLF();

            var testDir = TestAssets.SampleConsole;
            var options = new InMemoryDirectoryAccessor(testDir);
            var pipeline = new MarkdownPipelineBuilder().UseCodeLinks(options).Build();
            var document = @"
```js Program.cs
console.log(""Hello World"");
```";
            var html = Markdig.Markdown.ToHtml(document, pipeline).EnforceLF();
            html.Should().Contain(expectedValue);
        }

        [Fact]
        public void Does_not_insert_code_when_a_linked_file_does_not_exist()
        {
            var testDir = TestAssets.SampleConsole;
            var options = new InMemoryDirectoryAccessor(testDir);
            var pipeline = new MarkdownPipelineBuilder().UseCodeLinks(options).Build();
            var document = 
@"```cs DOESNOTEXIST
```";
            var html = Markdig.Markdown.ToHtml(document, pipeline).EnforceLF();
            html.Should().Contain("File not found: DOESNOTEXIST");
        }
    }
}
