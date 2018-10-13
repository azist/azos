
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Azos.Conf;

namespace Azos.Glue.Protocol
{
    /// <summary>
    /// Represents a header base - custom headers must inherit from this class
    /// </summary>
    [Serializable]
    public abstract class Header
    {

    }



    /// <summary>
    /// List of headers
    /// </summary>
    [Serializable]
    public sealed class Headers : List<Header>
    {
    }


    /// <summary>
    /// Provides general configuration reading logic for headers.
    /// Note: This class is not invoked by default glue runtime, so default application
    ///  configurations that include header injections will be ignored unless this class is specifically called
    ///  from code. This is because conf-based header injection is a rare case that may need to be controlled by a particular application
    /// </summary>
    public static class HeaderConfigurator
    {
       #region CONSTS

            public const string CONFIG_HEADERS_SECTION = "headers";

            public const string CONFIG_HEADER_SECTION = "header";

       #endregion

       public static void ConfigureHeaders(IList<Header> headers, IConfigSectionNode node)
       {
         node = node[CONFIG_HEADERS_SECTION];
         if (!node.Exists) return;

         foreach(var hnode in node.Children.Where(c => c.IsSameName(CONFIG_HEADER_SECTION)))
         {
           var header = FactoryUtils.Make<Header>(hnode);

           var chdr = header as IConfigurable;
           if (chdr!=null) chdr.Configure(hnode);

           headers.Add(header);
         }

       }

    }


}
