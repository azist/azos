/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System.Collections.Generic;

using Azos.Data;


namespace Azos.Tests.Nub.IO.Archiving
{
  public static class FakeBuilder
  {
    /// <summary>
    /// Creates and returns many random FakeRows with GDID global Ids based on
    /// the supplied era, authority, count (number to produce), and offset (starting point of GDID counter value)
    /// </summary>
    public static IEnumerable<T> GenerateMany<T>(uint era, int authority, ulong count, ulong offset = 0) where T: FakeRow , new()
    {
      for (ulong i = offset; i < count; i++)
      {
        yield return new T().Populate(new GDID(era, authority, i)) as T;
      }
    }
  }
}
