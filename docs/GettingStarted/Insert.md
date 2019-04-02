# Read-only code 

Add these usings
```cs  --editable false --region usings --destination-file .\Snippets\Program.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

```

```cs --hidden --editable false
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class DataObject{
     #region custom_code
     #endregion

}
```

Declare a method
```cs --editable false  --region custom_code
public IEnumerable<string> PrintMe(){
    yield return "What an adventure";
}
```

Declare Utilities that are not on rendered in the page
```cs --editable false --hidden
public class HiddenObject{

}
```

```cs --editable false --hidden --destination-file .\Snippets\Program.cs
 #region usings
 #endregion

namespace Snippets
{
    public class Program
    {
        static void Main()
        {
            #region run1
            Console.WriteLine(DateTime.Now);
            #endregion

            Console.WriteLine("this is from hidden include");
        }        
    }

```

```cs --editable false --hidden --destination-file .\Snippets\Program.cs
    public class ProgramUtility
    {
    }
}
```

```cs 
using System.Text;
```

readonly code is part of the compialtion, try using the `VisibleObject` or the `HiddenObject`.
```csharp --source-file .\Snippets\Program.cs --project .\Snippets\Snippets.csproj --session "Run example 1" --region run1
```