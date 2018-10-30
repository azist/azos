using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Azos.Apps;
using Azos.Conf;
using Azos.Data;
using Azos.Data.Access;
using Azos.Log;

using Azos.Sky.Contracts;

namespace Azos.Sky.Identification
{
  /// <summary>
  /// Generates Global Distributed IDs (GDID).
  /// This class is thread safe (for calling Generate)
  /// </summary>
  public sealed class GdidGenerator : ApplicationComponent, IConfigurable, IGdidProvider
  {
          #region CONSTS
            public const string CONFIG_GDID_SECTION  = "gdid";
            public const string CONFIG_AUTHORITY_SECTION  = "authority";

          #endregion

          #region Inner Classes

                 internal class scope : INamed
                 {
                    public string Name { get; set; }
                    public Registry<sequence> Sequences = new Registry<sequence>();
                 }

                 internal class sequence : INamed
                 {
                    internal scope Scope {get; set; }
                    public string Name { get ; set; }
                    public GdidBlock Block;

                    //the following are used for LWM condition handling
                    public volatile GdidBlock NextBlock;
                    public volatile Task FetchingNextBlock;
                    public DateTime LastAllocationUTC = DateTime.UtcNow.AddSeconds(-1000);

                    public double avgSinceLastAlloc1;
                    public double avgSinceLastAlloc2;
                    public double avgSinceLastAlloc3;
                 }

                 /// <summary>
                 /// Describes a status of the named sequence
                 /// </summary>
                 public class SequenceInfo : ISequenceInfo
                 {
                    internal SequenceInfo(sequence seq)
                    {
                      Name = seq.Name;
                      var block = seq.Block;
                      if (block!=null)
                      {
                        Authority = block.Authority;
                        AuthorityHost = block.AuthorityHost;
                        Era = block.Era;
                        Value     = block.StartCounterInclusive + (ulong)(block.BlockSize  - block.__Remaining);
                        UTCTime   = block.ServerUTCTime;
                        BlockSize = block.BlockSize;
                        Remaining = block.__Remaining;
                      }
                      NextBlock = seq.NextBlock!=null;
                    }

                    public readonly string Name;
                    public readonly int    Authority;
                    public readonly string AuthorityHost;
                    public readonly uint  Era;
                    public readonly ulong  Value;
                    public readonly DateTime UTCTime;
                    public readonly int   BlockSize;
                    public readonly int   Remaining;
                    public readonly bool  NextBlock;


                    uint ISequenceInfo.Era
                    {
                      get { return this.Era; }
                    }

                    ulong ISequenceInfo.ApproximateCurrentValue
                    {
                      get { return Value; }
                    }

                    DateTime ISequenceInfo.IssueUTCDate
                    {
                      get { return UTCTime; }
                    }

                    string ISequenceInfo.IssuerName
                    {
                      get { return "[{0}] {1}".Args(Authority, AuthorityHost); }
                    }

                    int ISequenceInfo.RemainingPreallocation
                    {
                      get { return Remaining; }
                    }

                    int ISequenceInfo.TotalPreallocation
                    {
                      get { return BlockSize; }
                    }

                    string INamed.Name
                    {
                      get { return Name; }
                    }
                 }


                 /// <summary>
                 /// Provides information about the host that runs GDID generation authority
                 /// </summary>
                 public sealed  class AuthorityHost : INamed, IEquatable<AuthorityHost>
                 {
                    /// <summary>
                    /// Name of the host and distance from the caller. If distance =0 then it will be calculated
                    /// </summary>
                    public AuthorityHost(string name, int distanceKm = 0)
                    {
                      m_Name = name.IsNullOrWhiteSpace() ? SysConsts.NULL : name;
                      if (SkySystem.IsMetabase)
                        m_DistanceKm = distanceKm>0 ? distanceKm : (int)SkySystem.Metabase.CatalogReg.GetDistanceBetweenPaths(SkySystem.HostName, name);
                      else
                        m_DistanceKm = distanceKm;

                    }
                    private string m_Name;
                    private int m_DistanceKm;


                    /// <summary>
                    /// Host name
                    /// </summary>
                    public string Name{ get {return m_Name;} }

                    /// <summary>
                    /// Relative distance to the destination host from this machine
                    /// </summary>
                    public int DistanceKm { get {return m_DistanceKm;} }

                    /// <summary>
                    /// Returns enumeration of Global Distributed ID generation Authorities.
                    /// The distance is computed, and can not be specified in config
                    /// </summary>
                    public static IEnumerable<AuthorityHost> FromConfNode(IConfigSectionNode parentNode)
                    {
                      if (parentNode==null || !parentNode.Exists)
                        yield break;

                      foreach(var anode in parentNode.Children.Where(n=>n.IsSameName(CONFIG_AUTHORITY_SECTION)))
                      {
                        var name = anode.AttrByName(Metabase.Metabank.CONFIG_NAME_ATTR).Value;

                        if (name.IsNullOrWhiteSpace())
                          name = anode.AttrByName(Metabase.Metabank.CONFIG_NETWORK_ROUTING_HOST_ATTR).Value;

                        if (name.IsNullOrWhiteSpace())
                          continue;

                        yield return new Identification.GdidGenerator.AuthorityHost(name);
                      }
                    }


                    public bool Equals(AuthorityHost other)
                    {
                      if (other==null) return false;

                      return Name.IsSameRegionPath(other.Name);
                    }

                    public override bool Equals(object obj)
                    {
                      return this.Equals(obj as AuthorityHost);
                    }

                    public override int GetHashCode()
                    {
                      return Name.GetRegionPathHashCode();
                    }

                    public override string ToString()
                    {
                      return Name;
                    }
                 }


          #endregion

    #region CONSTS

      /// <summary>
      /// Specifies the level of "free" ID range in block below which reservation of the next block gets triggered
      /// </summary>
      public const double BLOCK_LOW_WATER_MARK = 0.25d;


    #endregion

    #region .ctor

      public GdidGenerator() : this(null, null, null, null) {}
      public GdidGenerator(string name, object director) : this(name, director, null, null) {}
      public GdidGenerator(string name, object director, string scopePrefix, string sequencePrefix) : base(director)
      {
        if (name.IsNullOrWhiteSpace())
          name = Guid.NewGuid().ToString();

        m_Name = name;

        m_ScopePrefix = scopePrefix;
        if (m_ScopePrefix!=null)  m_ScopePrefix = m_ScopePrefix.Trim();

        m_SequencePrefix = sequencePrefix;
        if (m_SequencePrefix!=null) m_SequencePrefix = m_SequencePrefix.Trim();
      }


    #endregion

    #region Fields

      private string m_Name;
      private Registry<scope> m_Scopes = new Registry<scope>();
      private Registry<AuthorityHost> m_AuthorityHosts = new Registry<AuthorityHost>();

      private bool m_BlockWasAllocated;
      private string m_TestingAuthorityNode;

      private string m_ScopePrefix;
      private string m_SequencePrefix;

    #endregion

    #region Properties


      public override string ComponentCommonName { get { return "gdidgen-"+Name; }}

      /// <summary>
      /// Name of the instance
      /// </summary>
      public string Name
      {
        get { return m_Name; }
      }


      /// <summary>
      /// Returns immutable scope prefix set in .ctor or null. This name is prepended to all requests
      /// </summary>
      public string ScopePrefix    {    get{ return m_ScopePrefix;} }

      /// <summary>
      /// Returns immutable sequence prefix set in .ctor or null. This name is prepended to all requests
      /// </summary>
      public string SequencePrefix { get{ return m_SequencePrefix;} }


      /// <summary>
      /// Returns host names that host GDID generation authority service
      /// </summary>
      public Registry<AuthorityHost> AuthorityHosts
      {
        get { return m_AuthorityHosts; }
      }

      /// <summary>
      /// Returns the list of all scope names in the instance
      /// </summary>
      public IEnumerable<string> SequenceScopeNames
      {
        get { return m_Scopes.Select(s => s.Name);}
      }


      /// <summary>
      /// Gets/sets Authority Glue Node for testing. It can only be set once in the testing app container init before the first call to
      ///  Generate is made. When this setting is set then any authority nodes which would have been normally used will be
      ///  completely bypassed during block allocation
      /// </summary>
      public string TestingAuthorityNode
      {
        get { return m_TestingAuthorityNode;}
        set
        {
         if (m_BlockWasAllocated)
            throw new GdidException(StringConsts.GDIDGEN_SET_TESTING_ERROR);
          m_TestingAuthorityNode = value;
        }
      }

      /// <summary>
      /// Returns true after block was allocated at least once
      /// </summary>
      public bool BlockWasAllocated
      {
        get { return m_BlockWasAllocated;}
      }

    #endregion

    #region Public

      public void Configure(IConfigSectionNode node)
      {
        if (node!=null)
          ConfigAttribute.Apply(this, node);
      }


      /// <summary>
      /// Returns sequnce information enumerable for all sequences in the named scope
      /// </summary>
      public IEnumerable<SequenceInfo> GetSequenceInfos(string scopeName)
      {
        if (scopeName==null)
          throw new GdidException(StringConsts.ARGUMENT_ERROR+GetType().Name+".GetSequenceInfos(scopeName=null)");

        if (m_ScopePrefix!=null)
         scopeName = m_ScopePrefix + scopeName;

        var scope = m_Scopes[scopeName];
        if (scope==null)
          return Enumerable.Empty<SequenceInfo>();

        return scope.Sequences.Select(s => new SequenceInfo(s));
      }

      IEnumerable<ISequenceInfo> IUniqueSequenceProvider.GetSequenceInfos(string scopeName)
      {
        return this.GetSequenceInfos(scopeName);
      }

      ulong IUniqueSequenceProvider.GenerateOneSequenceID(string scopeName, string sequenceName, int blockSize, ulong? vicinity, bool noLWM)
      {
        return GenerateOneGDID(scopeName, sequenceName, blockSize, vicinity, noLWM).ID;
      }

      ConsecutiveUniqueSequenceIDs IUniqueSequenceProvider.TryGenerateManyConsecutiveSequenceIDs(string scopeName,
                                                                                string sequenceName,
                                                                                int idCount,
                                                                                ulong? vicinity,
                                                                                bool noLWM)
      {
        var gdids = TryGenerateManyConsecutiveGDIDs(scopeName, sequenceName, idCount, vicinity, noLWM);
        return new ConsecutiveUniqueSequenceIDs(gdids[0].ID, gdids.Length);
      }



      /// <summary>
      /// Generates a single Globally-Unique distributed ID (GDID) for the supplied sequence name. This method is thread-safe.
      /// The algorithm also respects LWM value, once the number of free counters gets below LWM the asynchronous non-blocking acquisition from authority is triggered
      /// </summary>
      /// <param name="scopeName">The name of scope where sequences are kept</param>
      /// <param name="sequenceName">The name of sequence within the scope for which ID to be obtained</param>
      /// <param name="blockSize">If >0 requests to pre-allocate specified number of sequences, otherwise the generator will pre-allocate the most suitable/configured size</param>
      /// <param name="vicinity">The location on ID counter scale, the authority may disregard this parameter</param>
      /// <param name="noLWM">
      ///  When true, does not start async fetch of the next ID block while the current block reaches low-water-mark.
      ///  This may not be desired in some short-lived processes.
      ///  The provider may disregard this flag
      /// </param>
      /// <returns>The GDID instance</returns>
      public GDID GenerateOneGDID(string scopeName, string sequenceName, int blockSize=0, ulong? vicinity = GDID.COUNTER_MAX, bool noLWM = false)
      {
        if (scopeName==null || sequenceName==null)
          throw new GdidException(StringConsts.ARGUMENT_ERROR+GetType().Name+".GenerateOneGDID(scopeName|sequenceName=null)");

        if (m_ScopePrefix!=null) scopeName = m_ScopePrefix + scopeName;
        if (m_SequencePrefix!=null) sequenceName = m_SequencePrefix + sequenceName;

        scopeName = scopeName.Trim();
        sequenceName = sequenceName.Trim();

        GdidAuthorityService.CheckNameValidity(scopeName);
        GdidAuthorityService.CheckNameValidity(sequenceName);

        var scope = m_Scopes.GetOrRegister(scopeName, (snm) => new scope{Name = snm}, scopeName);

        var sequence = scope.Sequences.GetOrRegister(sequenceName, (_) => new sequence{Scope = scope, Name = sequenceName}, 0);//with block=NULL


        lock(sequence)
        {
            var block = sequence.Block;

            if (block==null || block.__Remaining<=0)//need to get Next
            {
               block = sequence.NextBlock;//atomic

               if (block==null)
                  block = allocateBlock(sequence, blockSize, vicinity);
               else
               {
                  sequence.NextBlock = null;
                  sequence.FetchingNextBlock = null;
               }

               sequence.Block = block;
               block.__Remaining = block.BlockSize;
            }


            var counter = block.StartCounterInclusive + (ulong)(block.BlockSize - block.__Remaining);
            block.__Remaining--;

            var result = new GDID(block.Era, block.Authority, counter);

            //check LWM
            if (!noLWM && block.BlockSize>7 && sequence.FetchingNextBlock==null)
            {
              double curlvl = (double)block.__Remaining / (double)block.BlockSize;
              if (curlvl <= BLOCK_LOW_WATER_MARK) //start fetching next block
              {
                sequence.FetchingNextBlock = Task.Factory.StartNew(()=>
                  {
                    try
                    {
                      var nextBlock = allocateBlock(sequence, blockSize, vicinity);
                      sequence.NextBlock = nextBlock;//atomic assignment
                    }
                    catch(Exception error)
                    { //todo Perf counter
                      log(MessageType.Error, GetType().Name+".Generate().Task{}", "Error getting NextBlock", error);
                    }
                  });
              }
            }


            return result;
        }//lock
      }


      /// <summary>
      /// Tries to generate the specified number of Globally-Unique distributed IDs (GDID) for the supplied sequence name. This method is thread-safe.
      /// The method may generate less GUIDs than requested. All IDs come from the same authority
      /// </summary>
      /// <param name="scopeName">The name of scope where sequences are kept</param>
      /// <param name="sequenceName">The name of sequence within the scope for which ID to be obtained</param>
      /// <param name="gdidCount">The dsired number of consecutive GDIDs</param>
      /// <param name="vicinity">The location on ID counter scale, the authority may disregard this parameter</param>
      /// <param name="noLWM">
      ///  When true, does not start async fetch of the next ID block while the current block reaches low-water-mark.
      ///  This may not be desired in some short-lived processes.
      ///  The provider may disregard this flag
      /// </param>
      /// <returns>The GDID instance array which may be shorter than requested</returns>
      public GDID[] TryGenerateManyConsecutiveGDIDs(string scopeName, string sequenceName, int gdidCount, ulong? vicinity = GDID.COUNTER_MAX, bool noLWM = false)
      {
        if (scopeName==null || sequenceName==null)
          throw new GdidException(StringConsts.ARGUMENT_ERROR+GetType().Name+".TryGenerateManyConsecutiveGDIDs(scopeName|sequenceName=null)");

        if (gdidCount<=0)
          throw new GdidException(StringConsts.ARGUMENT_ERROR+GetType().Name+".TryGenerateManyConsecutiveGDIDs(gdidCount<=0)");

        if (m_ScopePrefix!=null) scopeName = m_ScopePrefix + scopeName;
        if (m_SequencePrefix!=null) sequenceName = m_SequencePrefix + sequenceName;

        scopeName = scopeName.Trim();
        sequenceName = sequenceName.Trim();

        GdidAuthorityService.CheckNameValidity(scopeName);
        GdidAuthorityService.CheckNameValidity(sequenceName);

        var scope = m_Scopes.GetOrRegister(scopeName, (snm) => new scope{Name = snm}, scopeName);

        var sequence = scope.Sequences.GetOrRegister(sequenceName, (_) => new sequence{Scope = scope, Name = sequenceName}, 0);//with block=NULL


        lock(sequence)
        {
            var block = sequence.Block;

            if (block==null || block.__Remaining<=(gdidCount / 2))//get whole block
            {
               block = allocateBlock(sequence, gdidCount, vicinity);
               block.__Remaining =  block.BlockSize;
            }

            var result = new GDID[ Math.Min(gdidCount, block.__Remaining) ];

            for(var i=0; i<result.Length; i++, block.__Remaining--)
            {
               var counter = block.StartCounterInclusive + (ulong)(block.BlockSize - block.__Remaining);
               result[i] = new GDID(block.Era, block.Authority, counter);
            }

            return result;
        }//lock
      }


    #endregion

    #region .pvt

         private const int MIN_BLOCK_SZ = 2;
         private const double NORM_IDS_PER_SEC = 16;//suppose we generate 16 ids every second, every thread 1 per sec, 16 threads
         private const double NORM_AUTH_CALL_EVERY_SEC = 5.0;//normal case, call authority every seconds
         private const double MIN_TIME_SLICE_SEC = 0.01d;//10ms

      private GdidBlock allocateBlock(sequence seq, int blockSize, ulong? vicinity = GDID.COUNTER_MAX)
      {
        m_BlockWasAllocated = true;

        if (blockSize<=0)//calculate the most appropriate
        {
          var now = DateTime.UtcNow;
          var sinceLastAllocationSec = (now - seq.LastAllocationUTC).TotalSeconds;
          if (sinceLastAllocationSec<MIN_TIME_SLICE_SEC) sinceLastAllocationSec = MIN_TIME_SLICE_SEC;
          seq.LastAllocationUTC = now;

          seq.avgSinceLastAlloc1 = seq.avgSinceLastAlloc2;
          seq.avgSinceLastAlloc2 = seq.avgSinceLastAlloc3;
          seq.avgSinceLastAlloc3 = sinceLastAllocationSec;

          var avg = (seq.avgSinceLastAlloc1 +
                     seq.avgSinceLastAlloc2 +
                     seq.avgSinceLastAlloc3) / 3d;

          if (avg==0) avg = MIN_TIME_SLICE_SEC;

          blockSize = MIN_BLOCK_SZ + (int)((NORM_AUTH_CALL_EVERY_SEC * NORM_IDS_PER_SEC) / avg);
        }
////Console.WriteLine("|--------------------> Block size is: "+blockSize);
        if (m_TestingAuthorityNode.IsNullOrWhiteSpace())
          return allocateBlockInSystem(seq.Scope.Name, seq.Name, blockSize, vicinity);
        else
          return allocateBlockInTesting(seq.Scope.Name, seq.Name, blockSize, vicinity);
      }


      private GdidBlock allocateBlockInTesting(string scopeName, string sequenceName, int blockSize, ulong? vicinity)
      {
        Instrumentation.AllocBlockRequestedEvent.Happened(scopeName, sequenceName);
        using(var cl = new Clients.GdidAuthority(m_TestingAuthorityNode))
        {
            try
            {
              var result =  cl.AllocateBlock(scopeName, sequenceName, blockSize, vicinity);
              Instrumentation.AllocBlockSuccessEvent.Happened(scopeName, sequenceName, m_TestingAuthorityNode);
              return result;
            }
            catch
            {
              Instrumentation.AllocBlockFailureEvent.Happened(scopeName, sequenceName, blockSize, m_TestingAuthorityNode);
              throw;
            }
        }
      }

      private GdidBlock allocateBlockInSystem(string scopeName, string sequenceName, int blockSize, ulong? vicinity)
      {
        Instrumentation.AllocBlockRequestedEvent.Happened(scopeName, sequenceName);

        GdidBlock result = null;
        var batch = Guid.NewGuid();
        var list = "";
        foreach(var node in this.AuthorityHosts.OrderBy(h => h.DistanceKm))//in the order of distances, closest first
        {
          list += (" Trying '{0}' at {1}km\n".Args(node.Name, node.DistanceKm));
          try
          {
            using(var cl = SkySystem.IsMetabase
                           ? ServiceClientHub.New<IGdidAuthorityClient>( node.Name )
                           : new Clients.GdidAuthority( node.Name )
                 )
              result = cl.AllocateBlock(scopeName, sequenceName, blockSize, vicinity);

            Instrumentation.AllocBlockSuccessEvent.Happened(scopeName, sequenceName, node.Name);
          }
          catch(Exception error)
          {
            log(MessageType.Error, GetType().Name+".allocateBlock()", "Error invoking GDIDAuthority.AllocateBlock('{0}')".Args(node), error, batch);

            Instrumentation.AllocBlockFailureEvent.Happened(scopeName, sequenceName, blockSize, node.Name);
          }

          if (result!=null) break;
        }

        if (result==null)
        {
          if (list.IsNullOrWhiteSpace()) list = "<none>";
          log(MessageType.Emergency, GetType().Name+".allocateBlock()", StringConsts.GDIDGEN_ALL_AUTHORITIES_FAILED_ERROR + list, batch: batch);
          Instrumentation.AllocBlockRequestFailureEvent.Happened(scopeName, sequenceName);
          throw new GdidException(StringConsts.GDIDGEN_ALL_AUTHORITIES_FAILED_ERROR + list);
        }

        return result;
      }

      private void log(MessageType type, string from, string msg, Exception error = null, Guid? batch = null)
      {
        var lm = new NFX.Log.Message{
               Type = type,
               Topic = SysConsts.LOG_TOPIC_ID_GEN,
                From = from,
                Text = msg,
                Exception = error
            };

        if (batch.HasValue)
          lm.RelatedTo = batch.Value;

        App.Log.Write( lm );
      }

    #endregion

  }

}
