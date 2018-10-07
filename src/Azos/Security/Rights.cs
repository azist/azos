/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using Azos.Conf;

namespace Azos.Security
{
      /// <summary>
      /// User rights contains data about access levels to permissions in the system.
      /// Use Configuration internally to keep the data organized in hierarchical navigable structure.
      /// Configuration also allows to cross-link permission levels using vars and make acess level
      ///  dependent on settings on a particular machine using environmental vars
      /// </summary>
      public sealed class Rights
      {
        public const string CONFIG_ROOT_SECTION = "rights";

        private static Rights m_NoneInstance = new Rights( Configuration.NewEmptyRoot(CONFIG_ROOT_SECTION).Configuration );

        /// <summary>
        /// An instance that signifies an absence of any rights at all - complete access denied
        /// </summary>
        public static Rights None
        {
          get
          {
            return m_NoneInstance;
          }
        }

        public Rights(Configuration data)
        {
          m_Data = data;
        }

        private Configuration m_Data;

        public IConfigSectionNode Root
        {
          get { return m_Data.Root; }
        }
  }
}
