using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using MLS.WasmCodeRunner;

namespace MLS.Blazor.Pages
{
    public class IndexModel : BlazorComponent
    {
        public IndexModel()
        {
        }

        public static Task<string> PostMessage(string message)
        {
            // Implemented in interop.js
            return JSRuntime.Current.InvokeAsync<string>(
                "BlazorInterop.postMessage",
                message);
        }

        [JSInvokable]
        public static async Task<bool> PostMessageAsync(string message)
        {
            var result = CodeRunner.ProcessCompileResult(message);
            if (result != null)
            {
                await PostMessage(JObject.FromObject(result).ToString());
            }

            return true;
        }
    }
}