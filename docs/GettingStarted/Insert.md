# Read-only code 

Can be used to import a set of using statemtnts
```cs --include
using System.Collections.Generic;
using System.IO;
using System.Linq;
```

Declare a class
```cs --include
public class VisibleObject{

}
```

Declare Utilities that are not on rendered in the page
```cs --include --hidden
public class HiddenObject{

}
```

```cs 
using System.Text;
```

readonly code is part of the compialtion, try using the `VisibleObject` or the `HiddenObject`.
```csharp --source-file .\Snippets\Program.cs --project .\Snippets\Snippets.csproj --session "Run example 1" --region run1
```