/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Data.Modeling
{

    /// <summary>
    /// Compiles relation schema into MySQL scripts
    /// </summary>
    public class MySQLCompiler : RDBMSCompiler
    {
        #region .ctor

            public MySQLCompiler(Schema schema) : base(schema)
            {

            }


        #endregion

        #region Properties
            public override TargetType Target
            {
                get { return TargetType.MySQL;}//PostgreSQL; }// .MySQL; }
            }

            public override string Name
            {
                get { return "MySQL"; }
            }
        #endregion


        #region Protected

            public override string GetQuotedIdentifierName(RDBMSEntityType type, string name)
            {
                if (type!=RDBMSEntityType.Domain)
                    return "`{0}`".Args(name);
                else
                    return name;
            }

            public override string GetStatementDelimiterScript(RDBMSEntityType type, bool start)
            {
                return start ? "delimiter ;." : ";.";
            }

            protected override string FormatColumnStatement(string column, string type, string nnull, string auto, string dflt, string chk, string comment)
            {
                const int TAB = 4;
                var cw = 16;
                while(column.Length>cw) cw+=TAB;

                var result = " {{0,-{0}}} {{1}}".Args(cw).Args(column, type).TrimEnd();

                var rw = 32;
                while(result.Length>rw) rw+=TAB;
                result = "{{0,-{0}}} {{1}}".Args(rw).Args(result, nnull);

                if (auto.IsNotNullOrWhiteSpace())
                    result = "{0}{1}{2}".Args(result, " ".PadLeft(result.Length%TAB), auto);

                if (dflt.IsNotNullOrWhiteSpace())
                    result = "{0}{1}{2}".Args(result, " ".PadLeft(result.Length%TAB), dflt);

                ////MySQL does not allow to mix check with comment
                ////and ignores check constraints anyway
                //if (chk.IsNotNullOrWhiteSpace())
                //    result = "{0}{1}{2}".Args(result, " ".PadLeft(result.Length%TAB), chk);


                if (comment.IsNotNullOrWhiteSpace())
                    result = "{0}{1}{2}".Args(result, " ".PadLeft(result.Length%TAB), comment);

                return result.TrimEnd();
            }


            public override void TransformEntityName(RDBMSEntity entity)
            {
              base.TransformEntityName(entity);

              if (entity.EntityType==RDBMSEntityType.Column)
              {
                entity.TransformedName = entity.TransformedName.ToUpperInvariant();
                entity.TransformedShortName = entity.TransformedShortName.ToUpperInvariant();
              }
            }
        #endregion

    }
}
