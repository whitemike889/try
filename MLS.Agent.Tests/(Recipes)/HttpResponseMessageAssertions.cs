using System;
using System.Net.Http;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace Recipes
{
    internal class HttpResponseMessageAssertions : ObjectAssertions
    {
        private readonly HttpResponseMessage _subject;

        public HttpResponseMessageAssertions(HttpResponseMessage subject) : base(subject)
        {
            _subject = subject ?? throw new ArgumentNullException(nameof(subject));
        }

        public AndConstraint<HttpResponseMessageAssertions> BeSuccessful()
        {
            Execute.Assertion
                   .ForCondition(_subject.IsSuccessStatusCode)
                   .FailWith($"Expected successful response but received: {_subject}");

            return new AndConstraint<HttpResponseMessageAssertions>(this);
        }
    }

    internal static class HttpResponseMessageAssertionExtensions
    {
        public static HttpResponseMessage EnsureSuccess(this HttpResponseMessage subject)
        {
            subject.Should().BeSuccessful();

            return subject;
        }

        public static HttpResponseMessageAssertions Should(this HttpResponseMessage subject) => 
            new HttpResponseMessageAssertions(subject);
    }
}
