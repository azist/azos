/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using Azos.Financial;
using Azos.Geometry;
using Azos.Time;

namespace Azos.Data
{
  partial class SchemaSerializer
  {
    //primitive types, DO NOT include nullables
    private static readonly Dictionary<Type, string> PRIMITIVE_TYPES = new() {
      {typeof(string), "string"},
      {typeof(char),   "char"},
      {typeof(bool),   "bool"},
      {typeof(DateTime), "date"},
      {typeof(Guid),     "guid"},

      {typeof(sbyte), "sbyte"}, {typeof(byte),   "byte"},
      {typeof(short), "short"}, {typeof(ushort), "ushort"},
      {typeof(int),   "int"},   {typeof(uint),   "uint"},
      {typeof(long),  "long"},  {typeof(ulong),  "ulong"},
      {typeof(float),   "float"},
      {typeof(double),  "double"},
      {typeof(decimal), "decimal"},

      //Structured types: their value is represented as either a string or a map
      {typeof(Amount),   "str:amount"},
      {typeof(Atom),     "str:atom"},
      {typeof(EntityId), "str:eid"},
      {typeof(GDID),     "str:gdid"},
      {typeof(RGDID),    "str:rgdid"},
      {typeof(DateRange),"str:dtrng"},
      {typeof(LatLng),   "str:latlng"},

    };

    private static readonly Dictionary<string, Type> PRIMITIVE_MONIKERS = new(PRIMITIVE_TYPES.Select(kvp => new KeyValuePair<string, Type>(kvp.Value, kvp.Key)));
  }
}
