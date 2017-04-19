using System.Net;
using System.Net.Http;
using FluentAssertions.Execution;
using static System.Environment;

namespace MLS.Agent.Tests
{
    public static class HttpAssertionExtensions
    {
        public static HttpResponseMessage ShouldIndicateSuccess(
            this HttpResponseMessage response,
            HttpStatusCode? expected = null)
        {
            try
            {
                response.EnsureSuccessStatusCode();

                var actualStatusCode = response.StatusCode;

                if (expected != null && actualStatusCode != expected.Value)
                {
                    throw new AssertionFailedException(
                        string.Format("Status code was successful but not of the expected type: {0} was expected but {1} was returned.",
                                      expected,
                                      actualStatusCode));
                }
            }
            catch
            {
                ThrowVerboseAssertion(response);
            }
            return response;
        }

        public static HttpResponseMessage ShouldFailWith(
            this HttpResponseMessage response,
            HttpStatusCode code)
        {
            if (response.StatusCode != code)
            {
                ThrowVerboseAssertion(response);
            }

            return response;
        }

        private static void ThrowVerboseAssertion(HttpResponseMessage response) =>
            throw new AssertionFailedException($"{response}{NewLine}{NewLine}{response.Content}");
    }
}
