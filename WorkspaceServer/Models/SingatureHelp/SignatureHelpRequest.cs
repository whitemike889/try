using System;
using System.Collections.Generic;
using System.Text;
using WorkspaceServer.Models.Execution;

namespace WorkspaceServer.Models.SingatureHelp
{
    public class SignatureHelpRequest : WorkspacePositionRequest
    {
        public SignatureHelpRequest(Workspace workspace, string activeBufferId, int position, string[] implicitUsings = null) : base(workspace, activeBufferId, position, implicitUsings)
        {
        }
    }
}
