using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WorkspaceServer.Servers.Roslyn.Instrumentation.Contract;

namespace WorkspaceServer.Servers.Roslyn.Instrumentation.Contract
{
    public class ProgramStateAtPosition 
    {
        [JsonProperty("filePosition")]
        public FilePosition FilePosition { get; set; }

        [JsonProperty("stackTrace")]
        public string StackTrace { get; set; }

        [JsonProperty("locals")]
        public VariableInfo[] Locals { get; set; }

        [JsonProperty("parameters")]
        public VariableInfo[] Parameters { get; set; }

        [JsonProperty("fields")]
        public VariableInfo[] Fields { get; set; }

        [JsonProperty("output")]
        public DeclarationLocation Output { get; set; }
     
    }

    public partial class VariableInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public JToken Value { get; set; }

        [JsonProperty("declaredAt")]
        public DeclarationLocation DeclaredAt { get; set; }
    }

    public partial class DeclarationLocation
    {
        [JsonProperty("start")]
        public long Start { get; set; }

        [JsonProperty("end")]
        public long End { get; set; }
    }

    public partial class FilePosition
    {
        [JsonProperty("line")]
        public long Line { get; set; }

        [JsonProperty("character")]
        public long Character { get; set; }

        [JsonProperty("file")]
        public string File { get; set; }
    }
}
    public static class InstrumentationEmitter
    {
        public static readonly string Sentinel = "6a2f74a2-f01d-423d-a40f-726aa7358a81";
        public static JToken GetProgramState(
            string filePositionStr, //FilePosition filePosition,
            params (string info, object value)[] variableInfo) //VariableInfo[] variableInfo) // string = variableInfo about variable
        {
            var filePosition = JsonConvert.DeserializeObject<FilePosition>(filePositionStr);

            List<VariableInfo> finalInfos = new List<VariableInfo>();
            foreach (var (info, value) in variableInfo)
            {
                var vInfo = JsonConvert.DeserializeObject<VariableInfo>(info);
                var neInfo = new VariableInfo()
                {
                    DeclaredAt = vInfo.DeclaredAt,
                    Name = vInfo.Name,
                    Value = value.ToString()
                };
                finalInfos.Add(neInfo);
                   
            }


            return JToken.FromObject(new ProgramStateAtPosition
            {
                FilePosition = filePosition,
                Locals = finalInfos.ToArray()
            });
        }

        public static void EmitProgramState(JToken programState)
        {
            Console.WriteLine(Sentinel + programState + Sentinel);
        }
    }

