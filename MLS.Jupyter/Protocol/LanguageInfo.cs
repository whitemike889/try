using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MLS.Jupyter.Protocol
{
    public class LanguageInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("mimetype")]
        public string Mimetype { get; set; }

        [JsonProperty("file_extension")]
        public string FileExtension { get; set; }

        [JsonProperty("pygments_lexer")]
        public string PygmentsLexer { get; set; }

        [JsonProperty("codemirror_mode")]
        public JToken CodemirrorMode { get; set; }

        [JsonProperty("nbconvert_exporter")]
        public string NbconvertExporter { get; set; }

        [JsonProperty("help_links")]
        public List<Link> HelpLinks { get; set; }
    }
}