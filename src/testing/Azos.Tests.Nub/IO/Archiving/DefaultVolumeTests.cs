/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using Azos.Apps;
using Azos.Scripting;
using Azos.IO.Archiving;

namespace Azos.Tests.Nub.IO.Archiving
{
  [Runnable]
  public class DefaultVolumeTests
  {
    [Run]
    public void Metadata_Create_Multiple_Sections_Mount()
    {
      //var ms = new FileStream("c:\\azos\\archive.lar", FileMode.Create);//  new MemoryStream();
      var ms = new MemoryStream();

      var meta = VolumeMetadataBuilder.Make("20210115-1745-doctor")
                                      .SetVersion(99, 21)
                                      .SetDescription("B vs D De-Terminator for doctor bubblegumization")
                                      .SetChannel(Atom.Encode("dvop"))
                                      .SetCompressionScheme("gzip;level=5")
                                      .SetEncryptionScheme("marsha-keys2")
                                      .SetApplicationSection(app => {
                                        app.AddChildNode("user").AddAttributeNode("id", 111222);
                                        app.AddChildNode("user").AddAttributeNode("id", 783945);
                                        app.AddAttributeNode("is-good", false);
                                        app.AddAttributeNode("king-signature", Platform.RandomGenerator.Instance.NextRandomWebSafeString(150, 150));
                                      })
                                      .SetApplicationSection(app => { app.AddAttributeNode("a", false); })
                                      .SetApplicationSection(app => { app.AddAttributeNode("b", true); })
                                      .SetCompressionSection(cmp => { cmp.AddAttributeNode("z", 41); })
                                      .SetEncryptionSection(enc => { enc.AddAttributeNode("z", 99); });

      var volume = new DefaultVolume(NOPApplication.Instance, meta, ms);

      volume.Dispose();
     // ms.GetBuffer().ToDumpString(DumpFormat.Hex).See();

      volume = new DefaultVolume(NOPApplication.Instance, ms);
      volume.Metadata.See();
    }
  }
}
