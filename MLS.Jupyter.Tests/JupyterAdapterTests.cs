using System;
using MLS.Jupyter.Protocol;
using Xunit;

namespace MLS.Jupyter.Tests
{
    public class JupyterAdapterTests
    {
        [Fact]
        public void do_it()
        {
            var executeRequest = new ExecuteRequest
                                 {
                                     Code = "Console.WriteLine(12);"
                                 };

            


            throw new NotImplementedException();
        }
    }
}