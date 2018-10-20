
using System;

namespace Azos.Tools.Getdatetime
{

  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      const string deffmt = "yyyyMMddHHmmss";

      if ((args.Length > 0) && (args[0] == "/?"))
      {
        ConsoleColor cForeground = Console.ForegroundColor;
        ConsoleColor cBackground = Console.BackgroundColor;

        Console.ForegroundColor = ConsoleColor.White;
        Console.BackgroundColor = ConsoleColor.Black;
        Console.WriteLine();
        Console.WriteLine("Date/Time Utility");
        Console.WriteLine("Returns system data time in a specified format");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("Copyright (c) 2018 Azist Group");
        Console.WriteLine("Version 3.0 / Azos as of Oct 2018");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(" Usage:");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("    getdatetime [format] ([[{+/-},{part spec},{num}]/[{=,{part spec},{num}]]) ");
        Console.WriteLine(" ");
        Console.WriteLine("    [format] - standard .net date/time formatting pattern");
        Console.WriteLine("     \"" + deffmt + "\" will be used if no [format] param supplied");
        Console.WriteLine(" ");
        Console.WriteLine("    (modifier) - optional, allows to alter date time, may be repeated, evaluated from left to right");

        Console.WriteLine(" ");
        Console.WriteLine("    Shift Modifier:");
        Console.WriteLine("     [{+/-},{part spec},{num}] - allows to shift date time");
        Console.WriteLine("       {+/-}  add/subtract ");
        Console.WriteLine("       {part spec}  date/time part to change ");
        Console.WriteLine("       {num}  value of change (integer) ");
        Console.WriteLine(" ");

        Console.WriteLine("    Set Modifier:");
        Console.WriteLine("     [{=},{part spec},{num}] - allows to set date time part to a specific value");
        Console.WriteLine("       {=}  set ");
        Console.WriteLine("       {part spec}  date/time part to change ");
        Console.WriteLine("       {num}  new value (integer)");
        Console.WriteLine(" ");

        Console.WriteLine("    part spec:");
        Console.WriteLine("       {m/d/y/h/i/s} ");
        Console.WriteLine("           m  - month ");
        Console.WriteLine("           d  - day ");
        Console.WriteLine("           y  - year ");
        Console.WriteLine("           h  - hour ");
        Console.WriteLine("           i  - minute ");
        Console.WriteLine("           s  - second ");
        Console.WriteLine(" ");
        Console.WriteLine(" ");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine(" Example:");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(" ");
        Console.WriteLine("  getdatetime yyyyMMddHHmmss  -,d,15");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("   returns date and time 15 days ago from current system date time stamp");
        Console.WriteLine(" ");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  getdatetime yyyyMMddHHmmss  -,d,15 =,h,1 =,i,0 =,s,0");
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine("   returns date 15 days ago from current system date, time is set to 1:00 AM ");

        Console.ForegroundColor = cForeground;
        Console.BackgroundColor = cBackground;

        return;
      }//help


      string fmt = deffmt;
      DateTime dt = DateTime.Now;


      try
      {

        for (int i = 0; i < args.Length; i++)
        {
          string arg = args[i].Trim();
          if (arg.Length == 0) continue;// safeguard
          if (args[i].IndexOfAny(new char[] { '+', '-', '=' }) >= 0)
          {
            string[] cmd = args[i].Split(',');
            if (cmd.Length < 3) continue;//safeguard

            int val = int.Parse(cmd[2]);
            string part = cmd[1].Trim().ToLower();

            if (part.Length == 0) continue;//safeguard


            if (cmd[0] == "=") //set
            {
              switch (part)
              {
                case "m": dt = new DateTime(dt.Year, val, dt.Day, dt.Hour, dt.Minute, dt.Second); break;
                case "d": dt = new DateTime(dt.Year, dt.Month, val, dt.Hour, dt.Minute, dt.Second); break;
                case "y": dt = new DateTime(val, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second); break;
                case "h": dt = new DateTime(dt.Year, dt.Month, dt.Day, val, dt.Minute, dt.Second); break;
                case "i": dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, val, dt.Second); break;
                case "s": dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, val); break;
                default: throw new Exception("Invalid date time part spec");
              }//switch
            }//set

            if ((cmd[0] == "+") || (cmd[0] == "-")) //set
            {
              if (cmd[0] == "-") val *= -1;
              switch (part)
              {
                case "m": dt = dt.AddMonths(val); break;
                case "d": dt = dt.AddDays(val); break;
                case "y": dt = dt.AddYears(val); break;
                case "h": dt = dt.AddHours(val); break;
                case "i": dt = dt.AddMinutes(val); break;
                case "s": dt = dt.AddSeconds(val); break;
                default: throw new Exception("Invalid date time part spec");
              }//switch
            }//set


          }
          else fmt = args[i];
        }//for

      }//try
      catch (Exception err)
      {
        Console.WriteLine("Error: " + err.Message);
        System.Environment.ExitCode = -1;
        return;
      }//catch



      Console.WriteLine(dt.ToString(fmt));
      System.Environment.ExitCode = 0;
    }
  }


}
