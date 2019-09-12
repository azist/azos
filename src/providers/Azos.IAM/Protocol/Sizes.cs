using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.IAM.Protocol
{
  /// <summary>
  /// Defines size limits/length constraints for entity fields/collections etc.
  /// </summary>
  public static class Sizes
  {
    public const int NAME_MIN = 1;
    public const int NAME_MAX = 32;

    public const int DESCRIPTION_MAX = 0xff;
    public const int NOTE_MAX = 4 * 1024;

    public const int ENTITY_ID_MIN = 1;
    public const int ENTITY_ID_MAX = 32;

    public const int ACCOUNT_TITLE_MAX = 32;

    public const int PROPERTY_COUNT_MAX = 512;


    public const int RIGHTS_DATA_MAX = 256 * 1024;

  }
}
