using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Conf;
using Azos.Collections;
using Azos.IO.FileSystem;

namespace Azos.Sky.Metabase{ public sealed partial class Metabank{

    /// <summary>
    /// Base class for metadata section stored in metabank catalogs. Section represents a piece of metabase catalog that
    ///  can lazily load from the source file system. Contrary to monolithic application configurations, that load
    ///  at once from a single source (i.e. disk file), metadata class allows to wrap configuration and load segments of configuration
    ///   on a as-needed basis
    /// </summary>
    public abstract class Section : INamed
    {
        protected Section(Catalog catalog, string name, string path, FileSystemSession session)
        {
          if (name.IsNullOrWhiteSpace() || catalog==null || path.IsNullOrWhiteSpace())
            throw new MetabaseException(StringConsts.ARGUMENT_ERROR + GetType().Name+".ctor(catalog|name|path==null|empty");
          Catalog = catalog;
          Metabank = catalog.Metabank;
          m_Name = name;
          Path = Metabank.JoinPaths("", path);//ensure root path symbol i.e.

          m_LevelConfig = Metabank.getConfigFromFile(session, Metabank.JoinPaths(path, Metabank.CONFIG_SECTION_LEVEL_FILE)).Root;

          if (!m_LevelConfig.IsSameName(RootNodeName))
            throw new MetabaseException(StringConsts.METABASE_METADATA_CTOR_1_ERROR.Args(GetType().Name, RootNodeName, m_LevelConfig.Name));

          var cn = m_LevelConfig.AttrByName(Metabank.CONFIG_NAME_ATTR);
          if ( !cn.Exists || !m_LevelConfig.IsSameNameAttr(name))
            throw new MetabaseException(StringConsts.METABASE_METADATA_CTOR_2_ERROR.Args(GetType().Name, name, cn.ValueAsString(SysConsts.UNKNOWN_ENTITY)));

          if (!name.IsValidName())
            throw new MetabaseException(StringConsts.METABASE_METADATA_CTOR_3_ERROR.Args(GetType().Name, name, path));

          Metabank.includeCommonConfig(m_LevelConfig);
          m_LevelConfig.ResetModified();
        }

        public readonly Metabank Metabank;
        public readonly Catalog Catalog;

        /// <summary>
        /// File system path to this section with section name
        /// </summary>
        public readonly string Path;

        protected readonly string m_Name;
        private ConfigSectionNode m_LevelConfig;

        /// <summary>
        /// Section name
        /// </summary>
        public string Name { get { return m_Name; }}

        /// <summary>
        /// Section description
        /// </summary>
        public string Description { get { return m_LevelConfig.AttrByName(Metabank.CONFIG_DESCRIPTION_ATTR).ValueAsString(string.Empty); }}


        /// <summary>
        /// Returns metabase config for this section/level
        /// </summary>
        public IConfigSectionNode LevelConfig { get { return m_LevelConfig;} }

        /// <summary>
        /// Returns the name of root node in section level file
        /// </summary>
        public abstract string RootNodeName { get;}


        /// <summary>
        /// Validates metabase section by checking all of it contents for consistency
        /// </summary>
        public abstract void Validate(ValidationContext ctx);

        public override string ToString()
        {
          return "{0}({1})".Args(GetType().Name, this.Name);
        }

    }

    /// <summary>
    /// Metadata section that also has application-level configuration file for any application
    /// </summary>
    public abstract class SectionWithAnyAppConfig : Section
    {
        protected SectionWithAnyAppConfig(Catalog catalog, string name, string path, FileSystemSession session) : base(catalog, name, path, session)
        {
          m_AnyAppConfig = Metabank.getConfigFromFile(session, Metabank.JoinPaths(path, Metabank.CONFIG_SECTION_LEVEL_ANY_APP_FILE), require: false).Root;
        }

        private IConfigSectionNode m_AnyAppConfig;

        /// <summary>
        /// Any application(the one that does not specify a particular app name) config for this level or null
        /// </summary>
        public IConfigSectionNode AnyAppConfig { get { return m_AnyAppConfig;} }

        public override void Validate(ValidationContext ctx)
        {
           if (m_AnyAppConfig!=null)
            if (!m_AnyAppConfig.IsSameName(Metabank.RootAppConfig))
             ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this,
                         StringConsts.METABASE_VALIDATION_APP_CONFIG_ROOT_MISMTACH_ERROR.Args(m_AnyAppConfig.Name)) );
        }

    }


    /// <summary>
    /// Metadata section that also has application-level configuration files for any and named applications
    /// </summary>
    public abstract class SectionWithNamedAppConfigs : SectionWithAnyAppConfig
    {
        protected SectionWithNamedAppConfigs(Catalog catalog, string name, string path, FileSystemSession session) : base(catalog, name, path, session)
        {
          m_AppConfigs =
                Metabank.fsAccess("SectionWithAppConfig.ctor", path,
                      (fss, dir) =>
                      {
                        var result = new Dictionary<string, IConfigSectionNode>(StringComparer.InvariantCultureIgnoreCase);
                        foreach(var fn in dir.FileNames.Where(fn=>fn.StartsWith(Metabank.CONFIG_SECTION_LEVEL_APP_FILE_PREFIX)))
                        {
                          var ipr = fn.IndexOf(Metabank.CONFIG_SECTION_LEVEL_APP_FILE_PREFIX);
                          var si = ipr + Metabank.CONFIG_SECTION_LEVEL_APP_FILE_PREFIX.Length;
                          var isfx = fn.IndexOf(Metabank.CONFIG_SECTION_LEVEL_APP_FILE_SUFFIX);

                          if (ipr!=0 || isfx<=si) continue;
                          var appName = fn.Substring(si, isfx-si);

                          var app = this.Metabank.CatalogApp.Applications[appName];
                          if (app==null)
                           throw new MetabaseException(StringConsts.METABASE_APP_CONFIG_APP_DOESNT_EXIST_ERROR.Args(path, fn, appName));

                          var config = Metabank.getConfigFromExistingFile(session, Metabank.JoinPaths(dir.Path, fn)).Root;
                          result.Add(appName, config);
                        }
                        return result;
                      }
                );


        }

        private Dictionary<string, IConfigSectionNode> m_AppConfigs;

        /// <summary>
        /// Gets an application configuration file for the particular named application or null
        /// </summary>
        public IConfigSectionNode GetAppConfig(string appName)
        {
          IConfigSectionNode result;
          if (m_AppConfigs.TryGetValue(appName, out result)) return result;
          return null;
        }

        public override void Validate(ValidationContext ctx)
        {
           base.Validate(ctx);
           foreach(var name in m_AppConfigs.Keys)
           {
             var root = m_AppConfigs[name];
             if (root!=null)
              if (!root.IsSameName(Metabank.RootAppConfig))
               ctx.Output.Add( new MetabaseValidationMsg(MetabaseValidationMessageType.Error, Catalog, this,
                           StringConsts.METABASE_VALIDATION_APP_CONFIG_ROOT_MISMTACH_ERROR.Args(name+"."+root.Name)) );
           }
        }

    }


    public abstract class Section<TCatalog> : Section where TCatalog : Catalog
    {
        protected Section(TCatalog catalog, string name, string path, FileSystemSession session)
                        :base(catalog, name, path, session)
        {
        }


        public new TCatalog Catalog { get { return (TCatalog)base.Catalog;}}
    }

    /// <summary>
    /// Metadata section that also has application-level configuration file for any application
    /// </summary>
    public abstract class SectionWithAnyAppConfig<TCatalog> : SectionWithAnyAppConfig where TCatalog : Catalog
    {
        protected SectionWithAnyAppConfig(TCatalog catalog, string name, string path, FileSystemSession session)
                        :base(catalog, name, path, session)
        {
        }

        public new TCatalog Catalog { get { return (TCatalog)base.Catalog;}}
    }

    /// <summary>
    /// Metadata section that also has application-level configuration files for any and named applications
    /// </summary>
    public abstract class SectionWithNamedAppConfigs<TCatalog> : SectionWithNamedAppConfigs where TCatalog : Catalog
    {
        protected SectionWithNamedAppConfigs(TCatalog catalog, string name, string path, FileSystemSession session)
                        :base(catalog, name, path, session)
        {
        }

        public new TCatalog Catalog { get { return (TCatalog)base.Catalog;}}
    }



}}
