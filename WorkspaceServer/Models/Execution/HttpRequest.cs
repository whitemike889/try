using System;

namespace WorkspaceServer.Models.Execution
{
    public class HttpRequest
    {
        public HttpRequest(string uri, string verb, string body = null)
        {
            if (!Uri.TryCreate(uri, UriKind.Relative, out var parseduri))
            {
                throw new ArgumentException("Value must be a valid relative uri", nameof(uri));
            }

            if (string.IsNullOrWhiteSpace(verb))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(verb));
            }

            Uri = parseduri;
            Verb = verb;
            Body = body ?? string.Empty;
        }

        public Uri Uri { get; }

        public string Verb { get; }

        public string Body { get; }
    }
}
