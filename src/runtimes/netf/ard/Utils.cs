using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.AccessControl;

namespace ard
{
  public static class Utils
  {
      public const string AHGOV_EXE_NAME = "ahgov.exe";

      public const string ARD_PARENT_CMD_PARAM = "ARDPARENT";
      public const string ARD_UPDATE_PROBLEM_CMD_PARAM = "UPDATEPROBLEM";

      public const string ARD_DIR = "ard";
      public const string RUN_DIR = "run-netf";
      public const string UPDATE_DIR = "upd";
      public const string UPDATE_FINISHED_FILE = "update.finished";
      public const string UPDATE_FINISHED_FILE_OK_CONTENT = "OK.";




      public static string GetRootPath()
      {
         var exeName = System.Reflection.Assembly.GetEntryAssembly().Location;
         return Path.GetDirectoryName(exeName);
      }

      public static string GetAHGOVExecutablePath()
      {
         var result = Path.Combine(GetRunDir(), AHGOV_EXE_NAME);
         if (!File.Exists(result))throw new Exception("The AHGOV bootstrap executable is not installed: " + result);
         return result;
      }

      public static string GetRunDir()
      {
         var bootPath = GetRootPath();
         return Path.Combine(bootPath, RUN_DIR);
      }

      public static string GetUpdateDir()
      {
        var root = Utils.GetRootPath();
        var result =  Path.Combine(root, UPDATE_DIR);

        //Directory may not be present, in which case there is nothing to update
        if (!Directory.Exists(result)) return null;
        return result;
      }

      public static bool UpdatePathValid(string path)
      {
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (!Directory.Exists(path)) return false;
        var fn = Path.Combine(path, UPDATE_FINISHED_FILE);
        if (!File.Exists(fn)) return false;

        if (File.ReadAllText(fn)!=UPDATE_FINISHED_FILE_OK_CONTENT) return false;
        return true;
      }

      public static void ReplaceRUN_With_UPDATE(string updateDir)
      {
        var runDir = GetRunDir();
        try
        {
          //try to lock all files to make sure that the operation can proceed
          foreach(var fn in Directory.GetFiles(runDir, "*.*", SearchOption.AllDirectories))
            try
            {
              using (var fs = new FileStream(fn, FileMode.Open, FileAccess.ReadWrite, FileShare.None, 0xff))
              {
               //do nothing
              }
            }
            catch(Exception error)
            {
              throw new Exception(string.Format("The file '{0}' in RUN dir is locked. UPD->RUN swap can not proceed. Exception: [{1}]{2}", fn, error.GetType().FullName, error.Message));
            }
          EnsureDirectoryDeleted(runDir);
        }
        catch(Exception error)
        {
          throw new Exception(string.Format("The deletion of RUN dir '{0}' failed: [{1}] {2}", runDir, error.GetType().FullName, error.Message));
        }

        try
        {
          Directory.Move(updateDir, runDir);
        }
        catch(Exception error)
        {
          throw new Exception(string.Format("The move of UPDATE DIR '{0}' -> RUN DIR '{1}' failed: [{2}] {3}", updateDir, runDir, error.GetType().FullName, error.Message));
        }

        try
        {
          MakeDirectoryAccesible(runDir);
        }
        catch(Exception error)
        {
          throw new Exception(string.Format("Error making RUN DIR '{0}' accessible: [{1}] {2}", runDir, error.GetType().FullName, error.Message));
        }
      }

    /// <summary>
    /// Creates directory and immediately grants it accessibility rules for everyone
    /// </summary>
    public static void MakeDirectoryAccesible(string path)
    {
      FileSystemAccessRule ausersRule = new FileSystemAccessRule(
                  "Authenticated Users",
                  FileSystemRights.FullControl,
                  InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                  PropagationFlags.None,
                  AccessControlType.Allow);

      FileSystemAccessRule usersRule = new FileSystemAccessRule(
                  "Users",
                  FileSystemRights.FullControl,
                  InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                  PropagationFlags.None,
                  AccessControlType.Allow);

      FileSystemAccessRule adminsRule = new FileSystemAccessRule(
                  "Administrators",
                  FileSystemRights.FullControl,
                  InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                  PropagationFlags.None,
                  AccessControlType.Allow);

      FileSystemAccessRule sysRule = new FileSystemAccessRule(
                 "SYSTEM",
                 FileSystemRights.FullControl,
                 InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                 PropagationFlags.None,
                 AccessControlType.Allow);


      DirectorySecurity dirSec = new DirectorySecurity();
      dirSec.AddAccessRule(ausersRule);
      dirSec.AddAccessRule(usersRule);
      dirSec.AddAccessRule(adminsRule);
      dirSec.AddAccessRule(sysRule);

      Directory.SetAccessControl(path, dirSec);
    }

        public static bool EnsureDirectoryDeleted(string dirName)
        {
          const int TIMEOUT_MS = 10000;
          if (string.IsNullOrWhiteSpace(dirName)) return false;

          if (!Directory.Exists(dirName)) return true;

          Directory.Delete(dirName, true);//MARKS directory for deletion, but does not delete it

          var sw = System.Diagnostics.Stopwatch.StartNew();
          while(sw.ElapsedMilliseconds < TIMEOUT_MS)
          {
             if (!Directory.Exists(dirName)) return true;//actual check for physical presence on disk
             System.Threading.Thread.Sleep(500);
          }
          return false;
        }


  }
}
