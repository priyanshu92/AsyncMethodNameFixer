# Async Method Name Fixer

Download this extension from the [VS Marketplace](https://marketplace.visualstudio.com/items?itemName=PRIYANSHUAGRAWAL92.AsyncMethodNameFixer) or get the [CI build](http://vsixgallery.com/extension/3f1bd9bf-d048-4430-8705-1a26a4819614/)

---------------------------------------

[![Nuget](https://img.shields.io/nuget/v/AsyncMethodNameFixer.svg)](https://www.nuget.org/packages/AsyncMethodNameFixer)
[![Nuget](https://img.shields.io/nuget/dt/AsyncMethodNameFixer.svg)](https://www.nuget.org/packages/AsyncMethodNameFixer)
[![Build Status](https://dev.azure.com/agrawalpriyanshu/AsyncMethodNameFixer/_apis/build/status/priyanshu92.AsyncMethodNameFixer?branchName=master)](https://dev.azure.com/agrawalpriyanshu/AsyncMethodNameFixer/_build/latest?definitionId=1&branchName=master)
![Azure DevOps tests](https://img.shields.io/azure-devops/tests/agrawalpriyanshu/AsyncMethodNameFixer/1.svg)

The easiest way to analyze and fix method names for asynchronous methods.

See the [change log](CHANGELOG.md) for changes and road map.

## Features

- Fix Async Method Names.
- Ignores overridden methods.
- Ignores `Main` method.
- Ignores interface implemented methods.
- Ignores test methods.

### Fix Async Method Names
If there is any asynchronous method violating the naming convention then the analyzer will show a warning and the lightbulb to fix it.

![Warning](Screenshots/Warning.png)

Clicking on the lightbulb or pressing `Ctrl + . ` will show the fix with preview.

![Fix Preview](Screenshots/Fix_Preview.png)

Pressing *return* will apply the fix and rename the method at all places.

![Fixed](Screenshots/Fixed.png)


## Contribute

For cloning and building this project yourself, make sure
to install the
[Extensibility Tools 2015](https://visualstudiogallery.msdn.microsoft.com/ab39a092-1343-46e2-b0f1-6a3f91155aa6)
extension for Visual Studio which enables some features
used by this project.

## License
[Apache 2.0](LICENSE)
