using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;


using Azos;
using Azos.Data;
using Azos.Pile;


namespace WinFormsTestSky.Caching
{
  public partial class CacheForm : System.Windows.Forms.Form
  {
    public CacheForm()
    {
      InitializeComponent();
    }

    private LocalCache m_Cache;

    private void CacheForm_Load(object sender, EventArgs e)
    {
       try
       {
         m_Cache = new LocalCache();
         m_Cache.Pile = new DefaultPile(m_Cache);
         m_Cache.Configure(null);
       //DURABLE
       m_Cache.TableOptions.Register(new TableOptions("table1") { CollisionMode = CollisionMode.Durable });
       // m_Cache.DefaultTableOptions = new TableOptions("*") { CollisionMode = CollisionMode.Durable };
         m_Cache.Start();
       }
       catch(Exception error)
       {
         MessageBox.Show(error.ToMessageWithType());
       }

       tbAbsoluteExpiration.Text = DateTime.Now.AddMinutes(120).ToString();
    }

    private void CacheForm_FormClosed(object sender, FormClosedEventArgs e)
    {
       m_Cache.Dispose();
    }

    private void btnPut_Click(object sender, EventArgs e)
    {
      var cnt = tbCount.Text.AsInt();
      var keyStart = tbKeyStart.Text.AsInt();
      var tbl = m_Cache.GetOrCreateTable<GDID>(tbTable.Text);
      var par = tbParallel.Text.AsInt(1);
      var absExp = tbAbsoluteExpiration.Text.AsNullableDateTime();
      if (absExp.HasValue) absExp = absExp.Value.ToUniversalTime();

      var maxAgeSec = tbMaxAgeSec.Text.AsNullableInt(null);

      var binF = tbBinFrom.Text.AsInt();
      var binT = tbBinTo.Text.AsInt();

      if (tbMaxCapacity.Text.IsNotNullOrWhiteSpace())
       tbl.Options.MaximumCapacity = tbMaxCapacity.Text.AsInt();

      var sw = Stopwatch.StartNew();
      var utcNow = DateTime.UtcNow;

      Parallel.For(0, cnt,
        new ParallelOptions{ MaxDegreeOfParallelism = par },
        (i)=>
        {
          var key = new GDID(0, (ulong)(keyStart+i));


        var data = //new SomeDataParcel(key,
           new SomeData
           {
             ID = key,
             Text1 = "Some text one" + i.ToString(),
             Text2 = "Another line of text which is longer",
             Long1 = i,
             Long2 = i * 178,
             SD = DateTime.UtcNow,
             Bin = (binF >= 0 && binT >= 0) ? new byte[App.Random.NextScaledRandomInteger(binF, binT)] : null
           };//,
         //  new SomeReplicationVersionInfo(utcNow)
         // );//"My data object #"+key.ToString()

          tbl.Put(key, data, maxAgeSec: maxAgeSec, absoluteExpirationUTC: absExp);
        }
      );


      tbKeyStart.Text = (keyStart+cnt+1).ToString();

      var ems = sw.ElapsedMilliseconds;
      Text = "Put {0:n0} in {1:n0} ms at {2:n2}/sec".Args(cnt, ems, cnt / (ems / 1000d));
    }

    private void btnGet_Click(object sender, EventArgs e)
    {
      var cnt = tbCount.Text.AsInt();
      var keyStart = tbKeyStart.Text.AsInt();
      var tbl = m_Cache.GetOrCreateTable<GDID>(tbTable.Text);
      var par = tbParallel.Text.AsInt(1);

      var sw = Stopwatch.StartNew();

      int found = 0;
      int notfound = 0;

      Parallel.For(0, cnt,
        new ParallelOptions{ MaxDegreeOfParallelism = par },
        (i)=>
        {
          var key = new GDID(0, (ulong)(keyStart+i));
          var data = tbl.Get(key);
          if (data==null)
           Interlocked.Increment(ref notfound);
          else
           Interlocked.Increment(ref found);
        }
      );

      var ems = sw.ElapsedMilliseconds;
      Text = "Did {0:n0} in {1:n0} ms at {2:n2}/sec; Hits: {3:n0}; Misses: {4:n0}".Args(cnt, ems, cnt / (ems / 1000d), found, notfound);
    }

    private void btnRemove_Click(object sender, EventArgs e)
    {
      var cnt = tbCount.Text.AsInt();
      var keyStart = tbKeyStart.Text.AsInt();
      var tbl = m_Cache.GetOrCreateTable<GDID>(tbTable.Text);
      var par = tbParallel.Text.AsInt(1);

      var sw = Stopwatch.StartNew();

      int found = 0;
      int notfound = 0;

      Parallel.For(0, cnt,
        new ParallelOptions{ MaxDegreeOfParallelism = par },
        (i)=>
        {
          var key = new GDID(0, (ulong)(keyStart+i));
          var removed = tbl.Remove(key);
          if (removed)
           Interlocked.Increment(ref found);
          else
           Interlocked.Increment(ref notfound);
        }
      );

      var ems = sw.ElapsedMilliseconds;
      Text = "Did {0:n0} in {1:n0} ms at {2:n2}/sec; Removed: {3:n0}; Misses: {4:n0}".Args(cnt, ems, cnt / (ems / 1000d), found, notfound);
    }

    private void btnGC_Click(object sender, EventArgs e)
    {
      var was = GC.GetTotalMemory(false);
      var w = Stopwatch.StartNew();
      GC.Collect();
      Text = "GC Freed {0:n0} bytes in {1:n0} ms".Args(was - GC.GetTotalMemory(false), w.ElapsedMilliseconds);
    }

    private void tmrStatus_Tick(object sender, EventArgs e)
    {
      if (chkAutoGet.Checked) autoGet();
      if (chkAutoPut.Checked) autoPut();
      if (chkAutoRemove.Checked) autoRemove();
      lblAutoStatus.Text = "Put: {0}  Get Hit: {1} Get Miss: {2}  Remove Hit: {3} Remove Miss: {4}".
         Args(
            stat_Put,
            stat_GetHit,
            stat_GetMiss,
            stat_RemoveHit,
            stat_RemoveMiss
         );


      stat_Put = 0;
      stat_GetHit = 0;
      stat_GetMiss = 0;
      stat_RemoveHit = 0;
      stat_RemoveMiss = 0;
    }

       private int m_Key;

       private int stat_Put;
       private int stat_GetHit;
       private int stat_GetMiss;
       private int stat_RemoveHit;
       private int stat_RemoveMiss;



       private void autoGet()
       {
         var cnt = tbAutoGet.Text.AsInt();
         var key = new GDID(0, (ulong)(m_Key - cnt));
         var tbl = m_Cache.GetOrCreateTable<GDID>(chkAutoRandTbl.Checked ? "T-"+App.Random.NextScaledRandomInteger(0,99).ToString() : tbTable.Text);
         for(var i=0; i<cnt; i++)
         {
           var obj = tbl.Get(key);

           if (obj==null) stat_GetMiss++; else stat_GetHit++;

           if (chkAutoRandKey.Checked)
            key = new GDID(0, (ulong)App.Random.NextScaledRandomInteger(0, m_Key));
           else
            key = new GDID(0, key.ID+1);
         }
       }

       private void autoPut()
       {
         var cnt = tbAutoPut.Text.AsInt();
         var key = new GDID(0, (ulong)m_Key);
         var tbl = m_Cache.GetOrCreateTable<GDID>(chkAutoRandTbl.Checked ? "T-"+App.Random.NextScaledRandomInteger(0,99).ToString() : tbTable.Text);

         var binF = tbBinFrom.Text.AsInt();
         var binT = tbBinTo.Text.AsInt();

         var utcNow = DateTime.UtcNow;

         for(var i=0; i<cnt; i++)
         {
        /*
           var data = new SomeDataParcel(key,
           new SomeData
           {
             ID = key,
             Text1 = "Some text one"+i.ToString(),
             Text2 = "Another line of text which is longer",
             Long1 = i,
             Long2 = i * 178,
             SD = DateTime.UtcNow,
             Bin = (binF>=0 && binT>=0) ? new byte[App.Random.NextScaledRandomInteger(binF, binT)] : null
           },
           new SomeReplicationVersionInfo(utcNow)
          );//"My data object #"+key.ToString()

          tbl.Put(key, data);
*/

           stat_Put++;

           if (chkAutoRandKey.Checked)
            key = new GDID(0, (ulong)App.Random.NextScaledRandomInteger(0, m_Key));
           else
             key = new GDID(0, key.ID+1);

           m_Key++;
         }

       }

       private void autoRemove()
       {
         var cnt = tbAutoRemove.Text.AsInt();
         var key = new GDID(0, (ulong)(m_Key - cnt));
         var tbl = m_Cache.GetOrCreateTable<GDID>(chkAutoRandTbl.Checked ? "T-"+App.Random.NextScaledRandomInteger(0,99).ToString() : tbTable.Text);
         for(var i=0; i<cnt; i++)
         {
           var rem = tbl.Remove(key);

           if (rem) stat_RemoveHit++; else stat_RemoveMiss++;

           if (chkAutoRandKey.Checked)
            key = new GDID(0, (ulong)App.Random.NextScaledRandomInteger(0, m_Key));
           else
            key = new GDID(0, key.ID+1);
         }
       }

    /*
      [DataParcel("TestSchema", "TestArea")]
      public class SomeDataParcel : Parcel<SomeData>
      {

        public SomeDataParcel(GDID id, SomeData payload) : base(id, payload)
        {

        }

        public SomeDataParcel(GDID id, SomeData payload, IReplicationVersionInfo ver = null) : base(id, payload, ver)
        {

        }

        protected override void DoValidate(IBank bank)
        {

        }

        public override bool ReadOnly {  get { return false; } }
      }

    [Serializable]
    public struct SomeReplicationVersionInfo : IReplicationVersionInfo, IComparable<SomeReplicationVersionInfo>, IEquatable<SomeReplicationVersionInfo>
    {
      public SomeReplicationVersionInfo(DateTime stamp)
      {
        m_Stamp = stamp;
      }

      private DateTime m_Stamp;

      public Boolean VersionDeleted { get { return false; } }

      public DateTime VersionUTCTimestamp { get { return m_Stamp; } }

      public override int GetHashCode()
      {
        return (VersionDeleted ? (int)0x55555555 : 0) ^ VersionUTCTimestamp.GetHashCode();
      }

      public int CompareTo(object obj)
      {
        if (obj==null || !(obj is SomeReplicationVersionInfo)) return 1;
        var other = (SomeReplicationVersionInfo)obj;

        return this.CompareTo(other);
      }

      public int CompareTo(SomeReplicationVersionInfo other)
      {
        if (this.VersionDeleted==other.VersionDeleted)
         return this.VersionUTCTimestamp.CompareTo(other.VersionUTCTimestamp);

        return this.VersionDeleted ? 1 : -1; //DELETED = always LATER regardless time
      }

      public override bool Equals(object obj)
      {
        if (obj==null || !(obj is SomeReplicationVersionInfo)) return false;

        var other = (SomeReplicationVersionInfo)obj;

        return this.Equals(other);
      }

      public bool Equals(SomeReplicationVersionInfo other)
      {
        return this.VersionDeleted==other.VersionDeleted &&
               this.VersionUTCTimestamp.Equals(other.VersionUTCTimestamp);
      }
    }
*/

    public class SomeData
       {
         public GDID ID;
         public string Text1;
         public string Text2;
         public bool Flag;
         public long Long1;
         public long Long2;
         public long Long3;
         public DateTime SD;
         public DateTime? ED;
         public decimal Rate;
         public byte[] Bin;
       }


  }
}
