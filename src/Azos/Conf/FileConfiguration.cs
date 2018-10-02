
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Azos.Conf
{
  /// <summary>
  /// Provides file-based configuration base object used for concrete implementations such as XML or INI file base configurations
  /// </summary>
  [Serializable]
  public abstract class FileConfiguration : Configuration
  {
    #region .ctor

        /// <summary>
        /// Creates an instance of a new configuration not bound to any file
        /// </summary>
        protected FileConfiguration()
          : base()
        {

        }

        /// <summary>
        /// Creates an isntance of configuration and reads contents from the file
        /// </summary>
        protected FileConfiguration(string filename)
          : base()
        {
          m_FileName = filename;
        }

    #endregion

    #region Private Fields
        protected string m_FileName;

        private bool m_IsReadOnly;
    #endregion

    #region Properties
        public string FileName
        {
          get { return m_FileName; }
        }

        /// <summary>
        /// Indicates whether configuration is readonly or may be modified and saved
        /// </summary>
        public override bool IsReadOnly
        {
          get { return m_IsReadOnly; }
        }
    #endregion

    #region Public

        /// <summary>
        /// Saves configuration into specified file
        /// </summary>
        public virtual void SaveAs(string filename)
        {
          m_FileName = filename;

          if (m_Root != null)
            m_Root.ResetModified();
        }

        public void SetReadOnly(bool val)
        {
          m_IsReadOnly = val;
        }

    #endregion

    #region Protected

    #endregion

  }
}
