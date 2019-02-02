/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using Azos.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Web;
using Azos.Serialization.JSON;

namespace Azos.Tests.Unit.Web
{
  [Runnable(TRUN.BASE, 6)]
  public class ContentTypeMappingsTests
  {
    [Run]
    public void DefaultMappings_JustMount()
    {
      using(var app = new AzosApplication(null, null))
      {
        var maps = app.GetContentTypeMappings();
        Aver.IsNotNull(maps);
      }
    }

    [Run]
    public void DefaultMappings_Generic()
    {
      using (var app = new AzosApplication(null, null))
      {
        var maps = app.GetContentTypeMappings();
        Aver.IsNotNull(maps);

        Aver.IsNull( maps["cracks-pacs-facs"] );

        var map = maps.MapFileExtension("cracks-pacs-facs");
        Aver.IsNotNull(map);
        Aver.IsTrue(map.IsGeneric);
        Aver.IsTrue(map.IsBinary);
        Aver.IsFalse(map.IsText);
        Aver.IsFalse(map.IsImage);
        Aver.IsNotNull(map.Metadata);
        Aver.IsFalse(map.Metadata.Exists);

        Aver.AreSameRef(ContentType.Mapping.GENERIC_BINARY, map);
      }
    }

    [Run]
    public void DefaultMappings_Text()
    {
      using (var app = new AzosApplication(null, null))
      {
        var maps = app.GetContentTypeMappings();
        Aver.IsNotNull(maps);

        var map = maps.MapFileExtension("txt");
        Aver.IsNotNull(map);
        Aver.IsFalse(map.IsGeneric);
        Aver.IsFalse(map.IsBinary);
        Aver.IsTrue(map.IsText);
        Aver.IsFalse(map.IsImage);
        Aver.AreEqual("text/plain", map.ContentType);
        Aver.IsNotNull(map.Metadata);
        Aver.IsFalse(map.Metadata.Exists);

        Console.WriteLine(map.ToJSON());
      }
    }

    [Run]
    public void DefaultMappings_MapFileExt_MapContentType()
    {
      using (var app = new AzosApplication(null, null))
      {
        var maps = app.GetContentTypeMappings();
        Aver.IsNotNull(maps);

        var map = maps.MapFileExtension("txt");
        Aver.IsNotNull(map);
        Aver.AreEqual("text/plain", map.ContentType);

        var mappings = maps.MapContentType("text/plain").ToArray();
        Aver.AreEqual(1, mappings.Length);
        Aver.AreSameRef(map, mappings[0]);
      }
    }

    [Run]
    public void DefaultMappings_htm()
    {
      using (var app = new AzosApplication(null, null))
      {
        var maps = app.GetContentTypeMappings();
        Aver.IsNotNull(maps);

        var map = maps.MapFileExtension("htm");
        Aver.IsNotNull(map);
        Aver.IsFalse(map.IsGeneric);
        Aver.IsFalse(map.IsBinary);
        Aver.IsTrue(map.IsText);
        Aver.IsFalse(map.IsImage);
        Aver.AreEqual("text/html", map.ContentType);
        Aver.IsNotNull(map.Metadata);
        Aver.IsFalse(map.Metadata.Exists);

        Console.WriteLine(map.ToJSON());
      }
    }

    [Run]
    public void DefaultMappings_htm_multiple_exts()
    {
      using (var app = new AzosApplication(null, null))
      {
        var maps = app.GetContentTypeMappings();
        Aver.IsNotNull(maps);

        var map1 = maps.MapFileExtension("htm");
        Aver.IsNotNull(map1);
        Aver.IsFalse(map1.IsGeneric);
        Aver.IsFalse(map1.IsBinary);
        Aver.IsTrue(map1.IsText);
        Aver.IsFalse(map1.IsImage);
        Aver.AreEqual("text/html", map1.ContentType);
        Aver.IsNotNull(map1.Metadata);
        Aver.IsFalse(map1.Metadata.Exists);

        var map2 = maps.MapFileExtension("html");
        Aver.IsNotNull(map2);
        Aver.AreSameRef(map1, map2);
      }
    }

    [Run]
    public void DefaultMappings_png()
    {
      using (var app = new AzosApplication(null, null))
      {
        var maps = app.GetContentTypeMappings();
        Aver.IsNotNull(maps);

        var map = maps.MapFileExtension("png");
        Aver.IsNotNull(map);
        Aver.IsFalse(map.IsGeneric);
        Aver.IsTrue(map.IsBinary);
        Aver.IsFalse(map.IsText);
        Aver.IsTrue(map.IsImage);
        Aver.AreEqual("image/png", map.ContentType);
        Aver.IsNotNull(map.Metadata);
        Aver.IsFalse(map.Metadata.Exists);

        Console.WriteLine(map.ToJSON());
      }
    }


    [Run]
    public void Override_png()
    {
      var cfg = @"
app
{
  web-settings
  {
    content-type-mappings
    {
        map
        {
            extensions='png'
            content-type='image/custom-picture'
            binary=true image=true
            name{eng{n='PNG' d='My custom description'}}
        }
    }
  }
}
".AsLaconicConfig();

      using (var app = new AzosApplication(null, cfg))
      {
        var maps = app.GetContentTypeMappings();
        Aver.IsNotNull(maps);

        var map = maps.MapFileExtension("png");
        Aver.IsNotNull(map);
        Aver.IsFalse(map.IsGeneric);
        Aver.IsTrue(map.IsBinary);
        Aver.IsFalse(map.IsText);
        Aver.IsTrue(map.IsImage);
        Aver.AreEqual("image/custom-picture", map.ContentType);
        Aver.IsNotNull(map.Metadata);
        Aver.IsFalse(map.Metadata.Exists);

        Aver.AreEqual("My custom description", map.Name["eng"].Description);

        Console.WriteLine(map.ToJSON());
      }
    }

    [Run]
    public void WithMetadata()
    {
      var cfg = @"
app
{
  web-settings
  {
    content-type-mappings
    {
        map
        {
            extensions='xyz'
            content-type='custom/xyz'
            binary=true
            name{eng{n='XYZ' d='XYZ Custom Content'} zzz{n='XYZ' d='XYZ Pubra Manubra'}}
            meta
            {
               a=1 b=2
               actors
               {
                 actor{name='Man1'}
                 actor{name='Man2'}
               }
            }
        }
    }
  }
}
".AsLaconicConfig();

      using (var app = new AzosApplication(null, cfg))
      {
        var maps = app.GetContentTypeMappings();
        Aver.IsNotNull(maps);

        var map = maps.MapFileExtension("xyz");
        Aver.IsNotNull(map);
        Aver.IsFalse(map.IsGeneric);
        Aver.IsTrue(map.IsBinary);
        Aver.IsFalse(map.IsText);
        Aver.IsFalse(map.IsImage);
        Aver.AreEqual("custom/xyz", map.ContentType);

        Aver.AreEqual("XYZ Custom Content", map.Name["eng"].Description);
        Aver.AreEqual("XYZ Pubra Manubra", map.Name["zzz"].Description);

        Aver.IsNotNull(map.Metadata);
        Aver.IsTrue(map.Metadata.Exists);

        Aver.AreEqual("Man1", map.Metadata.Navigate("/actors/[0]/$name").Value);
        Aver.AreEqual("Man2", map.Metadata.Navigate("/actors/[1]/$name").Value);

        Console.WriteLine(map.ToJSON());
      }
    }


    [Run]
    public void ManyToMany()
    {
      var cfg = @"
app
{
  web-settings
  {
    content-type-mappings
    {
        map
        {
          extensions='f1,f2'
          content-type='custom/xyz'
          binary=true
        }

        map
        {
          extensions='f3'
          content-type='custom/xyz'
          binary=false
          name{eng{n='F3' d='F3 file'}}
        }
    }
  }
}
".AsLaconicConfig();

      using (var app = new AzosApplication(null, cfg))
      {
        var maps = app.GetContentTypeMappings();
        Aver.IsNotNull(maps);

        var map1 = maps.MapFileExtension("f1");
        var map2 = maps.MapFileExtension("f2");
        var map3 = maps.MapFileExtension("f3");
        Aver.IsNotNull(map1);
        Aver.IsNotNull(map2);
        Aver.IsNotNull(map3);

        Aver.AreSameRef(map1, map2);
        Aver.AreNotSameRef(map2, map3);

        Aver.IsFalse(map1.IsGeneric);
        Aver.IsFalse(map3.IsGeneric);
        Aver.IsTrue(map1.IsBinary);
        Aver.IsFalse(map3.IsBinary);
        Aver.AreEqual("custom/xyz", map1.ContentType);
        Aver.AreEqual("custom/xyz", map3.ContentType);

        Aver.IsNull(map1.Name["eng"].Description);
        Aver.AreEqual("F3 file", map3.Name["eng"].Description);

        var mappings = maps.MapContentType("custom/xyz").ToArray();
        Aver.AreEqual(2, mappings.Length);
        Aver.AreSameRef(map1, mappings[0]);
        Aver.AreSameRef(map3, mappings[1]);

      }
    }


  }
}
