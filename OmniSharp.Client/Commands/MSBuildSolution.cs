using System;
using System.Collections.Generic;

namespace OmniSharp.Client.Commands
{
    public class MSBuildSolution
    {
        public MSBuildSolution(string solutionPath, IEnumerable<MSBuildProject> projects)
        {
            SolutionPath = solutionPath;
            Projects = projects ?? Array.Empty<MSBuildProject>();
        }

        public string SolutionPath { get; }

        public IEnumerable<MSBuildProject> Projects { get; }
    }
}