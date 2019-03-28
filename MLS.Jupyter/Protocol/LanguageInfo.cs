using System.Collections.Generic;
using Newtonsoft.Json;

namespace MLS.Jupyter.Protocol
{
    public class LanguageInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("mimetype")]
        public string MimeType { get; set; }

        [JsonProperty("file_extension")]
        public string FileExtension { get; set; }

        [JsonProperty("pygments_lexer")]
        public string PygmentsLexer { get; set; }

        [JsonProperty("codemirror_mode")]
        public object CodeMirrorMode { get; set; }

        [JsonProperty("nbconvert_exporter")]
        public string NbConvertExporter { get; set; }

        [JsonProperty("help_links")]
        public List<Link> HelpLinks { get;  } = new List<Link>();
    }
}