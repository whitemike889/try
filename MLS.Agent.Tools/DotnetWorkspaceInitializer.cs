using System;
using System.IO;
using System.Threading.Tasks;
using Clockwise;

namespace MLS.Agent.Tools
{
    public class DotnetWorkspaceInitializer : IWorkspaceInitializer
    {
        private readonly Func<Budget, Task> afterCreate;

        public string Template { get; }

        public string Name { get; }

        public DotnetWorkspaceInitializer(
            string template, 
            string name,
            Func<Budget, Task> afterCreate = null)
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
            Budget budget = null)
        {
            budget = budget ?? new Budget();

            var dotnet = new Dotnet(directory);

            var result = await dotnet
                             .New(Template,
                                  args: $"--name \"{Name}\" --output \"{directory.FullName}\"",
                                  budget: budget);
            result.ThrowOnFailure();

            if (afterCreate != null)
            {
                await afterCreate(budget);
            }
        }
    }
}

