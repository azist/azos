/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.AuthKit
{
  public static class Constraints
  {
    public const int LOGIN_ID_MIN_LEN = 1;
    public const int LOGIN_ID_MAX_LEN = 2048;

    public const int LOGIN_PWD_MIN_LEN = 2;// { }
    public const int LOGIN_PWD_MAX_LEN = 2048;// { }

    public const int RIGHTS_MAX_LEN = 0xff * 1024;
  }
}
