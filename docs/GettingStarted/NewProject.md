## Build your first dotnet try Experience

1. Go to the terminal and navigate to the `GettingStarted` folder.

2. Create a new project `dotnet new console -o mydoc`
```
//This editor below will autopopulate to the full Program.cs once the user has created mydoc app
```
3. Go back to the terminal and run `dotnet try mydoc`. *(make sure you run `dotnet try mydoc` in \GettingStarted not in \mydoc)*

**Tada!** You have created your first C# interactive developer experience.Using the `--project` option we are able to pull code from the backing C# project. The code fence now looks like this:

<pre>
    <code>
        ```cs --project .\mydoc\mydoc.csproj .\mydoc\Program.cs
        ```
    </code>
</pre>

As you might have noticed you see the full Program.cs file.Suppose you would like to show your user only the `Console.WriteLine("Hello World!");`. 
This is done using the `--region` option and [C# regions](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives/preprocessor-region)

### Defining Regions
1. Open `Program.cs` in your `mydoc` app.
2. Open the code block region by placing `#region run1` above `Console.WriteLine("Hello World!");` then, place ` #endregion` to close the block.
Your Program.cs should look like this:

```cs
using System;
namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            #region run1
            Console.WriteLine("Hello World!");
            #endregion
        }
    }
}
```

3. In this Markdown file `GettingStarted\NewProject.md`, add the snippet below. Notice we have appended the `--region` option.

<pre>
    <code>
    ```cs --project .\mydoc\mydoc.csproj .\mydoc\Program.cs --region run1
    ```
    </code>
</pre>

By now you should be seeing an interactive code snippet that only shows `Console.WriteLine("Hello World!");`. In our next tutorial, we are going to learn about the `--session` option. 

#### [Back - Basics](./Introduction.md) <----> [Next - Defining sessions](./Sessions.md)

[Resources Glossary](./Glossary.md) | [Ask questions or file bugs here](https://teams.microsoft.com/l/channel/19%3a32c2f8c34d4b4136b4adf554308363fc%40thread.skype/Try%2520.NET?groupId=fdff90ed-0b3b-4caa-a30a-efb4dd47665f&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47)