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
            'LeftOperand': { 'Identifier': 'LEFTER'  },
            'RightOperand': { 'Value': -123    }
        },

        'RightOperand': {
            'Operator': 'and',
            'LeftOperand': {
              'Operator': '=',
              'LeftOperand': { 'Identifier': 'RIGHT.LEFTER' },
              'RightOperand': { 'Value': -234}
             },
            'RightOperand': {
              'Operator': '=',
              'LeftOperand': { 'Identifier': 'RIGHT.RIGHTER' },
              'RightOperand': { 'Value': 345 }
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
