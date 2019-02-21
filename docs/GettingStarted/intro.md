# Getting started with dotnet try

Congratulations! You just ran `dotnet try demo`. 

`dotnet try` lets you make interactive code samples like this:

_**Example #1**_

```csharp --project .\Snippets\Snippets.csproj .\Snippets\Program.cs --session "Run #1" --region run1
```

The content for this page is written in a Markdown file, `intro.md`, which you can find in the folder where you just ran `dotnet try demo`. The sample code lives in the Snippets subfolder, in a normal C# project that was created using `dotnet new console`.

_**Example #2**_

```cs --project .\Snippets\Snippets.csproj .\Snippets\Program.cs --session "Run #2" --region run2
```


The code fence in the Markdown pulls the sample code from the backing project, allowing the code rather than the documentation to be the source of truth.You don't need to keep a separate copy of the sample in the Markdown file.

`dotnet try` extends Markdown using a set of options that can be added after the language keyword in the code fence.

<pre>
    <code>
        ```cs --project .\Snippets\Snippets.csproj .\Snippets\Program.cs --session "Run #2" --region run2
        ```
    </code>
</pre>

To point to a specific file, 



