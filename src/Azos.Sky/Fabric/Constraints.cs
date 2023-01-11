﻿/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

namespace Azos.Sky.Fabric
{
  public static class Constraints
  {
    public const int MAX_TAG_COUNT = 32;
    public const int MAX_IMPERSONATE_LEN = 255;

    public const int MAX_GROUP_LEN = 127;
    public const int MAX_INITIATOR_LEN = 127;
    public const int MAX_OWNER_LEN = 127;

    public const int MAX_DESCRIPTION_LEN = 100;

    public const float PRIORITY_MIN =   0.01f;
    public const float PRIORITY_MAX = 100.00f;
  }
}
