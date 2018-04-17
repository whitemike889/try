using System;
using System.Linq;

namespace WorkspaceServer.Models.SingatureHelp
{
    public static class SignatureHelpResponseExtensions
    {
        public static SignatureHelpResponse ProcessDocumentation(this SignatureHelpResponse source)
        {
            var ret = new SignatureHelpResponse
            {
                ActiveParameter = source.ActiveParameter,
                ActiveSignature = source.ActiveSignature,
                Signatures = source.Signatures?.Select(s => ProcessDocumentation((SignatureHelpItem)s)) ?? Enumerable.Empty<SignatureHelpItem>()
            };

            return ret;
        }

        public static SignatureHelpItem ProcessDocumentation(this SignatureHelpItem source)
        {
            var ret = new SignatureHelpItem
            {
                Name = source.Name,
                Label = source.Label,
                Documentation = DocumentationConverter.ConvertDocumentation(source.Documentation, "\n"),
                Parameters = source.Parameters?.Select(p => p.ProcessDocumentation()) ?? Enumerable.Empty<SignatureHelpParameter>()
            };

            return ret;
        }

        public static SignatureHelpParameter ProcessDocumentation(this SignatureHelpParameter source)
        {
            var ret = new SignatureHelpParameter
            {
                Name = source.Name,
                Label = source.Label,
                Documentation = string.IsNullOrWhiteSpace(source.Documentation) ? source.Documentation : DocumentationConverter.ConvertDocumentation(source.Documentation, "\n")
            };

            return ret;
        }
    }
}