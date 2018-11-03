using System;
using System.Collections.Generic;
using System.Linq;

using Azos.Graphics;
using Azos.Wave.Mvc;
using Azos.Log;
using Azos.Instrumentation;
using Azos.Platform.Instrumentation;
using Azos.Serialization.JSON;

using Azos.Sky.Apps;
using Azos.Sky.Apps.ZoneGovernor;

namespace Azos.Sky.WebManager.Controllers
{

  public sealed class PublicAPI : WebManagerController
  {
    private const int MAX_COUNT = 1024;

    [Action]
    public object HostPerformance(int duration = 3000, int sample = 1000, DateTime? lastErrorSample = null)
    {
      if (duration < 1000) duration = 1000;
      if (duration > 10000) duration = 10000;

      if (sample < 250) sample = 250;
      if (sample > 2000) sample = 2000;


      var watch = System.Diagnostics.Stopwatch.StartNew();

      var load = new List<JSONDataMap>();

      var first = true;
      while (watch.ElapsedMilliseconds < duration)
      {
        var cpu = Platform.Computer.CurrentProcessorUsagePct;
        var ram = Platform.Computer.GetMemoryStatus().LoadPct;

        System.Threading.Thread.Sleep(sample);//todo:  Redo this


        var datum = new JSONDataMap{
                      {"at", App.TimeSource.UTCNow},
                      {"cpu",cpu},
                      {"ram",ram} };

        if (first)
        {
          addError(datum, "warning", App.Log.LastWarning, lastErrorSample);
          addError(datum, "error", App.Log.LastError, lastErrorSample);
          addError(datum, "catastrophe", App.Log.LastCatastrophe, lastErrorSample);
        }

        load.Add(datum);
        first = false;
      }


      return new
      {
        ProcessAllocated = GC.GetTotalMemory(false),
        Load = load
      };
    }

    [Action]
    public object PerformanceImg(int width = 64, int height = 64, int lookBackSec = 300)
    {
      const int MIN_LOOKBACK_SEC = 10, MAX_LOOKBACK_SEC = 600;

      if (lookBackSec < MIN_LOOKBACK_SEC) lookBackSec = MIN_LOOKBACK_SEC;
      else if (lookBackSec > MAX_LOOKBACK_SEC) lookBackSec = MAX_LOOKBACK_SEC;

      var utcNow = App.TimeSource.UTCNow.AddSeconds(-lookBackSec);
      IInstrumentation myInstrumentation = App.Instrumentation;

      IInstrumentation zoneInstrumentation = null;
      if (SkySystem.SystemApplicationType == SystemApplicationType.ZoneGovernor && ZoneGovernorService.IsZoneGovernor)
        zoneInstrumentation = ZoneGovernorService.Instance.SubordinateInstrumentation;

      Func<IInstrumentation, Datum[]> filter = (instr) => instr.GetBufferedResultsSince(utcNow)
                              .Where(d => d.GetType() == typeof(CPUUsage) || d.GetType() == typeof(RAMUsage))
                              .Take(MAX_COUNT)
                              .ToArray();

      Datum[] myData = filter(myInstrumentation);

      Datum[] zoneData = null;
      if (zoneInstrumentation != null) zoneData = filter(zoneInstrumentation);
      //Datum[] zoneData = myData; // TESTING!!!!!

      var img = renderPerformanceImg(width, height, myData, zoneData);

      return new Picture(img, JpegImageFormat.Standard);
    }

    private Image renderPerformanceImg(int width, int height, Datum[] myData, Datum[] zoneData)
    {
      return Image.Of(width, height);

      #warning ETO nado peredelat na Canvas vmesto Graphics ispolzuja Azos.Graphics namespace

      ////////var cpuData = myData.Where(d => d.GetType() == typeof(CPUUsage)).Cast<CPUUsage>().ToArray();
      ////////var ramData = myData.Where(d => d.GetType() == typeof(RAMUsage)).Cast<RAMUsage>().ToArray();

      ////////const int MIN_WIDTH = 16, MIN_HEIGHT = 16;
      ////////const int MAX_WIDTH = 1024, MAX_HEIGHT = 1024;

      ////////if (width < MIN_WIDTH) width = MIN_WIDTH;
      ////////else if (width > MAX_WIDTH) width = MAX_WIDTH;

      ////////if (height < MIN_HEIGHT) height = MIN_HEIGHT;
      ////////else if (height > MAX_HEIGHT) height = MAX_HEIGHT;

      ////////var dataWidth = cpuData.Length > ramData.Length ? cpuData.Length : ramData.Length;
      ////////if (dataWidth < 10) dataWidth = 10;

      ////////var imgResult = new Bitmap(width, height);
      ////////using(var imgDraw = new Bitmap(zoneData == null ? dataWidth : 2 * dataWidth, height))
      ////////{
      ////////  using (var gr = Graphics.FromImage(imgDraw))
      ////////  {
      ////////    var backColor = Color.FromArgb(40, 40, 40);
      ////////    gr.Clear(backColor);

      ////////    drawSet(gr, 0, dataWidth, height, cpuData, ramData);
      ////////    if (zoneData != null)
      ////////    {
      ////////      var zoneCPUData = zoneData.Where(d => d.GetType() == typeof(CPUUsage)).Cast<CPUUsage>().ToArray();
      ////////      var zoneRAMData = zoneData.Where(d => d.GetType() == typeof(RAMUsage)).Cast<RAMUsage>().ToArray();

      ////////      drawSet(gr, dataWidth, dataWidth, height, zoneCPUData, zoneRAMData);
      ////////    }
      ////////  }//using Graphics

      ////////  using (var gr = Graphics.FromImage(imgResult))
      ////////  {
      ////////    //scale the picture
      ////////    gr.DrawImage(imgDraw, new Rectangle(0, 0, width, height));
      ////////  }
      //////// }//using imgDraw
      ////////return imgResult;
    }

    #warning ETO nado peredelat na Canvas vmesto Graphics ispolzuja Azos.Graphics namespace
    ////////private void drawSet(Graphics gr, float startX, int dataWidth, int height, CPUUsage[] cpuData, RAMUsage[] ramData)
    ////////{
    ////////  //======= CPU
    ////////  using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, dataWidth, height),
    ////////                                                                    Color.FromArgb(0xFF, 0x40, 0x00),
    ////////                                                                    Color.FromArgb(0x20, 0xFF, 0x00),
    ////////                                                                    90f))
    ////////  {
    ////////    using (var lp = new Pen(br, 1f))
    ////////    {
    ////////      float x = startX;
    ////////      for (int i = 0; i < cpuData.Length - 1; i++)
    ////////      {
    ////////        var datum = cpuData[i];
    ////////        gr.DrawLine(lp, x, height, x, (int)(height - (height * (datum.Value / 100f))));

    ////////        x += 1f;
    ////////      }
    ////////    }
    ////////  }

    ////////  //========= RAM
    ////////  using (var br = new System.Drawing.Drawing2D.LinearGradientBrush(new Rectangle(0, 0, dataWidth, height),
    ////////                                                                    Color.FromArgb(0xA0, 0xff, 0xc0, 0xff),
    ////////                                                                    Color.FromArgb(0x50, 0xc0, 0xc0, 0xff),
    ////////                                                                    90f))
    ////////  {
    ////////    using (var lp = new Pen(br, 1f))
    ////////    {
    ////////      float x = startX;
    ////////      for (int i = 0; i < ramData.Length - 1; i++)
    ////////      {
    ////////        var datum = ramData[i];
    ////////        gr.DrawLine(lp, x, height, x, (int)(height - (height * (datum.Value / 100f))));

    ////////        x += 1f;
    ////////      }
    ////////    }
    ////////  }
    ////////}

    private void addError(JSONDataMap datum, string type, Message msg, DateTime? lastErrorSample)
    {
        if (msg==null) return;
        if (!lastErrorSample.HasValue || (msg.TimeStamp-lastErrorSample.Value).TotalSeconds>1)
        {
            datum[type] = msg.TimeStamp.ToString("dd HH:mm:ss");
            datum["LastErrorSample"] = msg.TimeStamp;
        }
    }
  }
}
