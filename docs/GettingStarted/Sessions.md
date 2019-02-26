### Defining Sessions
Let's make this a little more interesting.  
1. Go to `mydoc` app you created and add a new cs file called `Cat.cs`and add the following code
```
using System;

namespace mydoc
{
    class Cat
    {
        public string Say() 
        {
            #region run2
            return "meow!";
            #endregion
        }
    }
}
```
2. Update your `Program.cs`as follows 
```
   Console.WriteLine(new Cat().Say());
```
3. In this markdown`GettingStarted\Session` Create two separate snippets with one point to `Program.cs` and `run1` region and the other pointing to, `Cat.cs` and `run2` region. 

*Cheat Sheet*
<pre>
<code>
```cs --project .\mydoc\mydoc.csproj .\mydoc\Program.cs --region run1```
```cs --project .\mydoc\mydoc.csproj .\mydoc\Cat.cs --region run2```
</code>
</pre>
Once you have added the above refresh the page and, now hit run.

These code snippets are in two separate files and compile and execute together. If we wanted to execute them independently, we would use a`--session` option. Providing a new session name for each snippet.

4. Copy the code fence snippets we created in step 3 and add the `--session` option.
<pre>
<code>
```cs --project .\mydoc\mydoc.csproj .\mydoc\Program.cs --session "Run 1" --region run1```
```cs --project .\mydoc\mydoc.csproj .\mydoc\Cat.cs --session "Run 2" --region run2```
</code>
</pre>
Once you have added the above refresh the page and, you should see two separate output panels and run buttons.

Well done! Now, you have a project that you almost ready to share with others. A big part for good documentation is making sure everything works. In `dotnet try` we do this by using `dotnet try verify` which we will explore in the last module.

#### [Back - Defining Regions](./NewProject.md)<----> [Next - Document verification](./Verify.md)

[Resources Glossary](./Glossary.md) | [Ask questions or file bugs here](https://teams.microsoft.com/l/channel/19%3a32c2f8c34d4b4136b4adf554308363fc%40thread.skype/Try%2520.NET?groupId=fdff90ed-0b3b-4caa-a30a-efb4dd47665f&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47)