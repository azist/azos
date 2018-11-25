using System;
using System.Collections.Generic;
using System.Text;

namespace Azos.Web
{
  /// <summary>
  /// Globally uniquely identifies social network archetypes
  /// </summary>
  public enum SocialNetID
  {
    UNS = 0, // Unspecified

    TWT = 1, // Twitter
    FBK = 2, // Facebook
    GPS = 3, // Google+
    LIN = 4, // LinkedIn
    IGM = 5, // Instagram
    PIN = 6, // Pinterest
    TUM = 7, // Tumblr
    BAI = 8, // Baidu
    RED = 9, // Reddit
    FSQ = 10, // Foursquare
    BAD = 11, // Badoo
    SUP = 12, // Stumble Upon
    MYS = 13, // My Space
    FLK = 14, // Flickr
    MEE = 15, // Meetup
    QUO = 16, // Quora


    VKT = 100, // VKontakte
    ODN = 101, // Odnoklassniki
    YTB = 102, // YouTube
    VIN = 103, // Vine
    CLS = 104, // Classmates
    LJR = 105, // LiveJournal


    SKY = 1000, //SKYPE
    VIB = 1001, //Viber
    WCH = 1002, //WeChat
    WAP = 1003, //WhatsApp
    QZN = 1004, //QZone
    TLG = 1005, //Telegram
    SCH = 1007, //Snapchat

    OTH = 1000000 // Other
  }
}
