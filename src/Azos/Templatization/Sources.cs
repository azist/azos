
using System.Text;
using System.IO;

namespace Azos.Templatization
{
    /// <summary>
    /// Describes an entity that provides source for template
    /// </summary>
    public interface ITemplateSource
    {
      /// <summary>
      /// Returns template source content
      /// </summary>
      object GetSourceContent();

      /// <summary>
      /// Tries to suggest a class name for this content or null
      /// </summary>
      string InferClassName();

      /// <summary>
      /// Gets printable source name
      /// </summary>
      string GetName(int maxLength);

      /// <summary>
      /// Returns content relative to this one or null if not supported
      /// </summary>
      object GetRelativeContent(string relativeName);
    }

    /// <summary>
    /// Describes an entity that provides source for templates
    /// </summary>
    public interface ITemplateSource<T> : ITemplateSource
    {
      /// <summary>
      /// Returns template source content
      /// </summary>
      new T GetSourceContent();

      /// <summary>
      /// Returns content relative to this one or null if not supported
      /// </summary>
      new T GetRelativeContent(string relativeName);
    }



    /// <summary>
    /// Represents a string template source that comes from a file
    /// </summary>
    public class FileTemplateStringContentSource : ITemplateSource<string>
    {
      public FileTemplateStringContentSource(string fileName)
      {
        if (string.IsNullOrWhiteSpace(fileName))
         throw new TemplatizationException(StringConsts.ARGUMENT_ERROR + "FileTemplateStringContentSource.ctor(null)");
        FileName = fileName;
      }


      public readonly string FileName;


      /// <summary>
      /// Returns the name of template class inferred from file name
      /// </summary>
      public string InferClassName()
      {
          var name = Path.GetFileNameWithoutExtension(FileName).Trim();

          var result = new StringBuilder(name.Length);
          var first = true;
          foreach(var c in name)
          {
             if ((first && !char.IsLetter(c)) || !char.IsLetterOrDigit(c))
                result.Append("_");
             else
                result.Append(c);

             first = false;
          }

          return result.ToString();
      }

      public string GetSourceContent()
      {
        return File.ReadAllText(FileName);
      }

      object ITemplateSource.GetSourceContent()
      {
        return this.GetSourceContent();
      }

      public string GetRelativeContent(string relativeName)
      {
        var rfn = Path.Combine(Path.GetDirectoryName(FileName), relativeName);
        return File.ReadAllText(rfn);
      }

      object ITemplateSource.GetRelativeContent(string relativeName)
      {
        return this.GetRelativeContent(relativeName);
      }

      /// <summary>
      /// Gets printable source name
      /// </summary>
       public string GetName(int maxLength)
       {
         if (maxLength>0 && FileName.Length>maxLength)
          return "..." + FileName.Substring(FileName.Length-maxLength);

         return FileName;
       }

      public override string ToString()
      {
          return FileName;
      }

      public override bool Equals(object obj)
      {
          return FileName.Equals(obj);
      }

      public override int GetHashCode()
      {
          return FileName.GetHashCode();
      }
    }


    /// <summary>
    /// Represents a string template source that comes from a string
    /// </summary>
    public class TemplateStringContentSource : ITemplateSource<string>
    {
      public TemplateStringContentSource(string content)
      {
        if (string.IsNullOrWhiteSpace(content))
         throw new TemplatizationException(StringConsts.ARGUMENT_ERROR + "TemplateStringContentSource.ctor(null)");
        Content = content;
      }


      public readonly string Content;

      /// <summary>
      /// Returns null as class name can not be inferred from string
      /// </summary>
      public string InferClassName()
      {
         return null;
      }

      public string GetSourceContent()
      {
        return Content;
      }

      object ITemplateSource.GetSourceContent()
      {
        return Content;
      }

      public string GetRelativeContent(string relativeName)
      {
        return null;
      }

      object ITemplateSource.GetRelativeContent(string relativeName)
      {
        return null;
      }


      /// <summary>
      /// Gets printable source name
      /// </summary>
       public string GetName(int maxLength)
       {
         if (maxLength>0 && Content.Length>maxLength)
          return Content.Substring(0, maxLength) + "...";

         return Content;
       }


      public override string ToString()
      {
          return Content;
      }

      public override bool Equals(object obj)
      {
          return Content.Equals(obj);
      }

      public override int GetHashCode()
      {
          return Content.GetHashCode();
      }
  }



}
