# Quick Start

Congratulations! You just ran `dotnet try demo`.  This an interactive getting starting guide to get you familiar with the `dotnet try` tool.  

**What is dotnet try?**  `dotnet try ` is a tool that allows you to create interactive documentation for your users. 

### Getting familiar with `dotnet try` 
Let's start your journey by getting familiar with this new interactive documentation experience.


**Run & Edit**

Let's at start the beginning.  Try running and editing the code in the editor below.

**Example 1**
```csharp --project .\Snippets\Snippets.csproj .\Snippets\Program.cs --session "Run #1" --region run1
```
**What's happening behind the scenes?**

The content for this page is in a Markdown file, `QuickStart.md`, which you can find in the folder where you just ran `dotnet try demo` command. The sample code that you see in the editor is in the Snippets subfolder, in a regular C# project that was created using `dotnet new console`.

**Extending the code fence**

The code fence in the Markdown pulls the sample code from the backing project, allowing the code rather than the documentation to be the source of truth. This removes the need to copy and paste code snippets from a code sample into your Markdown file.

**Example 2**
```cs --project .\Snippets\Snippets.csproj .\Snippets\Program.cs --session "Run #2" --region run2
```

`dotnet try` extends Markdown using a set of options that can be added after the language keyword in the code fence. 

For example, the code snippet above was extended using `dotnet try`. So, your code fence will look like the snippet out below. 

<pre>
    <code>
        ```cs --project .\Snippets\Snippets.csproj .\Snippets\Program.cs --session "Run #2" --region run2
        ```
    </code>
</pre>

**Code Fence Breakdown**

|Code Fence option  | What it does  |
|---|---|
|`--project .\Snippets\Snippets.csproj .\Snippets\Program.cs`   | Points to a specific file. |
| `--session "Run #2"`  | Creates a new session and is the name of the run button. |  
|  `--region run2` |identifes and displays the code in the editor. |

**Document Verification**

Verifying that your code snippets and samples are in sync is vital to your user experience.  That's why we added `dotnet try verify` to the toolset. 
`dotnet try verify` is a compiler for documentation. With this command, you can make sure that every code snippet will work and is in sync with the backing project. 
You can see `dotnet try verify` at work in two ways.
1. In the document UI. Let's give it a try,  rename the region in Example 1 `--region run1` to `--region run5` . Save the changes and refresh the page. You'll now see a *Region not found* error.'

2. Using the `dotnet try verify`. Open a new terminal and navigate to the `GettingStarted` folder.
- Run `dotnet try verify` command. You see something similar to the below. 
![dotnet verify -error](https://user-images.githubusercontent.com/2546640/53290283-c8b95f00-376f-11e9-8350-1a3e470267b5.PNG)

- Change the region name back to `run1`. Save the changes. If you re-run the  `dotnet try verify` you see all green check marks and the *Region not found* error is gone from the UI. 

#### Exercise   
Below we have created a quick exercise that will teach you how to: create a new snippet, specify a region in an existing back project, and define a new session. 

Steps 

1. Go to  `\Snippets\Program.cs`  in the `GettingStarted` folder.
2. Find the `Run3` method and add the code below makes sure to leave the C# regions in place.
```
string primes;
primes = String.Format("Prime numbers less than 10: {0}, {1}, {2}, {3}",2, 3, 5, 7 );
Console.WriteLine(primes);
```
3. Update this markdown file (`QuickStart.md`) and add a new `cs` code fence that references the code in the `Run3` method. 

***Results for your code snippet should go here.***

*Hint* Look at the static code snippet under **Extending the code fence**. Make sure to update the `--session name` and `--region name`

#### [Next: A beginners guide to getting started with `dotnet try` from scratch](./Introduction.md)

[Resources Glossary](./Glossary.md) | [Ask questions or file bugs here](https://teams.microsoft.com/l/channel/19%3a32c2f8c34d4b4136b4adf554308363fc%40thread.skype/Try%2520.NET?groupId=fdff90ed-0b3b-4caa-a30a-efb4dd47665f&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47)




