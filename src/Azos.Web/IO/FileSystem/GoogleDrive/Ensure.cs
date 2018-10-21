
using System;

namespace Azos.IO.FileSystem.GoogleDrive
{
  static class Ensure
  {
    public static void NotNull(object value, string name)
    {
      if (value != null)
        return;

      throw new ArgumentNullException(name);
    }
  }
}
