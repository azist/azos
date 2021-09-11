/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Data;

namespace Azos.Conf.Metabiz
{
  /// <summary>
  /// Path is either a string or gdid
  /// </summary>
  public struct MetabizPath : IEquatable<MetabizPath>
  {
    public MetabizPath(GDID g)
    {
      Gdid = g;
      Path = null;
    }

    public MetabizPath(string path)
    {
      Gdid = GDID.ZERO;
      Path = path.NonBlank(nameof(path));
    }

    public readonly GDID Gdid;
    public readonly string Path;

    public bool IsAssigned => Path.IsNotNullOrWhiteSpace() || !Gdid.IsZero;

    public bool Equals(MetabizPath other) => this.Gdid == other.Gdid && this.Path.EqualsOrdSenseCase(other.Path);

    public override int GetHashCode() => Gdid.GetHashCode() ^ (Path != null ? Path.GetHashCode() : 0);
    public override bool Equals(object obj) => obj is MetabizPath other ? this.Equals(other) : false;

    public override string ToString() => Gdid.IsZero ? Path : '#' + Gdid.ToHexString();

    public static bool operator ==(MetabizPath a, MetabizPath b) => a.Equals(b);
    public static bool operator !=(MetabizPath a, MetabizPath b) => !a.Equals(b);
  }
}
