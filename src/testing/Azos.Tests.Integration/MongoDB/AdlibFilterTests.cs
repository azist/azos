/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

using Azos.Scripting;
using Azos.Data.Adlib;
using Azos.Data.Adlib.Server;
using Azos.Serialization.JSON;

namespace Azos.Tests.Integration.MongoDb
{
  [Runnable]
  public class AdlibFilterTests
  {
    [Run]
    public void Test01()
    {
      var JSON = @"
      {
      'tagfilter': {
        'Operator': 'and',

        'LeftOperand': {
            'Operator': '=',
            'LeftOperand': { 'Identifier': 'L'  },
            'RightOperand': { 'Value': 100    }
        },

        'RightOperand': {
            'Operator': 'and',
            'LeftOperand': {
              'Operator': '=',
              'LeftOperand': { 'Identifier': 'R-L' },
              'RightOperand': { 'Value': 2000}
             },
            'RightOperand': {
              'Operator': '=',
              'LeftOperand': { 'Identifier': 'R-R' },
              'RightOperand': { 'Value': 3000 }
          }
        }
      }
  }
  ";

      var filter = JsonReader.ToDoc<ItemFilter>(JSON);

      var qry = BsonConvert.GetFilterQuery(filter);

      qry.See();

    }
  }
}

/*

{
  "$and":
  [
       { "tags.p":  { "$eq": { "$numberLong": 76 } },
         "tags.v":  { "$eq": { "$numberLong": 100} }
       },
       [
          { "tags.p": { "$eq": { "$numberLong": 4992338 } },
            "tags.v":  { "$eq": { "$numberLong": 2000 } }
          },
          { "tags.p": {"$eq": { "$numberLong": 5385554 } },
            "tags.v": {"$eq": { "$numberLong": 3000    } }
          }
       ]
  ]
}

*/
