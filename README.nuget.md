[![ci.appveyor.com](https://ci.appveyor.com/api/projects/status/github/blushingpenguin/Quill.Delta?branch=master&svg=true)](https://ci.appveyor.com/api/projects/status/github/blushingpenguin/Quill.Delta?branch=master&svg=true)
[![codecov.io](https://codecov.io/gh/blushingpenguin/Quill.Delta/coverage.svg?branch=master)](https://codecov.io/gh/blushingpenguin/Quill.Delta?branch=master)

# Quill Delta to HTML Converter #

Converts [Quill's](https://quilljs.com) [Delta](https://quilljs.com/docs/delta/) format to XML or HTML (insert ops only) with properly nested lists.  This is a c# port of the [javascript version](https://github.com/nozer/quill-delta-to-html) by nozer.

## Quickstart ##

Install with package manager:

    Install-Package Quill.Delta

or with nuget:

    nuget install Quill.Delta

Or with dotnet:

    dotnet add package Quill.Delta

Quill.Delta can also be installed from [nuget.org](https://www.nuget.org/packages/Quill.Delta/).

## Usage ##

```csharp
using Quill.Delta;
using Newtonsoft.Json.Linq;

var deltaOps = JArray.Parse(@"[
    {insert: ""Hello\n""},
    {insert: ""This is colorful"", attributes: {color: '#f00'}}
]");
var htmlConverter = new HtmlConverter(deltaOps);
string html = htmlConverter.Convert();

var xmlConverter = new XmlConverter(deltaOps);
XmlDocument xml = xmlConverter.Convert();
```

## Advanced ##

See the [full documentation on github](https://github.com/blushingpenguin/Quill.Delta/blob/master/README.md)
