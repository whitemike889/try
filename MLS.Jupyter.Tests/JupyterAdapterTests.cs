using System;
using System.Text;
using MLS.Jupyter.Protocol;
using NetMQ;
using Xunit;

namespace MLS.Jupyter.Tests
{

    public class JupyterMessageContractTests
    {
        class TextSocket : IOutgoingSocket
        {
            StringBuilder buffer = new StringBuilder();
            public bool TrySend(ref Msg msg, TimeSpan timeout, bool more)
            {
                buffer.AppendLine($"data: {msg.Data} more: {more}");
                return true;
            }
        }
        [Fact]
        public void KernelInfoReply_contract_has_not_been_broken()
        {
            throw new NotImplementedException();
        }
    }
    public class JupyterAdapterTests
    {
        [Fact]
        public void do_it()
        {
             // dispatch a jupyter execute request

             var executeRequest = new ExecuteRequest
             {
                 Code = "Console.WriteLine(12);"
             };

            throw new NotImplementedException();
        }
    }
}
