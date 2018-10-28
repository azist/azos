
using Azos.Collections;

namespace Azos.Sky.Metabase{ public sealed partial class Metabank{

      /// <summary>
      /// Represents a catalog in metabase - a top-level folder in the metabase file system that contains
      ///  logically-grouped data. Catalog implementers contain various instances of Section-derived classes that represent pieces of metabase whole data that
      ///  can lazily load from the source file system. Contrary to monolithic application configurations, that load
      ///  at once from a single source (i.e. disk file), metadata class allows to wrap configuration and load segments of configuration
      ///   on a as-needed basis
      /// </summary>
      public abstract class Catalog : INamed
      {
        #region CONSTS

        #endregion

        #region .ctor
            internal Catalog(Metabank bank, string name)
            {
              if (bank==null || name.IsNullOrWhiteSpace())
                throw new MetabaseException(StringConsts.ARGUMENT_ERROR + this.GetType().Name + ".ctor(bank==null|name==null|empty)");

              Metabank = bank;
              m_Name = name;

              Metabank.m_Catalogs.Register( this );
            }

        #endregion

        #region Fields

            public readonly Metabank  Metabank;
            protected readonly string m_Name;


        #endregion

        #region Properties

            ///// <summary>
            ///// References metabank instance that created this object
            ///// </summary>
            //public Metabank Metabank { get{ return m_Metabank;} }


            /// <summary>
            /// Returns catalog name
            /// </summary>
            public string Name { get{ return m_Name;}}


            /// <summary>
            /// Returns true to designate catalog instance as a system-recognized.
            /// System catalogs are the ones that metabank hard-codes, i.e. "Regions" is a hard-coded catalog that contains system information.
            /// Metabase can also contain user-definable catalogs in which case this property  will return false
            /// </summary>
            public bool IsSystem { get { return this is SystemCatalog; }}


        #endregion

        #region Public

            /// <summary>
            /// Validates catalog
            /// </summary>
            public abstract void Validate(ValidationContext ctx);

            public override string ToString()
            {
              return this.Name;
            }

        #endregion


      }//Catalog

      /// <summary>
      /// Denotes a system catalog
      /// </summary>
      public abstract class SystemCatalog : Catalog
      {
          protected SystemCatalog(Metabank bank, string name) : base(bank, name)
          {

          }
      }//SystemCatalog

      /// <summary>
      /// Denotes a user catalog
      /// </summary>
      public abstract class UserCatalog : Catalog
      {
          protected UserCatalog(Metabank bank, string name) : base(bank, name)
          {

          }
      }//UserCatalog


}}
