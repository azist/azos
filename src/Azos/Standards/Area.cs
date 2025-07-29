/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Azos.Conf;
using Azos.Data;
using Azos.Serialization.JSON;

namespace Azos.Standards
{
  /// <summary>
  /// Represents an area stored as microns squared with `Distance.Unit` squared unit of measure.
  /// The type is efficient for storage and transmission, as it uses a long integer to represent the area in microns squared
  /// which is sufficient for most engineering tasks such as room/floor/building areas, train/ship/cargo/truckload/container areas etc.
  /// The purpose of this type is to provide enough accuracy up to 1 micron squared for most common business needs while avoiding costly
  /// floating point/decimal operations. This structure is not suitable for very large areas (such as areas of continents, planets, etc..)
  /// as for these purposes you can use double/decimal values stored in much larger units of measure, such as meters squared as
  /// micron precision is not needed for such tasks.
  /// <br/>
  /// The maximum area that can be represented is -/+9,223,372,036,854,775,807 microns squared
  /// which equates to about 9.2 million meters squared or 9.2 square kilometers = 3.56 square miles or 99 million square feet.
  /// For reference, the area of the largest building in the world in 2025, the New Century Global Center in Chengdu, China, is
  /// about 1.7 million square meters = 18.3 million square feet.
  /// </summary>
  public readonly struct Area : IScalarMeasure //, IFormattable
  {

    /// <summary>
    /// Creates an instance from the specified square microns value
    /// </summary>
    public Area(Distance.UnitType unit, long sqMicronValue)
    {
      Unit = unit;
      ValueInSquareMicrons = sqMicronValue;
    }

    //WIP TBD

    /// <summary>
    /// Normalized area expressed in whole microns squared.
    /// The maximum precision supported by this type is 1 sq micron which is 1 millions^2 of a meter or 1 thousands^2 of a millimeter,
    /// hence the largest are representable is about 9.2 sq km or 99M sq ft which is enough for 99.9% of business use cases such as
    /// construction, engineering, logistics, shipping, etc.
    /// </summary>
    public readonly long ValueInSquareMicrons;

    /// <summary>
    /// Units of distance measurement, which is squared for area, e.g. "meter squared", "foot squared", etc.
    /// </summary>
    public readonly Distance.UnitType Unit;


    decimal IScalarMeasure.Value => throw new NotImplementedException();


    /// <summary> Provides unit name a s short string </summary>
    public string ShortUnitName => $"sq {Distance.GetUnitName(Unit, true)}";
    string IScalarMeasure.UnitName => ShortUnitName;

    /// <summary> Provides unit name as long string </summary>
    public string LongUnitName => $"sq {Distance.GetUnitName(Unit, false)}";
  }
}
