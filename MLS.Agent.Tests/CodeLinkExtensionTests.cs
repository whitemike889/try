using FluentAssertions;
using Markdig;
using MLS.Agent.Markdown;
using MLS.Project.Generators;
using Xunit;

namespace MLS.Agent.Tests
{
    public class CodeLinkExtensionTests
    {
        private readonly string ProgramContent =
@"using System;

namespace BasicConsoleApp
{
    class Program
    {
        static void MyProgram(string[] args)
        {
            Console.WriteLine(&quot;Hello World!&quot;);
        }
    }
}".EnforceLF();

        [Theory]
        [InlineData("cs")]
        [InlineData("csharp")]
        [InlineData("c#")]
        [InlineData("CS")]
        [InlineData("CSHARP")]
        [InlineData("C#")]
        public void Replaces_filename_in_a_fenced_code_block_with_code_present_in_that_file(string language)
        {
            var testDir = TestAssets.BasicConsole;
            var options = new Configuration(testDir);
            var pipeline = new MarkdownPipelineBuilder().UseCodeLinks(options).Build();
            var document = 
$@"```{language} Program.cs
```";
            string html = Markdig.Markdown.ToHtml(document, pipeline).EnforceLF();
            html.Should().Contain(ProgramContent);
        }

        [Fact]
        public void Doesnot_replace_filename_if_specified_language_is_not_csharp()
        {
            string expectedValue =
@"<pre><code class=""language-js"">console.log(&quot;Hello World&quot;);
</code></pre>
".EnforceLF();

            var testDir = TestAssets.BasicConsole;
            var options = new Configuration(testDir);
            var pipeline = new MarkdownPipelineBuilder().UseCodeLinks(options).Build();
            var document = @"
```js Program.cs
console.log(""Hello World"");
```";
            var html = Markdig.Markdown.ToHtml(document, pipeline).EnforceLF();
            html.Should().NotContain(ProgramContent);
            html.Should().Contain(expectedValue);
        }

        [Fact]
        public void Doesnot_replace_filename_if_the_file_doesnot_exist()
        {
            //todo: what is the expected output in cases where there is unexpected input like below
            var testDir = TestAssets.BasicConsole;
            var options = new Configuration(testDir);
            var pipeline = new MarkdownPipelineBuilder().UseCodeLinks(options).Build();
            var document = 
@"```cs DOESNOTEXIST
```";
            var html = Markdig.Markdown.ToHtml(document, pipeline).EnforceLF();
            html.Should().Contain("Error reading the file DOESNOTEXIST");
        }
    }
}
