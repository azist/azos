/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Azos.Conf;
using Azos.Serialization.JSON;

namespace Azos.Geometry
{
  /// <summary>
  /// Models a simple polygon shape (see formal definition) - a polygonal shape that does not intersect itself and has no holes.
  /// Complex topologies are modeled using a layering of "include" and "exclude" simple polygons.
  /// </summary>
  /// <seealso cref="https://en.wikipedia.org/wiki/Simple_polygon"/>
  public struct SimplePolygon : IShape, IEquatable<SimplePolygon>, Collections.INamed, IJsonReadable, IJsonWritable, IConfigurationPersistent
  {
    private string m_Name;
    private PointD[] m_Vertices;

    public string Name => m_Name;

    public ConfigSectionNode PersistConfiguration(ConfigSectionNode parentNode, string name)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Calculates a rectangle which bounds (covers) the whole polygon by latching onto its
    /// min and max bounding orthonormal lines
    /// </summary>
    public RectangleD GetBoundingRect()
    {
        return default;
    }

    /// <summary>
    /// Performs point hit test - if the point is within the polygon - returns true.
    /// The method is heavily used in cartography/mapping
    /// </summary>
    public bool HasPoint(PointD point)
    {
      return false;
    }

    public bool Equals(SimplePolygon other)
    {
      throw new NotImplementedException();
    }

    public (bool match, IJsonReadable self) ReadAsJson(object data, bool fromUI, JsonReader.DocReadOptions? options)
    {
      throw new NotImplementedException();
    }

    public void WriteAsJson(TextWriter wri, int nestingLevel, JsonWritingOptions options = null)
    {
      throw new NotImplementedException();
    }
  }
}
