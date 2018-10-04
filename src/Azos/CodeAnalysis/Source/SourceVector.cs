
namespace Azos.CodeAnalysis.Source
{
   /// <summary>
   /// Represents a pointer to the named source code  and character position
   /// </summary>
   public struct SourceVector
   {
         public readonly string SourceName;
         public readonly SourcePosition Position;

         public SourceVector(string srcName, SourcePosition position)
         {
           SourceName = srcName;
           Position = position;
         }

   }


}
