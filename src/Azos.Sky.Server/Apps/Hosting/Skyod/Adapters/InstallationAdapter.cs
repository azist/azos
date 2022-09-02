/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azos.Data;
using Azos.Serialization.Bix;

namespace Azos.Apps.Hosting.Skyod.Adapters
{
  /// <summary>
  /// Outlines protocol for activities related to software component installation
  /// </summary>
  public abstract class InstallationAdapter : AdapterBase
  {
    protected InstallationAdapter(SetComponent director) : base(director)
    {

    }

    public override IEnumerable<Type> SupportedRequestTypes
    {
      get
      {
        yield return typeof(InstallationGetCurrentPackageRequest);
        yield return typeof(InstallationGetPackageListRequest);
        yield return typeof(InstallationGetRepositoryPackageListRequest);
        yield return typeof(InstallationDownloadPackageRequest);
        yield return typeof(InstallationInstallPackageRequest);
      }
    }


    protected sealed override async Task<AdapterResponse> DoExecRequestAsync(AdapterRequest request)
    {
      ComponentDirector.IsManagedInstall.IsTrue("Support managed installation");
      var response = await DoExecActivationRequest(request.CastTo<InstallationRequest>());
      return response;
    }

    protected abstract Task<InstallationResponse> DoExecActivationRequest(InstallationRequest request);
  }


  public abstract class InstallationRequest : AdapterRequest { }
  public abstract class InstallationResponse : AdapterResponse { }


  [Bix("3eff55b3-05ec-4ef5-9e16-e47b6e89b2c3")]
  public class InstallationGetCurrentPackageRequest : InstallationRequest { }

  [Bix("ca3e46a9-079a-4e9e-ae86-0bd1b8b53fb8")]
  public class InstallationGetCurrentPackageResponse : InstallationResponse
  {
    [Field]
    public PackageInfo PackageInfo { get; set; }
  }


  [Bix("279245da-e572-4c7e-b947-0e5c2ba1c506")]
  public class InstallationGetPackageListRequest : InstallationRequest { }

  [Bix("5ba0a0de-aebf-48bc-9a4e-c917588bea97")]
  public class InstallationGetPackageListResponse : InstallationResponse
  {
    [Field]
    public PackageInfo[] PackageInfos { get; set; }
  }


  [Bix("7954905d-0c96-4d96-81ed-f14e488527a9")]
  public class InstallationGetRepositoryPackageListRequest : InstallationRequest { }

  [Bix("a016c3a5-acaf-412b-a9d7-61fcb081b79a")]
  public class InstallationGetRepositoryPackageListResponse : InstallationResponse
  {
    [Field]
    public PackageInfo[] PackageInfos { get; set; }
  }

  [Bix("7ab7e9e7-6477-4599-a6ad-4cf1a84bb903")]
  public class InstallationDownloadPackageRequest : InstallationRequest
  {
    [Field]
    public PackageInfo PackageInfo { get; set; }
  }

  [Bix("82edb39f-d369-41f1-abc0-783c2dca4233")]
  public class InstallationDownloadPackageResponse : InstallationResponse
  {
  }

  [Bix("c6f1d120-e0a7-404f-8923-4f5247648e6b")]
  public class InstallationInstallPackageRequest : InstallationRequest
  {
    [Field]
    public PackageInfo PackageInfo { get; set; }
  }

  [Bix("df1dca1f-b79f-4d5f-93fa-533e33bc0726")]
  public class InstallationInstallPackageResponse : InstallationResponse
  {
  }

}
