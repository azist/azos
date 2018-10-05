# Azos.Graphics

The purpose of this library is to provide cross-platform very basic imaging functionality.

The very basic functionality is defined as:
* Read/Write Jpeg, Png and Gif formats
* Ability to draw lines, brushes and text/fonts with simple attributes like color/width
* Scale images

This covers the needs of 90+% of simple application that need to generate a simple image (e.g. for testing purposes).

This library is not purposed/desined for complex image processing tool like Photoshop and the like
as it only abstracts the basics of imaging.

## Complex Imaging

Should a complex "good-looking" rendering be required (in most business-oriented apps), the application should instead delegate 
complex graphics to an application module(microservice) which abstracts the creation of such graphics. 

### Complex Imaging Delegation Example
```
public interface IMemebrshipCardRenderer : IModule
{
  Azos.Graphics.Image RenderMemebrshipCardFront(IMemberData data);
  Azos.Graphics.Image RenderMemebrshipCardBack(IMemberData data);
}
```
......

Then, a module implements this interface:

* MembershipCardRenderer class gets implemented using SkiaSharp.dll on Linux;
* MembershipCardRenderer class gets implemented using and GDI+(or WPF) on Windows/.NET FX;
* more implementations may be provided....

The module dependency gets injected/installed in the application container via configuration,
so for an entry point running on Windows, a DLL with GDI-specific code is used, whereas
Linux uses its own (e.g. Skia et.al.)

This approach allows for a complete control of how images get renedered per specific technology,
without sacrificing platform features of the particular graphics library.
