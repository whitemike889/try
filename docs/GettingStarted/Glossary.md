## Glossary 

### dotnet try
`dotnet try ` is a set of tools that allows you to create interactive samples for your users.

**List of available dotnet try commands.**

If you shut down this project and type the command `dotnet try -h` you will see a list of commands:

|Command   |Purpose   |
|---|---|
|`demo`|launches getting started documentation|
|`list-package`|list of installed Try .NET packages|
|`github`|try a GitHub repo|
| `verify`|compiler for documentation|

### Code Fence Options
`dotnet try` extends Markdown using set of options that are added after langauage keyword in the code fence (*see below*).

|Option   |Purpose   |
|---|---|
|`--project`   |enables you to point to a specific file. |
| `--session`  | allows you create a seperate session in single file. |  
|  `--region` |lets you specify the block of code that you want to display in the editor.   |  

