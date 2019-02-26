# Quick Start

Congratulations! You just ran `dotnet try demo`.  This an interactive getting starting guide to get you familiar with the `dotnet try` tool.  

## What is dotnet try?

`dotnet try` is a tool that allows you to create interactive documentation for your users. 

Let's start your journey by getting familiar with this new interactive documentation experience. Try editing and running the code in the editor below.

**Example 1**
```csharp .\Snippets\Program.cs --project .\Snippets\Snippets.csproj --session "Run example 1" --region run1
```
## What's happening behind the scenes?

The content for this page is in a Markdown file, `QuickStart.md`, which you can find in the folder where you just ran `dotnet try demo`. The sample code that you see in the editor is in the Snippets subfolder, in a regular C# project that was created using `dotnet new console`.

## Code fence options

The term "code fence" refers to the Markdown delimiters around a multi-line block of code. Here's an example:

<pre>
    <code>
        ```cs 
        Console.WriteLine("Hello World!");
        ```
    </code>
</pre>

The `dotnet try` tool extends Markdown using a set of options that can be added after the language keyword in the code fence. This lets you reference sample code from the backing project, allowing a normal C# project, rather than the documentation, to be the source of truth. This removes the need to copy and paste code snippets from a code sample into your Markdown file.

**Example 2**
```cs --project .\Snippets\Snippets.csproj .\Snippets\Program.cs --session "Run example 2" --region run2
```

For example, the code snippet above was extended using `dotnet try`. So, your code fence will look like the snippet out below. 

<pre>
    <code>
        ```cs .\Snippets\Program.cs  --project .\Snippets\Snippets.csproj --session "Run example 2" --region run2
        ```
    </code>
</pre>

## Code Fence Options Breakdown

| Option                                 | What it does                                             |
|----------------------------------------|----------------------------------------------------------|
| `.\Snippets\Program.cs`                | Points to the file where the sample code is found.       |
| `--region run2`                        | Identifes a C# code `#region` to focus on.               |
| `--project .\Snippets\Snippets.csproj` | Points to the project that the sample is part of.        |
| `--session "Run example 2"`            | Creates a new session and is the name of the run button. |  

### Document Verification

Verifying that your code samples work is vital to your user experience, so `dotnet try` acts as a compiler for your documentation. Change the `--region` option in Example 1 from `--region run1` to `--region run5`. This change will break the sample, and you can see this error in two different ways:

1. Refresh the browser. You'll now see an error like this:

    ![image](https://user-images.githubusercontent.com/547415/53391389-14743000-394b-11e9-8305-1f2a3b72f95a.png)


2. Since it's also important to be able to verify your documentation using automation, we added the `dotnet try verify` command. At the command line, navigate to the root of your samples directory and run `dotnet try verify`. You will see something similar to this:

![dotnet verify -error](https://user-images.githubusercontent.com/2546640/53290283-c8b95f00-376f-11e9-8350-1a3e470267b5.PNG)

Now change the region option back to `--region run1`. Save the changes. If you re-run the  `dotnet try verify` you'll see all green check marks and the *Region not found* error is gone in the. 

## Exercise   
Below we have created a quick exercise that will teach you how to: create a new snippet, specify a region in an existing back project, and define a new session. 

1. Go to  `\Snippets\Program.cs`  in the `GettingStarted` folder.
2. Find the `Run3` method and add the code below makes sure to leave the C# regions in place.
```
string primes;
primes = String.Format("Prime numbers less than 10: {0}, {1}, {2}, {3}",2, 3, 5, 7 );
Console.WriteLine(primes);
```
3. Update this markdown file (`QuickStart.md`) and add a new `cs` code fence that references the code in the `Run3` method. 

***Results for your code snippet should go here.***

*Hint* Look at the static code snippet under **Code fence options**. Make sure to update the `--session` and `--region` options.

#### [Next: A beginners guide to getting started with `dotnet try` from scratch](./Introduction.md)

[Resources Glossary](./Glossary.md) | [Ask questions or file bugs here](https://teams.microsoft.com/l/channel/19%3a32c2f8c34d4b4136b4adf554308363fc%40thread.skype/Try%2520.NET?groupId=fdff90ed-0b3b-4caa-a30a-efb4dd47665f&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47)




