# leviathan-server

Compile using Visual Studio Code (VS Code). This project targets .Net Core 2.1. I am using Ubuntu 18.04.

Install .Net Core using the following [instructions](https://docs.microsoft.com/en-us/dotnet/core/install/linux-scripted-manual#scripted-install).

Here is the script parameters I used for development purposes:

```bash dotnet-install.sh -c 2.1```

Then I added it to the PATH.

To run use ```dotnet run```

To build dll use ```dotnet publish -c Release -r linux-x64 --self-contained```

If you are just running the program on a server, use the complete published folder (removing the .pdb file):

```./publish/leviathan-server```