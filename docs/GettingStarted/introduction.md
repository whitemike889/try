# Getting Started with dotnet try 

Congratulations! You have run `dotnet try demo`.  

`dotnet try ` is a set of tools that allows you to create interactive samples for your users.

Over the next couple of steps, we are going to provide you with an immersive on how to get started. 

Click on the `Run` button to run the code. 
```cs --project .\Snippets\Snippets.csproj .\Snippets\Program.cs --session "Run" --region run
```
Feel free to edit the code too. In this example, we are making a simple Hello World console application. Why don't we try something different like, `Console.WriteLine(DateTime.Now);` and hit run again. As you can see as a user, you have the flexibility to alter the content in the editor. 

## Understanding the Basics.
The content on this page is in the `Introduction.md` Markdown file,which you can find in `GettingStarted\Introduction.md`. The code snippets are backed by a full C# project created using `dotnet new console`, found in the `GettingStarted\Snippets` subfolder.

### Exploring the Markdown file.

What's new in your markdown file?  `dotnet try` extends Markdown using set of flags that are added after langauage keyword in the code fence (*see below*).
<pre>
<code>
       ```cs --project .\Snippets\Snippets.csproj .\Snippets\Program.cs --session "Run" --region run```
</code>
</pre>


|Flag   |Purpose   |
|---|---|
|`--project`   |enables you to point to a specific file. |
|`--session`  | allows you create a seperate session in single file. |  
|`--region` |lets you specify the block of code that you want to display in the editor.   |  

#### [Back - Quick Start](./QuickStart.md) <----> [Next - Getting Started from scratch](./NewProject.md)


[Resources Glossary](./Glossary.md) | [Ask questions or file bugs here](https://teams.microsoft.com/l/channel/19%3a32c2f8c34d4b4136b4adf554308363fc%40thread.skype/Try%2520.NET?groupId=fdff90ed-0b3b-4caa-a30a-efb4dd47665f&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47)


