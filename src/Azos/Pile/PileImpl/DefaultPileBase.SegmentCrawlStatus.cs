/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Pile{ public abstract partial class DefaultPileBase{

  /// <summary>
  /// Holds information obtained after a segment crawl
  /// </summary>
  public struct SegmentCrawlStatus
  {
      internal SegmentCrawlStatus
      (
        long objectCount, long objectLinkCount,
        long crawledChunks, long originalFreeChunks, long resultFreeChunks, long freePayloadSize, long usedPayloadSize
      )
      {
        ObjectCount = objectCount;
        ObjectLinkCount = objectLinkCount;

        CrawledChunks      = crawledChunks;
        OriginalFreeChunks = originalFreeChunks;
        ResultFreeChunks   = resultFreeChunks;
        FreePayloadSize    = freePayloadSize;
        UsedPayloadSize    = usedPayloadSize;
      }

      public readonly long ObjectCount;
      public readonly long ObjectLinkCount;

      public readonly long CrawledChunks;
      public readonly long OriginalFreeChunks;
      public readonly long ResultFreeChunks;
      public readonly long FreePayloadSize;
      public readonly long UsedPayloadSize;

      public SegmentCrawlStatus Sum(SegmentCrawlStatus other)
      {
        return new SegmentCrawlStatus(
          ObjectCount     + other.ObjectCount,
          ObjectLinkCount + other.ObjectLinkCount,

          CrawledChunks      + other.CrawledChunks       ,
          OriginalFreeChunks + other.OriginalFreeChunks  ,
          ResultFreeChunks   + other.ResultFreeChunks    ,
          FreePayloadSize    + other.FreePayloadSize     ,
          UsedPayloadSize    + other.UsedPayloadSize
        );
      }

      public override string ToString()
      {
        return "Chunks crawled: {0:n0}, Free chunks orig: {1:n0}, Free chunks: {2:n0}, Free sz: {3:n0}, Used sz: {4:n0}"
              .Args(CrawledChunks, OriginalFreeChunks, ResultFreeChunks, FreePayloadSize,  UsedPayloadSize);
      }
  }

}}
