
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;

namespace Azos.Data.Modeling.DataTypes
{
    /// <summary>
    /// Represents a domain - a named type
    /// </summary>
    public abstract class Domain : Collections.INamed, IConfigurable
    {

        #region Properties

            /// <summary>
            /// Returns the name of this domain, i.e. 'THumanAge', 'TSalary'
            /// </summary>
            public virtual string Name
            {
                get { return this.GetType().Name; }
            }

        #endregion



        public virtual void Configure(IConfigSectionNode node)
        {
            ConfigAttribute.Apply(this, node);
        }
    }
}
