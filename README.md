# FileTypeInterrogator

netstandard library for detecting file types by 'magic numbers', similar to the `file` command in Linux/Unix. Useful for validating file uploads instead of trusting file extensions.

[![NuGet](https://img.shields.io/nuget/v/FileTypeInterrogator.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/FileTypeInterrogator/)

# Usage

```
IFileTypeInterrogator interrogator = new FileTypeInterrogator();

byte[] fileBytes = File.ReadAllBytes("pdfFile.pdf");

FileTypeInfo fileTypeInfo = interrogator.DetectType(fileBytes);

Console.WriteLine("Name = " + fileTypeInfo.Name);
Console.WriteLine("Extension = " + fileTypeInfo.FileType);
Console.WriteLine("Mime Type = " + fileTypeInfo.MimeType);

// The following output would be displayed:
// Name = Portable Document Format File
// Extension = pdf
// Mime Type = application/pdf
```

Notice that `IFileTypeInterrogator` was assigned, meaning custom implementations are welcomed.  File definitions are provided in the source and will be regularly updated, feel free to submit an issue or pull request to add other signatures.  To quickly provide support for a new signature, create an instance of `CustomFileTypeInterrogator`:

```
IFileTypeInterrogator interrogator = new CustomFileTypeInterrogator("path_to_custom_definitions_file", Encoding.UTF8);

FileTypeInfo fileTypeInfo = interrogator.DetectType(...);
```

It is best to create a single instance and manage it's lifetime through an IOC/DI Container.
