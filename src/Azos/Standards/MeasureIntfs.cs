/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

namespace Azos.Standards
{
  /// <summary>
  /// Marker interface for units of measure such as:  Distance, Weight, Area, Volume, Time, and Temperature.
  /// Measures are used to store, transmit and process data, such as product/item measurements in eCommerce, manufacturing, logistics etc.
  /// A typical use case is Bill of Materials (BOM) used in product manufacturing, product catalog/CPQ (Configure, Price, Quote),
  /// inventory/warehouse management and shipping systems. <br/><br/>
  /// Please note: the 3d modeling and other software may use different units, such as `double` or `float` for performance.
  /// The `IMeasure` interface is primarily built for accurate data storage, transmission and processing where maximum data veracity
  /// is required. When you need to work with other systems, such as 3d modeling, you may want to use `double` or `float` types as required, however
  /// the data conversion should be applied POST LOGICAL/BUSINESS PROCESSING which is based on `decimal` type in IMeasure interface
  /// </summary>
  public interface IMeasure
  {
  }

  /// <summary>
  /// <inheritdoc cref="IMeasure"/> <br/><br/>
  /// IScalarNMeasure is a marker interface for scalar (single number) measures such as:  Distance, Weight, Area, Volume, Time, and Temperature, all of which have a single real number,
  /// unlike composite measures such as product dimensions (width x height x depth)
  /// </summary>
  public interface IScalarMeasure : IMeasure
  {
    /// <summary>
    /// Measured value expressed in the unit of measure.
    /// The system purposely uses decimal and not double to avoid precision issues when multiple measurements need to be processed.
    /// The behavior is akin to how money is handled in the system, where precision is critical, because we
    /// may need to tally up many measurements and we do not want to lose precision.
    /// </summary>
    decimal Value { get; }

    /// <summary> Unit name string (e.g. "meter", "C" etc.)</summary>
    string UnitName { get; }
  }

  /// <summary>
  /// <inheritdoc cref="IMeasure"/> <br/><br/>
  /// ICompositeMeasure interface is used for complex (composite) measures that consist of multiple measures, such as product dimensions (width x height x depth).
  /// </summary>
  public interface ICompositeMeasure : IMeasure
  {
    /// <summary>
    /// Values of the composite measure is a collection key value pairs: measure name and measure value.
    /// </summary>
    IEnumerable<KeyValuePair<string, IMeasure>> Values { get; }
  }

}