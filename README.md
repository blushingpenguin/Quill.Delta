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

```c#
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

## Configuration ##

`HtmlConverter` and `XmlConverter` take an configuration options object as the second (optional) argument. The `HtmlConverterOptions` and `XmlConverterOptions` objects share the following properties:

|Option | Default | Description
|---|---|---|
|ParagraphTag| "p" | Custom tag to wrap inline html elements|
|ClassPrefix| "ql" | A css class name to prefix class generating styles such as `size`, `font`, etc. |
|InlineStyles| false | If true, use inline styles instead of classes |
|MultiLineBlockquote| true | Instead of rendering multiple `blockquote` elements for quotes that are consecutive and have same styles(`align`, `indent`, and `direction`), it renders them into only one|
|MultiLineHeader| true | Same deal as `multiLineBlockquote` for headers|
|MultiLineCodeblock| true | Same deal as `multiLineBlockquote` for code-blocks|
|MultiLineParagraph| true | Set to false to generate a new paragraph tag after each enter press (new line)|
|LinkRel| "" | Specifies a value to put on the `rel` attr on links|
|LinkTarget| "_blank" | Specifies target for all links; use `''` (empty string) to not generate `target` attribute. This can be overridden by an individual link op by specifiying the `target` with a value in the respective op's attributes.|
|AllowBackgroundClasses| false | If true, css classes will be added for background attr|

`HtmlConverter` has the following extra properties:
|Option | Default | Description
|--|--|--|
|EncodeHtml| true | If true, `<, >, /, ', ", &` characters in content will be encoded.|
|CustomRenderer| null | A function that will be called to render custom (unknown) delta op to html (the default rendering code skips them)
|BeforeRenderer| null | A function that will be called before rendering the current delta op. If it returns a value then this is used in place of the default rendering result.
|AfterRenderer| null | A function that will be called after rendering the current delta op with the op and result. It should return the result (with optional modifications).

`XmlConverter` has the following extra properties:
|Option | Default | Description
|--|--|--|
|RootNodeTag| "template" | Specifies the root node of the resulting xml document
|CustomRenderer| null | A function that will be called to render custom (unknown) delta op to xml
|BeforeRenderer| null | A function that will be called before rendering the current delta op. If it returns a value then this is used in place of the default rendering result.
|AfterRenderer| null | A function that will be called after rendering the current delta op with the op and result. It should return the result (with optional modifications).

## Rendering Quill Formats ##

You can customize the rendering of Quill formats by registering to the render events before calling the `Convert()` method.

There are `BeforeRender` and `AfterRender` events and they are called multiple times before and after rendering each group. A group is one of:

- continuous sets of inline elements
- a video element
- list elements
- block elements (header, code-block, blockquote, align, indent, and direction)

`BeforeRender` event is called with raw operation objects for you to generate and return your own html. If you return an empty value, the system will return its own generated html.

`AfterRender` event is called with generated xml/html for you to inspect, maybe make some changes and return your modified or original html.

Following shows the parameter formats for `beforeRender` event:

|groupType|data|
|---|---|
|GroupType.Video|{op: `op object`}|
|GroupType.Block|{op: `op object`: ops: Array<`op object`>}|
|GroupType.List| {items: Array<{item: `block`, innerList: `list` or `null` }> }|
|GroupType.InlineGroup|{ops: Array<`op object`>}|

`op object` will have the following format:

```javascript
{
    insert: {
        type: '', // one of 'text' | 'image' | 'video' | 'formula',
        value: '' // some string value  
    },
    attributes: {
        // ... quill delta attributes
    }
}
```

## Rendering Inline Styles ##

If you are rendering to HTML that you intend to include in an email, using classes and a style sheet are not recommended, as [not all browsers support style sheets](https://www.campaignmonitor.com/css/style-element/style-in-head/).  Quill.Delta supports rendering inline styles instead.  The easiest way to enable this is to pass the option `InlineStyles: new InlineStyles()`.

You can customize styles by setting the properties of the `InlineStyles` object:

```c#
var fontDic = new Dictionary<string, string>() {
    { "serif": "font-family: Georgia, Times New Roman, serif" },
    { "monospace": "font-family: Monaco, Courier New, monospace" }
}
var sizeDic = new Dictionary<string, string>() {
      { "small": "font-size: 0.75em" },
      { "large": "font-size: 1.5em" },
      { "huge": "font-size: 2.5em" }
};
new InlineStyles {
    Font = InlineStyles.MakeLookup(fontDic),
    Size = InlineStyles.MakeLookup(sizeDic),
    Indent = (value, op) =>
    {
        int indentSize = Int32.Parse(value) * 3;
        var side = op.Attributes != null &&
            op.Attributes.Direction == DirectionType.Rtl ? "right" : "left";
        return "padding-" + side + ":" + indentSize + "em";
    },
    Direction = (value, op) =>
    {
        if (value == "rtl")
        {
            var hasAlign = op.Attributes != null && op.Attributes.Align.HasValue;
            return $"direction:rtl{(hasAlign ? "" : "; text-align:inherit")}";
        }
        return null;
    }
};
```

Keys to this object are the names of attributes from Quill.  The values are either a simple lookup table (like in the 'font' example above) used to map values to styles, or a `fn(value, op)` which returns a style string.

## Rendering Custom Blot Formats ##

You need to tell system how to render your custom blot by suppliing the CustomRenderer handler in the `XmlConverterOptions` or `HtmlConverterOptions`.

If you would like your custom blot to be rendered as a block (not inside another block or grouped as part of inlines), then add `renderAsBlock: true` to its attributes.

Example:

```c#
using Newtonsoft.Json.Linq;
using Quill.Delta;

var ops = JArray.Parse(@"[
    {insert: {'my-blot': {id: 2, text: 'xyz'}}, attributes: {renderAsBlock: true|false}}
]");

var opts = new HtmlConverterOptions
{
    // customOp is your custom blot op
    // contextOp is the block op that wraps this op, if any.
    // If, for example, your custom blot is located inside a list item,
    // then contextOp would provide that op.
    CustomRenderer = (op: DeltaOp, contextOp: DeltaOp)
    {
        var insert = (InsertDataCustom)op.Insert;
        if (insert.CustomType == "my-blot")
        {
            var val = (JObject)insert.Value;
            return $"<span id=\"{val["id"]}\">{val["text"]}</span>";
        }
        else
        {
            return "Unmanaged custom blot!";
        }
    }
};
var converter = new HtmlConverter(ops, opts);
string html = converter.Convert();
```

`customOp object` will have the following format:

```javascript
{
    insert: {
        type: string // whatever you specified as key for insert, in above example: 'my-blot'
        value: any // value for the custom blot  
    },
    attributes: {
        // ... any attributes custom blot may have
    }
}
```

## Advanced Custom Rendering Using Grouped Ops ##

If you want to do the full rendering yourself, you can do so
by getting the processed & grouped ops.

```c#
var groupedOps = converter.GetGroupedOps();
```

Each element in groupedOps array will be an instance of the
following types:

|type|properties|
|---|---|
|`InlineGroup`|Ops: `IList<DeltaOp>`|
|`VideoItem`|Op: `DeltaOp`|
|`BlockGroup`|Op: `DeltaOp`, Ops: `IList<DeltaOp>`|
|`ListGroup`|Items: `IList<ListItem>`|
||ListItem: { Item: `BlockGroup`, InnerList: `ListGroup` }|
|`BlotBlock`|Op: `DeltaOp`|

`BlotBlock` represents custom blots with `renderAsBlock:true` property pair in its attributes

See above for `DeltaOp` properties.

## Local Development ##

Hacking on `Quill.Delta` is easy! To quickly get started clone the repo:

    $ git clone https://github.com/blushingpenguin/Quill.Delta.git
    $ cd Quill.Delta

To compile the code and run the tests just open the solution in
Visual Studio 2017 Community Edition.  To generate a code coverage report
run cover.bat from the solution directory.
