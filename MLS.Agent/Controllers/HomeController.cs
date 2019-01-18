using Microsoft.AspNetCore.Mvc;

namespace MLS.Agent.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("/")]
        public IActionResult Content()
        {
            return base.Content(Html(), "text/html");
        }

        private static string Html() =>
            @"<!DOCTYPE html>
<html lang=""en"">

<head>
    
    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
    <meta name=""robots"" content=""noindex"" />
    <meta http-equiv=""Content-Type"" content=""text/html;charset=utf-8"">
    <script src=""//trydotnet.microsoft.com/api/trydotnet.min.js""></script>
</head>

<body>
    <pre style=""border: none"" height=""300px"" width=""800px"" trydotnetMode=""editor"" projectTemplate=""console""
        trydotnetSessionId=""a"" height=""300px"" width=""800px"">
using System;

public class Program 
{
    public static void Main()
    {
        Console.WriteLine(""Put the code you want to run in a &lt;pre&gt; tag and go!"");
    }
}
    </pre>
    </br>
    <button trydotnetMode=""run"" trydotnetSessionId=""a"">Run</button>
    </br>
    <div trydotnetMode=""runResult"" trydotnetSessionId=""a""></div>
    <script nonce=""3Ylwe7FVSanYwwVoBKXA1WLjbN8vnTKFyv90yityOU4="">
        trydotnet.autoEnable(new URL(""http://localhost:4242/""));
    </script>
</body>

</html>

";
    }
}