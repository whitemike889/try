using System;
using System.Collections.Generic;

namespace OmniSharp.Client.Commands
{
    public class QuickFix
    {
        public QuickFix(
            string logLevel = null, 
            string fileName = null, 
            int line = 0, 
            int column = 0, 
            int endLine = 0, 
            int endColumn = 0, 
            string text = null,
             ICollection<string> projects = null)
        {
            LogLevel = logLevel;
            FileName = fileName;
            Line = line;
            Column = column;
            EndLine = endLine;
            EndColumn = endColumn;
            Text = text;
            Projects = projects ?? Array.Empty<string>();
        }

        public string LogLevel { get;  }

        public string FileName { get;  }

        public int Line { get;  }

        public int Column { get;  }

        public int EndLine { get;  }

        public int EndColumn { get;  }

        public string Text { get;  }

        public ICollection<string> Projects { get;  } 

        public override string ToString()
            => $"{Line}:{Column}-{EndLine}:{EndColumn}: {Text} ({FileName})";
    }
}