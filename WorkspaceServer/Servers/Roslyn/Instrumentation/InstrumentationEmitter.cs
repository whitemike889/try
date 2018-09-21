using System;
using System.Collections.Generic;
using MLS.Protocol.Instrumentation;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
            var neInfo = new VariableInfo
            {
                RangeOfLines = vInfo.RangeOfLines,
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
