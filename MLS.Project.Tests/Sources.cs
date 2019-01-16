using System;
using System.Collections.Generic;
using System.Text;

namespace MLS.Project.Tests
{
    internal class Sources
    {
        #region GistRawFile
        public static string GistRawFile => @"this is a raw file wonloaded because truncated";
        #endregion

        #region GistResponseSingleFile

        public static string GistResponseSingleFile => @"
{
  ""url"": ""https://api.github.com/gists/b98a2434b990d0963d9d7fe07ffa6765/2e1abef8d7e9ad63f767d0187be469ce9321969b"",
  ""forks_url"": ""https://api.github.com/gists/b98a2434b990d0963d9d7fe07ffa6765/forks"",
  ""commits_url"": ""https://api.github.com/gists/b98a2434b990d0963d9d7fe07ffa6765/commits"",
  ""id"": ""b98a2434b990d0963d9d7fe07ffa6765"",
  ""git_pull_url"": ""https://gist.github.com/b98a2434b990d0963d9d7fe07ffa6765.git"",
  ""git_push_url"": ""https://gist.github.com/b98a2434b990d0963d9d7fe07ffa6765.git"",
  ""html_url"": ""https://gist.github.com/b98a2434b990d0963d9d7fe07ffa6765"",
  ""files"": {
    ""Program.cs"": {
      ""filename"": ""Program.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/colombod/b98a2434b990d0963d9d7fe07ffa6765/raw/1ddc5595401364a0af575d7bf11868c634cede75/Program.cs"",
      ""size"": 651,
      ""truncated"": false,
      ""content"": ""using System;\nusing System.Linq;\nusing System.Collections.Generic;\n\nnamespace FibonacciTest\n{\n    public class Program\n    {\n        public static void Main()\n        {\n            foreach (var i in FibonacciGeneratorTwo.Fibonacci().Take(10))\n            {\n                Console.WriteLine(i);\n            }\n        }       \n    }\n    \n    internal static class FibonacciGeneratorTwo\n    {\n        public  static IEnumerable<int> Fibonacci()\n        {\n            int current = 1, next = 1;\n            while (true)\n            {\n                yield return current;\n                next = current + (current = next);\n            }\n        }\n    }\n}""
    }
  },
  ""public"": true,
  ""created_at"": ""2018-02-13T18:09:51Z"",
  ""updated_at"": ""2018-02-13T18:15:26Z"",
  ""description"": ""SingleFibonacci"",
  ""comments"": 0,
  ""user"": null,
  ""comments_url"": ""https://api.github.com/gists/b98a2434b990d0963d9d7fe07ffa6765/comments"",
  ""owner"": {
    ""login"": ""colombod"",
    ""id"": 375556,
    ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
    ""gravatar_id"": """",
    ""url"": ""https://api.github.com/users/colombod"",
    ""html_url"": ""https://github.com/colombod"",
    ""followers_url"": ""https://api.github.com/users/colombod/followers"",
    ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
    ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
    ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
    ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
    ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
    ""repos_url"": ""https://api.github.com/users/colombod/repos"",
    ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
    ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
    ""type"": ""User"",
    ""site_admin"": false
  },
  ""forks"": [

  ],
  ""history"": [
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""2e1abef8d7e9ad63f767d0187be469ce9321969b"",
      ""committed_at"": ""2018-02-13T18:15:25Z"",
      ""change_status"": {
        ""total"": 0,
        ""additions"": 0,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/b98a2434b990d0963d9d7fe07ffa6765/2e1abef8d7e9ad63f767d0187be469ce9321969b""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""8e9309346a56dac0c7fc66dfa81dcdfe6404394b"",
      ""committed_at"": ""2018-02-13T18:14:19Z"",
      ""change_status"": {
        ""total"": 5,
        ""additions"": 2,
        ""deletions"": 3
      },
      ""url"": ""https://api.github.com/gists/b98a2434b990d0963d9d7fe07ffa6765/8e9309346a56dac0c7fc66dfa81dcdfe6404394b""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""0efc34eb1c0ea19e5306eaf41d6e85adbd6ed4c5"",
      ""committed_at"": ""2018-02-13T18:11:23Z"",
      ""change_status"": {
        ""total"": 1,
        ""additions"": 1,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/b98a2434b990d0963d9d7fe07ffa6765/0efc34eb1c0ea19e5306eaf41d6e85adbd6ed4c5""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""5bafb4f469c01a6a6cb26b7ed094fe6426cbaaa9"",
      ""committed_at"": ""2018-02-13T18:09:51Z"",
      ""change_status"": {
        ""total"": 30,
        ""additions"": 30,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/b98a2434b990d0963d9d7fe07ffa6765/5bafb4f469c01a6a6cb26b7ed094fe6426cbaaa9""
    }
  ],
  ""truncated"": false
}
";
        #endregion

        #region GistResponse2Files
        public static string GistResponse2Files => @"
{
  ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/48d51b3362ebdefecb64a2986278439abc608bb8"",
  ""forks_url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/forks"",
  ""commits_url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/commits"",
  ""id"": ""3d5c3795a58b3e9345e44b5a4541a9c7"",
  ""git_pull_url"": ""https://gist.github.com/3d5c3795a58b3e9345e44b5a4541a9c7.git"",
  ""git_push_url"": ""https://gist.github.com/3d5c3795a58b3e9345e44b5a4541a9c7.git"",
  ""html_url"": ""https://gist.github.com/3d5c3795a58b3e9345e44b5a4541a9c7"",
  ""files"": {
    ""Program.cs"": {
      ""filename"": ""Program.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/colombod/3d5c3795a58b3e9345e44b5a4541a9c7/raw/894dbbd89a23bcad1d997b1bbe386a246a0f8c95/Program.cs"",
      ""size"": 628,
      ""truncated"": false,
      ""content"": ""using System;\nusing Newtonsoft.Json;\nusing Newtonsoft.Json.Serialization;\nusing Newtonsoft.Json.Converters;\nusing Newtonsoft.Json.Linq;\n\nnamespace jsonDotNetExperiment\n{\n    class Program\n    {\n        static void Main(string[] args)\n        {\n            Console.WriteLine(\""jsonDotNet workspace\"");\n            #region jsonSnippet\n            var simpleObject = new JObject\n            {\n                {\""property\"", 4}\n            };\n            Console.WriteLine(simpleObject.ToString(Formatting.Indented));\n            #endregion\n            Console.WriteLine(\""Bye!\"");\n            Console.WriteLine(\""Bye!\"");\n        }\n    }\n}""
    },
    ""secondFile.cs"": {
      ""filename"": ""secondFile.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/colombod/3d5c3795a58b3e9345e44b5a4541a9c7/raw/ed8c5a7f32f00ac26697f0c6dec648651d8c2aeb/secondFile.cs"",
      ""size"": 631,
      ""truncated"": false,
      ""content"": ""using System;\nusing Newtonsoft.Json;\nusing Newtonsoft.Json.Serialization;\nusing Newtonsoft.Json.Converters;\nusing Newtonsoft.Json.Linq;\n\nnamespace jsonDotNetExperiment\n{\n    class ProgramTwo\n    {\n        static void Main(string[] args)\n        {\n            Console.WriteLine(\""jsonDotNet workspace\"");\n            #region jsonSnippet\n            var simpleObject = new JObject\n            {\n                {\""property\"", 4}\n            };\n            Console.WriteLine(simpleObject.ToString(Formatting.Indented));\n            #endregion\n            Console.WriteLine(\""Bye!\"");\n            Console.WriteLine(\""Bye!\"");\n        }\n    }\n}""
    }
  },
  ""public"": true,
  ""created_at"": ""2018-02-07T12:51:43Z"",
  ""updated_at"": ""2018-02-09T16:35:03Z"",
  ""description"": ""JsonDotNet Api"",
  ""comments"": 0,
  ""user"": null,
  ""comments_url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/comments"",
  ""owner"": {
    ""login"": ""colombod"",
    ""id"": 375556,
    ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
    ""gravatar_id"": """",
    ""url"": ""https://api.github.com/users/colombod"",
    ""html_url"": ""https://github.com/colombod"",
    ""followers_url"": ""https://api.github.com/users/colombod/followers"",
    ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
    ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
    ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
    ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
    ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
    ""repos_url"": ""https://api.github.com/users/colombod/repos"",
    ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
    ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
    ""type"": ""User"",
    ""site_admin"": false
  },
  ""forks"": [

  ],
  ""history"": [
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""48d51b3362ebdefecb64a2986278439abc608bb8"",
      ""committed_at"": ""2018-02-09T15:56:01Z"",
      ""change_status"": {
        ""total"": 25,
        ""additions"": 25,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/48d51b3362ebdefecb64a2986278439abc608bb8""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""cc99b1353788707a3a6350bc2fa613fc1882676a"",
      ""committed_at"": ""2018-02-07T20:24:08Z"",
      ""change_status"": {
        ""total"": 1,
        ""additions"": 1,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/cc99b1353788707a3a6350bc2fa613fc1882676a""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""55918631e6e63669967fcc5b467efd57bf036eb9"",
      ""committed_at"": ""2018-02-07T12:51:42Z"",
      ""change_status"": {
        ""total"": 24,
        ""additions"": 24,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/55918631e6e63669967fcc5b467efd57bf036eb9""
    }
  ],
  ""truncated"": false
}";
        #endregion

        #region GistResponse3Files

        public static string GistResponse3Files = @"

{
  ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/d1b537520d812de49ae5639ca487d3e99304a488"",
  ""forks_url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/forks"",
  ""commits_url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/commits"",
  ""id"": ""3d5c3795a58b3e9345e44b5a4541a9c7"",
  ""git_pull_url"": ""https://gist.github.com/3d5c3795a58b3e9345e44b5a4541a9c7.git"",
  ""git_push_url"": ""https://gist.github.com/3d5c3795a58b3e9345e44b5a4541a9c7.git"",
  ""html_url"": ""https://gist.github.com/3d5c3795a58b3e9345e44b5a4541a9c7"",
  ""files"": {
    ""Program.cs"": {
      ""filename"": ""Program.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/colombod/3d5c3795a58b3e9345e44b5a4541a9c7/raw/894dbbd89a23bcad1d997b1bbe386a246a0f8c95/Program.cs"",
      ""size"": 628,
      ""truncated"": false,
      ""content"": ""using System;\nusing Newtonsoft.Json;\nusing Newtonsoft.Json.Serialization;\nusing Newtonsoft.Json.Converters;\nusing Newtonsoft.Json.Linq;\n\nnamespace jsonDotNetExperiment\n{\n    class Program\n    {\n        static void Main(string[] args)\n        {\n            Console.WriteLine(\""jsonDotNet workspace\"");\n            #region jsonSnippet\n            var simpleObject = new JObject\n            {\n                {\""property\"", 4}\n            };\n            Console.WriteLine(simpleObject.ToString(Formatting.Indented));\n            #endregion\n            Console.WriteLine(\""Bye!\"");\n            Console.WriteLine(\""Bye!\"");\n        }\n    }\n}""
    },
    ""secondFile.cs"": {
      ""filename"": ""secondFile.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/colombod/3d5c3795a58b3e9345e44b5a4541a9c7/raw/ed8c5a7f32f00ac26697f0c6dec648651d8c2aeb/secondFile.cs"",
      ""size"": 631,
      ""truncated"": false,
      ""content"": ""using System;\nusing Newtonsoft.Json;\nusing Newtonsoft.Json.Serialization;\nusing Newtonsoft.Json.Converters;\nusing Newtonsoft.Json.Linq;\n\nnamespace jsonDotNetExperiment\n{\n    class ProgramTwo\n    {\n        static void Main(string[] args)\n        {\n            Console.WriteLine(\""jsonDotNet workspace\"");\n            #region jsonSnippet\n            var simpleObject = new JObject\n            {\n                {\""property\"", 4}\n            };\n            Console.WriteLine(simpleObject.ToString(Formatting.Indented));\n            #endregion\n            Console.WriteLine(\""Bye!\"");\n            Console.WriteLine(\""Bye!\"");\n        }\n    }\n}""
    },
    ""thirdFile.cs"": {
      ""filename"": ""thirdFile.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/colombod/3d5c3795a58b3e9345e44b5a4541a9c7/raw/6f6e2fe5d0b6a26db0071eac3ac79d57d24f7029/thirdFile.cs"",
      ""size"": 633,
      ""truncated"": false,
      ""content"": ""using System;\nusing Newtonsoft.Json;\nusing Newtonsoft.Json.Serialization;\nusing Newtonsoft.Json.Converters;\nusing Newtonsoft.Json.Linq;\n\nnamespace jsonDotNetExperiment\n{\n    class ProgramThree\n    {\n        static void Main(string[] args)\n        {\n            Console.WriteLine(\""jsonDotNet workspace\"");\n            #region jsonSnippet\n            var simpleObject = new JObject\n            {\n                {\""property\"", 4}\n            };\n            Console.WriteLine(simpleObject.ToString(Formatting.Indented));\n            #endregion\n            Console.WriteLine(\""Bye!\"");\n            Console.WriteLine(\""Bye!\"");\n        }\n    }\n}""
    }
  },
  ""public"": true,
  ""created_at"": ""2018-02-07T12:51:43Z"",
  ""updated_at"": ""2018-02-09T16:35:03Z"",
  ""description"": ""JsonDotNet Api"",
  ""comments"": 0,
  ""user"": null,
  ""comments_url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/comments"",
  ""owner"": {
    ""login"": ""colombod"",
    ""id"": 375556,
    ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
    ""gravatar_id"": """",
    ""url"": ""https://api.github.com/users/colombod"",
    ""html_url"": ""https://github.com/colombod"",
    ""followers_url"": ""https://api.github.com/users/colombod/followers"",
    ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
    ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
    ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
    ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
    ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
    ""repos_url"": ""https://api.github.com/users/colombod/repos"",
    ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
    ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
    ""type"": ""User"",
    ""site_admin"": false
  },
  ""forks"": [

  ],
  ""history"": [
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""d1b537520d812de49ae5639ca487d3e99304a488"",
      ""committed_at"": ""2018-02-09T16:35:02Z"",
      ""change_status"": {
        ""total"": 25,
        ""additions"": 25,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/d1b537520d812de49ae5639ca487d3e99304a488""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""48d51b3362ebdefecb64a2986278439abc608bb8"",
      ""committed_at"": ""2018-02-09T15:56:01Z"",
      ""change_status"": {
        ""total"": 25,
        ""additions"": 25,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/48d51b3362ebdefecb64a2986278439abc608bb8""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""cc99b1353788707a3a6350bc2fa613fc1882676a"",
      ""committed_at"": ""2018-02-07T20:24:08Z"",
      ""change_status"": {
        ""total"": 1,
        ""additions"": 1,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/cc99b1353788707a3a6350bc2fa613fc1882676a""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""55918631e6e63669967fcc5b467efd57bf036eb9"",
      ""committed_at"": ""2018-02-07T12:51:42Z"",
      ""change_status"": {
        ""total"": 24,
        ""additions"": 24,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/55918631e6e63669967fcc5b467efd57bf036eb9""
    }
  ],
  ""truncated"": false
}
";

        #endregion

        #region GistResponseForLatest

        public static string GistResponseForLatest => @"

{
  ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7"",
  ""forks_url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/forks"",
  ""commits_url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/commits"",
  ""id"": ""3d5c3795a58b3e9345e44b5a4541a9c7"",
  ""git_pull_url"": ""https://gist.github.com/3d5c3795a58b3e9345e44b5a4541a9c7.git"",
  ""git_push_url"": ""https://gist.github.com/3d5c3795a58b3e9345e44b5a4541a9c7.git"",
  ""html_url"": ""https://gist.github.com/3d5c3795a58b3e9345e44b5a4541a9c7"",
  ""files"": {
    ""Program.cs"": {
      ""filename"": ""Program.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/colombod/3d5c3795a58b3e9345e44b5a4541a9c7/raw/894dbbd89a23bcad1d997b1bbe386a246a0f8c95/Program.cs"",
      ""size"": 628,
      ""truncated"": false,
      ""content"": ""using System;\nusing Newtonsoft.Json;\nusing Newtonsoft.Json.Serialization;\nusing Newtonsoft.Json.Converters;\nusing Newtonsoft.Json.Linq;\n\nnamespace jsonDotNetExperiment\n{\n    class Program\n    {\n        static void Main(string[] args)\n        {\n            Console.WriteLine(\""jsonDotNet workspace\"");\n            #region jsonSnippet\n            var simpleObject = new JObject\n            {\n                {\""property\"", 4}\n            };\n            Console.WriteLine(simpleObject.ToString(Formatting.Indented));\n            #endregion\n            Console.WriteLine(\""Bye!\"");\n            Console.WriteLine(\""Bye!\"");\n        }\n    }\n}""
    },
    ""secondFile.cs"": {
      ""filename"": ""secondFile.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/colombod/3d5c3795a58b3e9345e44b5a4541a9c7/raw/ed8c5a7f32f00ac26697f0c6dec648651d8c2aeb/secondFile.cs"",
      ""size"": 631,
      ""truncated"": false,
      ""content"": ""using System;\nusing Newtonsoft.Json;\nusing Newtonsoft.Json.Serialization;\nusing Newtonsoft.Json.Converters;\nusing Newtonsoft.Json.Linq;\n\nnamespace jsonDotNetExperiment\n{\n    class ProgramTwo\n    {\n        static void Main(string[] args)\n        {\n            Console.WriteLine(\""jsonDotNet workspace\"");\n            #region jsonSnippet\n            var simpleObject = new JObject\n            {\n                {\""property\"", 4}\n            };\n            Console.WriteLine(simpleObject.ToString(Formatting.Indented));\n            #endregion\n            Console.WriteLine(\""Bye!\"");\n            Console.WriteLine(\""Bye!\"");\n        }\n    }\n}""
    },
    ""thirdFile.cs"": {
      ""filename"": ""thirdFile.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/colombod/3d5c3795a58b3e9345e44b5a4541a9c7/raw/6f6e2fe5d0b6a26db0071eac3ac79d57d24f7029/thirdFile.cs"",
      ""size"": 633,
      ""truncated"": false,
      ""content"": ""using System;\nusing Newtonsoft.Json;\nusing Newtonsoft.Json.Serialization;\nusing Newtonsoft.Json.Converters;\nusing Newtonsoft.Json.Linq;\n\nnamespace jsonDotNetExperiment\n{\n    class ProgramThree\n    {\n        static void Main(string[] args)\n        {\n            Console.WriteLine(\""jsonDotNet workspace\"");\n            #region jsonSnippet\n            var simpleObject = new JObject\n            {\n                {\""property\"", 4}\n            };\n            Console.WriteLine(simpleObject.ToString(Formatting.Indented));\n            #endregion\n            Console.WriteLine(\""Bye!\"");\n            Console.WriteLine(\""Bye!\"");\n        }\n    }\n}""
    }
  },
  ""public"": true,
  ""created_at"": ""2018-02-07T12:51:43Z"",
  ""updated_at"": ""2018-02-09T16:35:03Z"",
  ""description"": ""JsonDotNet Api"",
  ""comments"": 0,
  ""user"": null,
  ""comments_url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/comments"",
  ""owner"": {
    ""login"": ""colombod"",
    ""id"": 375556,
    ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
    ""gravatar_id"": """",
    ""url"": ""https://api.github.com/users/colombod"",
    ""html_url"": ""https://github.com/colombod"",
    ""followers_url"": ""https://api.github.com/users/colombod/followers"",
    ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
    ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
    ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
    ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
    ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
    ""repos_url"": ""https://api.github.com/users/colombod/repos"",
    ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
    ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
    ""type"": ""User"",
    ""site_admin"": false
  },
  ""forks"": [

  ],
  ""history"": [
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""d1b537520d812de49ae5639ca487d3e99304a488"",
      ""committed_at"": ""2018-02-09T16:35:02Z"",
      ""change_status"": {
        ""total"": 25,
        ""additions"": 25,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/d1b537520d812de49ae5639ca487d3e99304a488""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""48d51b3362ebdefecb64a2986278439abc608bb8"",
      ""committed_at"": ""2018-02-09T15:56:01Z"",
      ""change_status"": {
        ""total"": 25,
        ""additions"": 25,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/48d51b3362ebdefecb64a2986278439abc608bb8""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""cc99b1353788707a3a6350bc2fa613fc1882676a"",
      ""committed_at"": ""2018-02-07T20:24:08Z"",
      ""change_status"": {
        ""total"": 1,
        ""additions"": 1,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/cc99b1353788707a3a6350bc2fa613fc1882676a""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""55918631e6e63669967fcc5b467efd57bf036eb9"",
      ""committed_at"": ""2018-02-07T12:51:42Z"",
      ""change_status"": {
        ""total"": 24,
        ""additions"": 24,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/55918631e6e63669967fcc5b467efd57bf036eb9""
    }
  ],
  ""truncated"": false
}
";

        #endregion

        #region GistResponseWithTruncatedFiles

        public static string GistResponseWithTruncatedFiles = @"

{
  ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/d1b537520d812de49ae5639ca487d3e99304a488"",
  ""forks_url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/forks"",
  ""commits_url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/commits"",
  ""id"": ""3d5c3795a58b3e9345e44b5a4541a9c7"",
  ""git_pull_url"": ""https://gist.github.com/3d5c3795a58b3e9345e44b5a4541a9c7.git"",
  ""git_push_url"": ""https://gist.github.com/3d5c3795a58b3e9345e44b5a4541a9c7.git"",
  ""html_url"": ""https://gist.github.com/3d5c3795a58b3e9345e44b5a4541a9c7"",
  ""files"": {
    ""Program.cs"": {
      ""filename"": ""Program.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/colombod/3d5c3795a58b3e9345e44b5a4541a9c7/raw/894dbbd89a23bcad1d997b1bbe386a246a0f8c95/Program.cs"",
      ""size"": 628,
      ""truncated"": true,
      ""content"": """"
    },
    ""secondFile.cs"": {
      ""filename"": ""secondFile.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/colombod/3d5c3795a58b3e9345e44b5a4541a9c7/raw/ed8c5a7f32f00ac26697f0c6dec648651d8c2aeb/secondFile.cs"",
      ""size"": 631,
      ""truncated"": true,
      ""content"": """"
    },
    ""thirdFile.cs"": {
      ""filename"": ""thirdFile.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/colombod/3d5c3795a58b3e9345e44b5a4541a9c7/raw/6f6e2fe5d0b6a26db0071eac3ac79d57d24f7029/thirdFile.cs"",
      ""size"": 633,
      ""truncated"": true,
      ""content"": """"
    }
  },
  ""public"": true,
  ""created_at"": ""2018-02-07T12:51:43Z"",
  ""updated_at"": ""2018-02-09T16:35:03Z"",
  ""description"": ""JsonDotNet Api"",
  ""comments"": 0,
  ""user"": null,
  ""comments_url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/comments"",
  ""owner"": {
    ""login"": ""colombod"",
    ""id"": 375556,
    ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
    ""gravatar_id"": """",
    ""url"": ""https://api.github.com/users/colombod"",
    ""html_url"": ""https://github.com/colombod"",
    ""followers_url"": ""https://api.github.com/users/colombod/followers"",
    ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
    ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
    ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
    ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
    ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
    ""repos_url"": ""https://api.github.com/users/colombod/repos"",
    ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
    ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
    ""type"": ""User"",
    ""site_admin"": false
  },
  ""forks"": [

  ],
  ""history"": [
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""d1b537520d812de49ae5639ca487d3e99304a488"",
      ""committed_at"": ""2018-02-09T16:35:02Z"",
      ""change_status"": {
        ""total"": 25,
        ""additions"": 25,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/d1b537520d812de49ae5639ca487d3e99304a488""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""48d51b3362ebdefecb64a2986278439abc608bb8"",
      ""committed_at"": ""2018-02-09T15:56:01Z"",
      ""change_status"": {
        ""total"": 25,
        ""additions"": 25,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/48d51b3362ebdefecb64a2986278439abc608bb8""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""cc99b1353788707a3a6350bc2fa613fc1882676a"",
      ""committed_at"": ""2018-02-07T20:24:08Z"",
      ""change_status"": {
        ""total"": 1,
        ""additions"": 1,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/cc99b1353788707a3a6350bc2fa613fc1882676a""
    },
    {
      ""user"": {
        ""login"": ""colombod"",
        ""id"": 375556,
        ""avatar_url"": ""https://avatars1.githubusercontent.com/u/375556?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/colombod"",
        ""html_url"": ""https://github.com/colombod"",
        ""followers_url"": ""https://api.github.com/users/colombod/followers"",
        ""following_url"": ""https://api.github.com/users/colombod/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/colombod/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/colombod/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/colombod/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/colombod/orgs"",
        ""repos_url"": ""https://api.github.com/users/colombod/repos"",
        ""events_url"": ""https://api.github.com/users/colombod/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/colombod/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""55918631e6e63669967fcc5b467efd57bf036eb9"",
      ""committed_at"": ""2018-02-07T12:51:42Z"",
      ""change_status"": {
        ""total"": 24,
        ""additions"": 24,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/3d5c3795a58b3e9345e44b5a4541a9c7/55918631e6e63669967fcc5b467efd57bf036eb9""
    }
  ],
  ""truncated"": false
}
";

        #endregion

        #region jsonDotNetBuffer

        public static string jsonDotNetBuffer = @"
var simpleObject = new JObject
{
    {""property"", 4}
};

Console.WriteLine(simpleObject.ToString(Formatting.Indented));";

        #endregion

        #region jsonDotNetFile

        public static string jsonDotNetFile = @"
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace jsonDotNetExperiment
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""jsonDotNet workspace"");
            #region jsonSnippet
            #endregion
            Console.WriteLine(""Bye!"");
        }
    }
}
";

        #endregion

        #region jsonDotNetFullCode

        public static string jsonDotNetFullCode = @"
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace jsonDotNetExperiment
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""jsonDotNet workspace"");
            #region jsonSnippet
            var simpleObject = new JObject
            {
                {""property"", 4}
            };
            Console.WriteLine(simpleObject.ToString(Formatting.Indented));
            #endregion
            Console.WriteLine(""Bye!"");
        }
    }
}";

        #endregion

        #region CodeWithNoRegions
        public static string CodeWithNoRegions = @"
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace jsonDotNetExperiment
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(""jsonDotNet workspace"");
            var simpleObject = new JObject
            {
                {""property"", 4}
            };
            Console.WriteLine(simpleObject.ToString(Formatting.Indented));
            Console.WriteLine(""Bye!"");
        }
    }
}";
        #endregion

        #region CodeWithTwoRegions
        public static string CodeWithTwoRegions = @"
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace jsonDotNetExperiment
{
    class Program
    {
        static void Main(string[] args)
        {
            #region workspaceIdentifier
            Console.WriteLine(""jsonDotNet workspace"");
            #endregion
            #region objetConstruction
            var simpleObject = new JObject
            {
                {""property"", 4}
            };
            #endregion
            Console.WriteLine(simpleObject.ToString(Formatting.Indented));
            Console.WriteLine(""Bye!"");
        }
    }
}";
        #endregion

        public static string GistWithRegion = @"using System;
using NodaTime;

namespace TryNodaTime
{
    class Program
    {  
        static void Main(string[] args)
        {
            #region fragment
            // Instant represents time from epoch
            Instant now = SystemClock.Instance.GetCurrentInstant();
            Console.WriteLine($""now: {now}"");

            // Convert an instant to a ZonedDateTime
            ZonedDateTime nowInIsoUtc = now.InUtc();
            Console.WriteLine($""nowInIsoUtc: {nowInIsoUtc}"");

            // Create a duration
            Duration duration = Duration.FromMinutes(3);
            Console.WriteLine($""duration: {duration}"");

            // Add it to our ZonedDateTime
            ZonedDateTime thenInIsoUtc = nowInIsoUtc + duration;
            Console.WriteLine($""thenInIsoUtc: {thenInIsoUtc}"");

            // Time zone support (multiple providers)
            var london = DateTimeZoneProviders.Tzdb[""Europe/London""];
            Console.WriteLine($""london: {london}"");

            // Time zone conversions
            var localDate = new LocalDateTime(2012, 3, 27, 0, 45, 00);
            var before = london.AtStrictly(localDate);
            Console.WriteLine($""before: {before}"");
            #endregion
        }
    }
}";

        #region TinkeringGist

        public static string TinkeringGist = @"
{
  ""url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36"",
  ""forks_url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36/forks"",
  ""commits_url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36/commits"",
  ""id"": ""2a42bde115c1c897a3ffc98256e08e36"",
  ""git_pull_url"": ""https://gist.github.com/2a42bde115c1c897a3ffc98256e08e36.git"",
  ""git_push_url"": ""https://gist.github.com/2a42bde115c1c897a3ffc98256e08e36.git"",
  ""html_url"": ""https://gist.github.com/2a42bde115c1c897a3ffc98256e08e36"",
  ""files"": {
    ""Program.cs"": {
      ""filename"": ""Program.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/jonsequitur/2a42bde115c1c897a3ffc98256e08e36/raw/3a0bfb849c7edfbd1e8191933e7fcaa9a86f080f/Program.cs"",
      ""size"": 8,
      ""truncated"": false,
      ""content"": ""// hmmmm""
    },
    ""tinkering.cs"": {
      ""filename"": ""tinkering.cs"",
      ""type"": ""text/plain"",
      ""language"": ""C#"",
      ""raw_url"": ""https://gist.githubusercontent.com/jonsequitur/2a42bde115c1c897a3ffc98256e08e36/raw/a577ed1cc616e26261980af91afaf1ecd8fd13e9/tinkering.cs"",
      ""size"": 724,
      ""truncated"": false,
      ""content"": ""using System;\nusing System.Net.Http;\nusing Newtonsoft.Json;\nusing Newtonsoft.Json.Linq;\nusing System.Threading.Tasks;\nusing System.Net.Http.Headers;\n\npublic static class Program\n{\n  public static void Main()\n  {\n    #region fragment\n    var httpClient = new HttpClient();\n\n    var uri = \""https://gist.githubusercontent.com/jonsequitur/2a42bde115c1c897a3ffc98256e08e36/raw/2dabd05bad72c5bc4ea4d391a348faf13cea53d6/tinkering.cs\"";\n    \n    var request = new HttpRequestMessage(HttpMethod.Get, uri);\n\n    request.Headers.UserAgent.Add(ProductInfoHeaderValue.Parse(\""jonsequitur\""));\n\n    var response = httpClient.SendAsync(request).Result;\n\n    Console.WriteLine(response.Content.ReadAsStringAsync().Result);\n    #endregion\n  }\n}""
    }
  },
  ""public"": true,
  ""created_at"": ""2018-02-22T23:57:35Z"",
  ""updated_at"": ""2018-02-23T00:51:05Z"",
  ""description"": """",
  ""comments"": 0,
  ""user"": null,
  ""comments_url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36/comments"",
  ""owner"": {
    ""login"": ""jonsequitur"",
    ""id"": 547415,
    ""avatar_url"": ""https://avatars0.githubusercontent.com/u/547415?v=4"",
    ""gravatar_id"": """",
    ""url"": ""https://api.github.com/users/jonsequitur"",
    ""html_url"": ""https://github.com/jonsequitur"",
    ""followers_url"": ""https://api.github.com/users/jonsequitur/followers"",
    ""following_url"": ""https://api.github.com/users/jonsequitur/following{/other_user}"",
    ""gists_url"": ""https://api.github.com/users/jonsequitur/gists{/gist_id}"",
    ""starred_url"": ""https://api.github.com/users/jonsequitur/starred{/owner}{/repo}"",
    ""subscriptions_url"": ""https://api.github.com/users/jonsequitur/subscriptions"",
    ""organizations_url"": ""https://api.github.com/users/jonsequitur/orgs"",
    ""repos_url"": ""https://api.github.com/users/jonsequitur/repos"",
    ""events_url"": ""https://api.github.com/users/jonsequitur/events{/privacy}"",
    ""received_events_url"": ""https://api.github.com/users/jonsequitur/received_events"",
    ""type"": ""User"",
    ""site_admin"": false
  },
  ""forks"": [

  ],
  ""history"": [
    {
      ""user"": {
        ""login"": ""jonsequitur"",
        ""id"": 547415,
        ""avatar_url"": ""https://avatars0.githubusercontent.com/u/547415?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/jonsequitur"",
        ""html_url"": ""https://github.com/jonsequitur"",
        ""followers_url"": ""https://api.github.com/users/jonsequitur/followers"",
        ""following_url"": ""https://api.github.com/users/jonsequitur/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/jonsequitur/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/jonsequitur/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/jonsequitur/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/jonsequitur/orgs"",
        ""repos_url"": ""https://api.github.com/users/jonsequitur/repos"",
        ""events_url"": ""https://api.github.com/users/jonsequitur/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/jonsequitur/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""82c75cdcac4a20d680383c88d8260604a8f5fcbd"",
      ""committed_at"": ""2018-02-23T00:51:04Z"",
      ""change_status"": {
        ""total"": 2,
        ""additions"": 1,
        ""deletions"": 1
      },
      ""url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36/82c75cdcac4a20d680383c88d8260604a8f5fcbd""
    },
    {
      ""user"": {
        ""login"": ""jonsequitur"",
        ""id"": 547415,
        ""avatar_url"": ""https://avatars0.githubusercontent.com/u/547415?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/jonsequitur"",
        ""html_url"": ""https://github.com/jonsequitur"",
        ""followers_url"": ""https://api.github.com/users/jonsequitur/followers"",
        ""following_url"": ""https://api.github.com/users/jonsequitur/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/jonsequitur/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/jonsequitur/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/jonsequitur/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/jonsequitur/orgs"",
        ""repos_url"": ""https://api.github.com/users/jonsequitur/repos"",
        ""events_url"": ""https://api.github.com/users/jonsequitur/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/jonsequitur/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""2dabd05bad72c5bc4ea4d391a348faf13cea53d6"",
      ""committed_at"": ""2018-02-23T00:50:32Z"",
      ""change_status"": {
        ""total"": 2,
        ""additions"": 1,
        ""deletions"": 1
      },
      ""url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36/2dabd05bad72c5bc4ea4d391a348faf13cea53d6""
    },
    {
      ""user"": {
        ""login"": ""jonsequitur"",
        ""id"": 547415,
        ""avatar_url"": ""https://avatars0.githubusercontent.com/u/547415?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/jonsequitur"",
        ""html_url"": ""https://github.com/jonsequitur"",
        ""followers_url"": ""https://api.github.com/users/jonsequitur/followers"",
        ""following_url"": ""https://api.github.com/users/jonsequitur/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/jonsequitur/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/jonsequitur/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/jonsequitur/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/jonsequitur/orgs"",
        ""repos_url"": ""https://api.github.com/users/jonsequitur/repos"",
        ""events_url"": ""https://api.github.com/users/jonsequitur/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/jonsequitur/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""acad25b27f19a287cd0588fc2ad11f04457f8440"",
      ""committed_at"": ""2018-02-23T00:38:40Z"",
      ""change_status"": {
        ""total"": 2,
        ""additions"": 1,
        ""deletions"": 1
      },
      ""url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36/acad25b27f19a287cd0588fc2ad11f04457f8440""
    },
    {
      ""user"": {
        ""login"": ""jonsequitur"",
        ""id"": 547415,
        ""avatar_url"": ""https://avatars0.githubusercontent.com/u/547415?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/jonsequitur"",
        ""html_url"": ""https://github.com/jonsequitur"",
        ""followers_url"": ""https://api.github.com/users/jonsequitur/followers"",
        ""following_url"": ""https://api.github.com/users/jonsequitur/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/jonsequitur/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/jonsequitur/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/jonsequitur/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/jonsequitur/orgs"",
        ""repos_url"": ""https://api.github.com/users/jonsequitur/repos"",
        ""events_url"": ""https://api.github.com/users/jonsequitur/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/jonsequitur/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""0e79023692792c8dc9a664a6823189f1cf572641"",
      ""committed_at"": ""2018-02-23T00:37:06Z"",
      ""change_status"": {
        ""total"": 7,
        ""additions"": 5,
        ""deletions"": 2
      },
      ""url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36/0e79023692792c8dc9a664a6823189f1cf572641""
    },
    {
      ""user"": {
        ""login"": ""jonsequitur"",
        ""id"": 547415,
        ""avatar_url"": ""https://avatars0.githubusercontent.com/u/547415?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/jonsequitur"",
        ""html_url"": ""https://github.com/jonsequitur"",
        ""followers_url"": ""https://api.github.com/users/jonsequitur/followers"",
        ""following_url"": ""https://api.github.com/users/jonsequitur/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/jonsequitur/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/jonsequitur/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/jonsequitur/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/jonsequitur/orgs"",
        ""repos_url"": ""https://api.github.com/users/jonsequitur/repos"",
        ""events_url"": ""https://api.github.com/users/jonsequitur/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/jonsequitur/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""182d0bbf06cbb9cea590fe1a0454d633bf8ab116"",
      ""committed_at"": ""2018-02-23T00:32:52Z"",
      ""change_status"": {
        ""total"": 3,
        ""additions"": 2,
        ""deletions"": 1
      },
      ""url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36/182d0bbf06cbb9cea590fe1a0454d633bf8ab116""
    },
    {
      ""user"": {
        ""login"": ""jonsequitur"",
        ""id"": 547415,
        ""avatar_url"": ""https://avatars0.githubusercontent.com/u/547415?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/jonsequitur"",
        ""html_url"": ""https://github.com/jonsequitur"",
        ""followers_url"": ""https://api.github.com/users/jonsequitur/followers"",
        ""following_url"": ""https://api.github.com/users/jonsequitur/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/jonsequitur/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/jonsequitur/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/jonsequitur/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/jonsequitur/orgs"",
        ""repos_url"": ""https://api.github.com/users/jonsequitur/repos"",
        ""events_url"": ""https://api.github.com/users/jonsequitur/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/jonsequitur/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""8efdd1413b5479d901efac7c2bb3cfed02f5d104"",
      ""committed_at"": ""2018-02-23T00:32:07Z"",
      ""change_status"": {
        ""total"": 15,
        ""additions"": 13,
        ""deletions"": 2
      },
      ""url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36/8efdd1413b5479d901efac7c2bb3cfed02f5d104""
    },
    {
      ""user"": {
        ""login"": ""jonsequitur"",
        ""id"": 547415,
        ""avatar_url"": ""https://avatars0.githubusercontent.com/u/547415?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/jonsequitur"",
        ""html_url"": ""https://github.com/jonsequitur"",
        ""followers_url"": ""https://api.github.com/users/jonsequitur/followers"",
        ""following_url"": ""https://api.github.com/users/jonsequitur/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/jonsequitur/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/jonsequitur/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/jonsequitur/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/jonsequitur/orgs"",
        ""repos_url"": ""https://api.github.com/users/jonsequitur/repos"",
        ""events_url"": ""https://api.github.com/users/jonsequitur/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/jonsequitur/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""f25903f15bd0db6e0af516dfc02442d1123de538"",
      ""committed_at"": ""2018-02-23T00:17:23Z"",
      ""change_status"": {
        ""total"": 0,
        ""additions"": 0,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36/f25903f15bd0db6e0af516dfc02442d1123de538""
    },
    {
      ""user"": {
        ""login"": ""jonsequitur"",
        ""id"": 547415,
        ""avatar_url"": ""https://avatars0.githubusercontent.com/u/547415?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/jonsequitur"",
        ""html_url"": ""https://github.com/jonsequitur"",
        ""followers_url"": ""https://api.github.com/users/jonsequitur/followers"",
        ""following_url"": ""https://api.github.com/users/jonsequitur/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/jonsequitur/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/jonsequitur/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/jonsequitur/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/jonsequitur/orgs"",
        ""repos_url"": ""https://api.github.com/users/jonsequitur/repos"",
        ""events_url"": ""https://api.github.com/users/jonsequitur/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/jonsequitur/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""bb1436060c24278a416d911ada5bd470b9c328c5"",
      ""committed_at"": ""2018-02-23T00:15:39Z"",
      ""change_status"": {
        ""total"": 0,
        ""additions"": 0,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36/bb1436060c24278a416d911ada5bd470b9c328c5""
    },
    {
      ""user"": {
        ""login"": ""jonsequitur"",
        ""id"": 547415,
        ""avatar_url"": ""https://avatars0.githubusercontent.com/u/547415?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/jonsequitur"",
        ""html_url"": ""https://github.com/jonsequitur"",
        ""followers_url"": ""https://api.github.com/users/jonsequitur/followers"",
        ""following_url"": ""https://api.github.com/users/jonsequitur/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/jonsequitur/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/jonsequitur/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/jonsequitur/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/jonsequitur/orgs"",
        ""repos_url"": ""https://api.github.com/users/jonsequitur/repos"",
        ""events_url"": ""https://api.github.com/users/jonsequitur/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/jonsequitur/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""07db03440b9eebcb9c9b0d1d840e9fd2ae52149a"",
      ""committed_at"": ""2018-02-23T00:10:00Z"",
      ""change_status"": {
        ""total"": 1,
        ""additions"": 1,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36/07db03440b9eebcb9c9b0d1d840e9fd2ae52149a""
    },
    {
      ""user"": {
        ""login"": ""jonsequitur"",
        ""id"": 547415,
        ""avatar_url"": ""https://avatars0.githubusercontent.com/u/547415?v=4"",
        ""gravatar_id"": """",
        ""url"": ""https://api.github.com/users/jonsequitur"",
        ""html_url"": ""https://github.com/jonsequitur"",
        ""followers_url"": ""https://api.github.com/users/jonsequitur/followers"",
        ""following_url"": ""https://api.github.com/users/jonsequitur/following{/other_user}"",
        ""gists_url"": ""https://api.github.com/users/jonsequitur/gists{/gist_id}"",
        ""starred_url"": ""https://api.github.com/users/jonsequitur/starred{/owner}{/repo}"",
        ""subscriptions_url"": ""https://api.github.com/users/jonsequitur/subscriptions"",
        ""organizations_url"": ""https://api.github.com/users/jonsequitur/orgs"",
        ""repos_url"": ""https://api.github.com/users/jonsequitur/repos"",
        ""events_url"": ""https://api.github.com/users/jonsequitur/events{/privacy}"",
        ""received_events_url"": ""https://api.github.com/users/jonsequitur/received_events"",
        ""type"": ""User"",
        ""site_admin"": false
      },
      ""version"": ""09d2eedbbb28f673207f2b54ff5535fc60a137f4"",
      ""committed_at"": ""2018-02-22T23:57:34Z"",
      ""change_status"": {
        ""total"": 11,
        ""additions"": 11,
        ""deletions"": 0
      },
      ""url"": ""https://api.github.com/gists/2a42bde115c1c897a3ffc98256e08e36/09d2eedbbb28f673207f2b54ff5535fc60a137f4""
    }
  ],
  ""truncated"": false
}
";
        #endregion
    }
}
