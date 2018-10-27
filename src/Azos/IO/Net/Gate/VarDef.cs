/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;

namespace Azos.IO.Net.Gate
{

    /// <summary>
    /// Provides variable definition - the name and parameters how fast a variable decays - loses its value towards 0 when it gets deleted
    /// </summary>
    public class VarDef : Collections.INamed
    {
         public const int DEFAULT_DECAY_BY = 1;
         public const int DEFAULT_INTERVAL_SEC = 10;


         public VarDef(string name)
         {
            if (name.IsNullOrWhiteSpace())
              throw new NetGateException(StringConsts.NETGATE_VARDEF_NAME_EMPTY_CTOR_ERROR);
            m_Name = name;
         }

         public VarDef(IConfigSectionNode node)
                  : this(node.NonNull(text: "VarDef.ctor(node==null)").AttrByName(Configuration.CONFIG_NAME_ATTR).Value)
         {
           ConfigAttribute.Apply(this, node);
         }



         [Config]
         private string m_Name;
         private int m_DecayBy     = DEFAULT_DECAY_BY;
         private int m_IntervalSec = DEFAULT_INTERVAL_SEC;


         public string Name { get{ return m_Name;}}

         [Config]
         public int DecayBy
         {
           get{ return m_DecayBy;}
           set
           {
              value = Math.Abs(value);
              m_DecayBy = value<1 ? 1 : value;
           }
         }

         [Config(Default=DEFAULT_INTERVAL_SEC)]
         public int IntervalSec
         {
            get{ return m_IntervalSec;}
            set
            {
              m_IntervalSec = value<1 ? 1 : value;
            }
         }
    }
}
