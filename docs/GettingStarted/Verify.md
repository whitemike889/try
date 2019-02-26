### Document Verification

`dotnet try verify` is a compiler for documentation. With this command,you can make sure that every code snippet will work and is in sync with the backing project.

You can see `dotnet try verify` at work in two ways: In the document UI and using `dotnet try verify` command. Let's see in practice.

Exercise 

1. Using the document UI. 
In the `Verify.md` file you are going to remove the `<pre>` and `<code>`around the code fence. 
Once you have remove the HTML tags, you will notice a `No Project Found` warning in your document UI.This is a clear way to indicate that your document and your backing project are out of sync.
<pre>
<code>
```cs --project .\mydoc2\mydoc2.csproj .\mydoc2\Program.cs --session "Run 1" --region run1
``` 
</code>
</pre>
2. Using the `dotnet try verify`. 
The first thing you need to shut down the app in the terminal. *Rerun the `dotnet try demo` command right after the next step.*
- Go to your terminal and run `dotnet try verify`. You should see something similar to the below. 

![dotnet verify -errorproject](https://user-images.githubusercontent.com/2546640/53291265-8f3c2000-377e-11e9-9b82-b7ea3ce1ab05.PNG)

- Change the project name back to `mydoc`. Save the changes. If you re-run the  `dotnet try verify` you see all green check marks and the *No project* error is gone from the UI. 

Congrats! You have finished the `dotnet try` interactive walkthrough. 

### [Back - Defining sessions](./Sessions.md)

Resources

- [Quick Start](./QuickStart.md)
- [Getting started from stratch](./Introduction.md) - Short 4 part tutorial. 
- [Questions and Feedback](https://teams.microsoft.com/l/channel/19%3a32c2f8c34d4b4136b4adf554308363fc%40thread.skype/Try%2520.NET?groupId=fdff90ed-0b3b-4caa-a30a-efb4dd47665f&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47)