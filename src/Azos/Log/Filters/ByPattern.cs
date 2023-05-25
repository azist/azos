/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting.Expressions;

namespace Azos.Log.Filters
{
  /// <summary>
  /// Performs pattern set search on Message.Topic
  /// </summary>
  public sealed class ByTopic : PatternSetFilter<Message>
  {
    protected override string GetValue(Message context) => context?.Topic;
  }

  /// <summary>
  /// Performs pattern set search on Message.From
  /// </summary>
  public sealed class ByFrom : PatternSetFilter<Message>
  {
    protected override string GetValue(Message context) => context?.From;
  }

  /// <summary>
  /// Performs pattern set search on Message.Text
  /// </summary>
  public sealed class ByText : PatternSetFilter<Message>
  {
    protected override string GetValue(Message context) => context?.Text;
  }


  /// <summary>
  /// Performs pattern set search on Message.Host
  /// </summary>
  public sealed class ByHost : PatternSetFilter<Message>
  {
    protected override string GetValue(Message context) => context?.Host;
  }

  /// <summary>
  /// Performs pattern set search on Message.Channel
  /// </summary>
  public sealed class ByChannel : AtomSetFilter<Message>
  {
    protected override Atom GetValue(Message context) => context?.Channel ?? Atom.ZERO;
  }

  /// <summary>
  /// Performs pattern set search on Message.App
  /// </summary>
  public sealed class ByApp : PatternSetFilter<Message>
  {
    protected override string GetValue(Message context) => context?.App.Value;
  }


}
