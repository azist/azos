/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Drawing;
using Azos.Scripting;

using Azos.Geometry;

namespace Azos.Tests.Nub.Geometry
{
  [Runnable]
  public class CommonTests
  {
    [Run]
    public void MapDirection()
    {
      var dir1 = Azos.Geometry.MapDirection.East;
      var dir2 = Azos.Geometry.MapDirection.North;
      var dir3 = Azos.Geometry.MapDirection.East;
      Aver.AreEqual("East", dir1.ToString());
      Aver.AreEqual("North", dir2.ToString());

      Aver.AreObjectsNotEqual(dir1, dir2);
      Aver.AreObjectsNotEqual(dir1, dir2);
      Aver.AreObjectsEqual(dir1, dir3);
    }


    [Run]
    public void Distance()
    {
      Aver.AreEqual(100, CartesianUtils.Distance(0, 0, 100, 0));
      Aver.AreEqual(100, CartesianUtils.Distance(0, 0, 0, 100));
      Aver.AreEqual(141421D, Math.Floor(1000F * CartesianUtils.Distance(0F, 0F, 100F, 100F)));
      Aver.AreEqual(141421D, Math.Floor(1000F * CartesianUtils.Distance(100F, 0F, 0F, 100F)));
      Aver.AreEqual(141421D, Math.Floor(1000F * CartesianUtils.Distance(100F, 100F, 0F, 0F)));
    }

    [Run]
    public void DistancePoints()
    {
      Aver.AreEqual(100, CartesianUtils.Distance(new Point(0, 0), new Point(100, 0)));
      Aver.AreEqual(100, CartesianUtils.Distance(new Point(0, 0), new Point(0, 100)));
      Aver.AreEqual(141421D, Math.Floor(1000F * CartesianUtils.Distance(new PointF(0, 0), new PointF(100, 100))));
      Aver.AreEqual(141421D, Math.Floor(1000F * CartesianUtils.Distance(new PointF(100, 0), new PointF(0, 100))));
      Aver.AreEqual(141421D, Math.Floor(1000F * CartesianUtils.Distance(new PointF(100, 100), new PointF(0, 0))));
    }

    [Run]
    public void RadToDeg()
    {
      Aver.AreEqual(180D, Math.PI.ToDeg());
      Aver.AreEqual(360D, (Math.PI * 2D).ToDeg());
    }

    [Run]
    public void DegToRad()
    {
      Aver.AreEqual(157, Math.Floor(90D.ToRad() * 100));
      Aver.AreEqual(314, Math.Floor(180D.ToRad() * 100));
    }

    [Run]
    public void AzimuthRad()
    {
      Aver.AreEqual(157D, Math.Floor(CartesianUtils.AzimuthRad(0, 0, 0, 100) * 100));
      Aver.AreEqual(0D, Math.Floor(CartesianUtils.AzimuthRad(0, 0, 100, 0) * 100));
    }

    [Run]
    public void AzimuthDeg()
    {
      Aver.AreEqual(90D, Math.Floor(CartesianUtils.AzimuthDeg(0, 0, 0, 100)));
      Aver.AreEqual(0D, Math.Floor(CartesianUtils.AzimuthDeg(0, 0, 100, 0)));
    }

    [Run]
    public void AzimuthOfRadix()
    {
      Aver.AreEqual(3, CartesianUtils.AzimuthOfRadix(0, 0,   0, -100, 4));
      Aver.AreEqual(0, CartesianUtils.AzimuthOfRadix(0, 0, 100, -100, 4));
      Aver.AreEqual(1, CartesianUtils.AzimuthOfRadix(0, 0, 100, 100, 4));
      Aver.AreEqual(2, CartesianUtils.AzimuthOfRadix(0, 0, -100, 100, 4));
    }

    [Run]
    public void WrapAngle()
    {
      Aver.AreEqual(314D, Math.Floor(CartesianUtils.WrapAngle(0D, Math.PI * 3D) * 100D));
      Aver.AreEqual(314D, Math.Floor(CartesianUtils.WrapAngle(Math.PI, Math.PI * 2D) * 100D));
      Aver.AreEqual(157D, Math.Floor(CartesianUtils.WrapAngle(Math.PI, -Math.PI / 2D) * 100D));
      Aver.AreEqual(314D + 157D, Math.Floor(CartesianUtils.WrapAngle(Math.PI, Math.PI / 2D) * 100D));
    }

    [Run]
    public void MapDirectionToAngle()
    {
      Aver.AreEqual(157D, Math.Floor(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.North) * 100D));
      Aver.AreEqual(314D + 157D, Math.Floor(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.South) * 100D));
      Aver.AreEqual(0D, Math.Floor(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.East) * 100D));
      Aver.AreEqual(314D, Math.Floor(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.West) * 100D));

      Aver.AreEqual(157D - 79D, Math.Floor(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.NorthEast) * 100D));
      Aver.AreEqual(157D + 78D, Math.Floor(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.NorthWest) * 100D));

      Aver.AreEqual(314D + 157D + 78D, Math.Floor(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.SouthEast) * 100D));
      Aver.AreEqual(314D + 157D - 79D, Math.Floor(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.SouthWest) * 100D));
    }

    [Run]
    public void AngleToMapDirection()
    {
      Aver.IsTrue(Azos.Geometry.MapDirection.North ==
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.North)));
      Aver.IsTrue(Azos.Geometry.MapDirection.South ==
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.South)));
      Aver.IsTrue(Azos.Geometry.MapDirection.East ==
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.East)));
      Aver.IsTrue(Azos.Geometry.MapDirection.West ==
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.West)));

      Aver.IsTrue(Azos.Geometry.MapDirection.NorthEast ==
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.NorthEast)));
      Aver.IsTrue(Azos.Geometry.MapDirection.NorthWest ==
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.NorthWest)));

      Aver.IsTrue(Azos.Geometry.MapDirection.SouthEast ==
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.SouthEast)));
      Aver.IsTrue(Azos.Geometry.MapDirection.SouthWest ==
        CartesianUtils.AngleToMapDirection(CartesianUtils.MapDirectionToAngle(Azos.Geometry.MapDirection.SouthWest)));
    }

    [Run]
    public void PerimeterViolationArea()
    {
      Aver.AreEqual(0, CartesianUtils.CalculatePerimeterViolationArea(new Rectangle(0, 0, 100, 100), new Rectangle(0, 0, 100, 100)));
      Aver.AreEqual(100, CartesianUtils.CalculatePerimeterViolationArea(new Rectangle(0, 0, 100, 100), new Rectangle(1, 0, 100, 100)));
      Aver.AreEqual(100, CartesianUtils.CalculatePerimeterViolationArea(new Rectangle(0, 0, 100, 100), new Rectangle(-1, 0, 100, 100)));
      Aver.AreEqual(20*100, CartesianUtils.CalculatePerimeterViolationArea(new Rectangle(0, 0, 100, 100), new Rectangle(-10, -10, 100, 100)));
    }

    [Run]
    public void FindRayFromRectangleCenterSideIntersection1()
    {
      var ray = CartesianUtils.FindRayFromRectangleCenterSideIntersection(new Rectangle(-50, -50, 100, 100), 0D);
      Aver.AreEqual(50, ray.X);
      Aver.AreEqual(0, ray.Y);
    }

    [Run]
    public void FindRayFromRectangleCenterSideIntersection2()
    {
      var ray = CartesianUtils.FindRayFromRectangleCenterSideIntersection(new Rectangle(-50, -50, 100, 100), Math.PI / 2);
      Aver.AreEqual(0, ray.X);
      Aver.AreEqual(50, ray.Y);
    }

    [Run]
    public void FindRayFromRectangleCenterSideIntersection3()
    {
      var ray = CartesianUtils.FindRayFromRectangleCenterSideIntersection(new Rectangle(-50, -50, 100, 100), Math.PI);
      Aver.AreEqual(-50, ray.X);
      Aver.AreEqual(0, ray.Y);
    }

    [Run]
    public void FindRayFromRectangleCenterSideIntersection4()
    {
      var ray = CartesianUtils.FindRayFromRectangleCenterSideIntersection(new Rectangle(-50, -50, 100, 100), Math.PI + Math.PI / 2);
      Aver.AreEqual(0, ray.X);
      Aver.AreEqual(-50, ray.Y);
    }

    [Run]
    public void PointToPolarPoint()
    {
      var center = new Point(0, 0);
      var pnt = new Point(150, 0);

      var polar = new PolarPoint(center, pnt);

      Aver.AreEqual(150D, polar.R);
      Aver.AreEqual(0D, polar.Theta);
    }

    [Run]
    public void PolarPointInvalid()
    {
      try
      {
        var pp = new PolarPoint(100, 789);
        Aver.Fail("Object was created");
      }
      catch (AzosException ex)
      {
        Aver.IsTrue(ex.Message.Contains("angle"));
      }
    }

    [Run]
    public void PolarPointRadius()
    {
      var polar = new PolarPoint(100D, Math.PI);
      Aver.AreEqual(100D, polar.R);

      polar.R = 125D;
      Aver.AreEqual(125D, polar.R);
    }

    [Run]
    public void PolarPointTheta()
    {
      var polar = new PolarPoint(100D, Math.PI);
      Aver.AreEqual(314D, Math.Floor(polar.Theta * 100D));

      polar.Theta = 1.18D;
      Aver.AreEqual(118D, Math.Floor(polar.Theta * 100D));
    }

    [Run]
    public void PolarPointToPoint()
    {
      var polar = new PolarPoint(100D, Math.PI / 4);
      var decart = polar.Point;

      Aver.AreEqual(70D, Math.Floor((double)decart.X));
      Aver.AreEqual(70D, Math.Floor((double)decart.Y));
    }

    [Run]
    public void PolarPointIsEqual()
    {
      var polar1 = new PolarPoint(100D, 1.2D);
      var polar2 = new PolarPoint(10D, 2.17D);
      var polar3 = new PolarPoint(100D, 1.2D);

      Aver.AreObjectsNotEqual(polar1, polar2);
      Aver.AreObjectsEqual(polar1, polar3);
    }

    [Run]
    public void VectorizeBalloon()
    {
      var rect = new Rectangle( -100, -100, 200, 200);
      var target = new Point(0, 300);
      var lagSweep = Math.PI / 16D;
      var points = VectorUtils.VectorizeBalloon(rect, target, lagSweep);

      foreach (var p in points)
        Console.WriteLine(p.ToString());

      Aver.AreEqual(7, points.Length);

      var expectedPoints = new int[,] { { 100, -100 }, { -100, -100 }, { -100, 100 }, { -9, 100 }, { 0, 300 }, { 9, 100 }, { 100, 100 } };
      for (int i = 0; i < points.Length; i++)
      {
        Aver.AreEqual(expectedPoints[i, 0], points[i].X);
        Aver.AreEqual(expectedPoints[i, 1], points[i].Y);
      }
    }
  }
}
