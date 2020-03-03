using System;

using Azos.Serialization.JSON;

namespace Azos.Security
{
  /// <summary>
  /// Describes an entity (e.g. a user/group/circle/client app) identified by Uri.
  /// This is used in secman.LookupEntityAsync(uri): IEntityInfo to look-up entities such as users, apps etc..
  /// The function is typically used to get user catalog details
  /// </summary>
  /// <remarks>
  /// WARNING: be careful to not unintentionally divulge too much of the private info to the public audience,
  /// as this structure may contain more data than the public should see. Instead, proxy just the public data elements via
  /// wrapper JSON object
  /// </remarks>
  public interface IEntityInfo
  {
    /// <summary>
    /// Uri of the entity used to fetch entity by thus Uri. The format is secman-specific
    /// </summary>
    string Uri {  get; }

    /// <summary>
    /// Localized name/description pair
    /// </summary>
    NLSMap Name{ get; }

    /// <summary>
    /// Primary/title image link or null if no image was set
    /// </summary>
    string TitleImage { get; }

    /// <summary>
    /// When was the entity created/appeared in the system (e.g. "Member since..."/"Established ... ago")
    /// </summary>
    DateTime? CreateDate { get; }
  }
}
