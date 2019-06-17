/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Data.Modeling
{
  /// <summary>
  /// Compiles relation schema into Microsoft Sql Server scripts
  /// </summary>
  public class MsSqlCompiler : RDBMSCompiler
  {
    public MsSqlCompiler(Schema schema) : base(schema) { }


    public override TargetType Target => TargetType.MsSQLServer;
    public override string Name => "MsSql";


    public override string GetStatementDelimiterScript(RDBMSEntityType type, bool start)
    {
      return start ? string.Empty : "\nGO\n";
    }

    public override string GetQuotedIdentifierName(RDBMSEntityType type, string name)
    {
      if (type!=RDBMSEntityType.Domain)
          return "[{0}]".Args(name);
      else
          return name;
    }

    public override void TransformEntityName(RDBMSEntity entity)
    {
      base.TransformEntityName(entity);

      switch (NameCaseSensitivity)
      {
        case NameCaseSensitivity.ToLower:
          entity.TransformedName = entity.TransformedName.ToLowerInvariant();
          entity.TransformedShortName = entity.TransformedShortName.ToLowerInvariant();
          break;

        case NameCaseSensitivity.ToUpper:
          entity.TransformedName = entity.TransformedName.ToUpperInvariant();
          entity.TransformedShortName = entity.TransformedShortName.ToUpperInvariant();
          break;
      }
    }

  }
}
