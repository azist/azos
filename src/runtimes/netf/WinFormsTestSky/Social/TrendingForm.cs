using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;

using Azos;
using Azos.Data;

using Azos.Sky.Social;
using Azos.Sky.Social.Trending.Server;
using Azos.Sky.Social.Trending;
using Azos.Serialization.JSON;

namespace WinFormsTestSky.Social
{
  public partial class TrendingForm : System.Windows.Forms.Form
  {
    public TrendingForm()
    {
      InitializeComponent();
    }

    #region Fields

    private TrendingSystemService m_Server;
    private TrendingSystemClient m_Client;
    private bool m_SendGauge = false;

    #endregion


    private void btnServerStart_Click(object sender, EventArgs e)
    {
      m_Server = new TrendingSystemService(null);

      var cfg= @"
service
{
  name=TrendingSvc
  type='Azos.Sky.Social.Trending.Server.TrendingSystemService, Azos.Sky.Social'

  trending-host
  {
    type='WinFormsTestSky.Social.TestTrendingHost, WinFormsTestSky'
  }

  volume
  {
    name=Fractional
    type='Azos.Sky.MongoDB.Social.Trending.MongoDBVolume, Azos.Sky.MongoDB.Social'
    mongo='mongo{server=""localhost:27017"" db=Trending_F}'

    detalization-level=Fractional
  }
  volume
  {
    name=Hourly
    type='Azos.Sky.MongoDB.Social.Trending.MongoDBVolume, Azos.Sky.MongoDB.Social'
    mongo='mongo{server=""localhost:27017"" db=Trending_H}'

    detalization-level=Hourly
  }
}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      m_Server.Configure(cfg);
      m_Server.Start();

      m_Client = new TrendingSystemClient("async://localhost:{0}".Args(App.ConfigRoot.Navigate("/gv/services/$async-trending").Value));
    }

    private void btnServerStop_Click(object sender, EventArgs e)
    {
      DisposableObject.DisposeAndNull(ref m_Server);
    }

    private void btnCheckTrendings_Click(object sender, EventArgs e)
    {
      readTrending("","");
    }

    private void tmr_Tick(object sender, EventArgs e)
    {
      if(m_SendGauge)
      {
        var gShard = new GDID();
        var gUser = new GDID();

        var bUser = cbUserEntity.Checked;
        var bProduct = cbProductEntity.Checked;

        if(bUser)
        {
          var age = tbAge.Text;
          var sex = tbSex.Text;
          var country = tbCountry.Text;
          var userDim = "{age:'"+age+"', sex:'"+sex+ "', country:'" + country+"'}";
          var userValue = tbUserValue.Text.AsLong();
          SocialTrendingGauge.Emit("user", gShard, gUser, userValue, userDim);
        }

        if(bProduct)
        {
          var product = "{size:'"+ tbSize.Text + "', color:'"+ tbColor.Text + "'}";
          var productValue = tbProductValue.Text.AsLong();
          SocialTrendingGauge.Emit("product", gShard, gUser, productValue, product);
        }
      }
    }

    private void bStartSend_Click(object sender, EventArgs e)
    {
        if(m_SendGauge)
        {
          m_SendGauge = false;
          bStartSend.Text = "START SEND";
        }
        else
        {
          m_SendGauge = true;
          bStartSend.Text = "STOP SEND";
        }
    }

    private void tmrSend_Tick(object sender, EventArgs e)
    {
      if(m_Server != null)
      {
        var gauges = App.Instrumentation.GetBufferedResultsSince(App.TimeSource.UTCNow.AddSeconds(-10))
        .Where(d => d.GetType() == typeof(SocialTrendingGauge))
        .Cast<SocialTrendingGauge>()
        .ToArray();
        if(gauges.Length > 0)
          m_Client.Send(gauges);
      }
    }

    private void tmrMain_Tick(object sender, EventArgs e)
    {
      bool isServerStart = m_Server != null;
      btnServerStart.Enabled = !isServerStart;
      btnServerStop.Enabled = isServerStart;
      bStartSend.Enabled = isServerStart;
      btnRead.Enabled = isServerStart;
    }

    private void btnFilter_Click(object sender, EventArgs e)
    {
      var age = tbfAge.Text;
      var sex = tbfSex.Text;
      var country = tbfCountry.Text;

      var userFilter = new Dictionary<String, String>();
      if(age.IsNotNullOrEmpty()) userFilter.Add("age", age);
      if(sex.IsNotNullOrEmpty()) userFilter.Add("sex", sex);
      if(country.IsNotNullOrEmpty()) userFilter.Add("country", country);

      var size = tbfSize.Text;
      var color = tbfColor.Text;
      var productFilter = new Dictionary<String, String>();
      if(size.IsNotNullOrEmpty()) productFilter.Add("size", size);
      if(color.IsNotNullOrEmpty()) productFilter.Add("color", color);


      readTrending(userFilter.ToJSON(),"");

    }

    private void readTrending(string userFilter, string productFilter)
    {
      var sd_ = dtpSB.Value.ToUniversalTime();
      var ed_ = dtpED.Value.ToUniversalTime();

      var sd = new DateTime(sd_.Year, sd_.Month, sd_.Day, 0, 0, 0);
      var ed = new DateTime(ed_.Year, ed_.Month, ed_.Day, 23, 59, 59);

      var qryUser = new TrendingQuery("user", sd_, ed_, 70*70, 0, nCol.Value.AsInt() , userFilter);
      var qryProduct = new TrendingQuery("product", sd_, ed_, 70*70, 0, nCol.Value.AsInt(), productFilter);

      var resultUser = m_Client.GetTrending(qryUser);
      var resultProduct = m_Client.GetTrending(qryProduct);
      MessageBox.Show(resultUser.ToJSON(JSONWritingOptions.PrettyPrint) +"\n"+ resultProduct.ToJSON(JSONWritingOptions.PrettyPrint));
    }
  }
}
