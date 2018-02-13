using System;
using System.IO;
using System.Threading.Tasks;
using Clockwise;

namespace MLS.Agent.Tools
{
    public class DotnetWorkspaceInitializer : IWorkspaceInitializer
    {
        private readonly Action<Dotnet> afterCreate;

        public string Template { get; }

        public string Name { get; }

        public DotnetWorkspaceInitializer(string template, string name, Action<Dotnet> afterCreate = null)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(template));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(name));
            }

            this.afterCreate = afterCreate;

            Template = template;

            Name = name;
        }

        public async Task Initialize(
            DirectoryInfo directory,
            TimeBudget budget = null)
        {
            var dotnet = new Dotnet(directory);

            var result = await dotnet
                             .New(Template,
                                  args: $"--name \"{Name}\" --output \"{directory.FullName}\"",
                                  budget: budget);
            result.ThrowOnFailure();

            afterCreate?.Invoke(dotnet);
        }
    }
}

