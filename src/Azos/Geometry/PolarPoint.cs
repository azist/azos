/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Drawing;

namespace Azos.Geometry
{
  /// <summary>
  /// Represents a 2D point with polar coordinates
  /// </summary>
  public struct PolarPoint : IEquatable<PolarPoint>
  {
    #region .ctor
    /// <summary>
    /// Initializes polar coordinates
    /// </summary>
    public PolarPoint(double r, double theta)
    {
      m_R = r;
      m_Theta = 0;
      Theta = theta;
    }

    /// <summary>
    /// Initializes polar coordinates from 2-d cartesian coordinates
    /// </summary>
    public PolarPoint(Point center, Point point)
    {
      this = CartesianUtils.PointToPolarPoint(center, point);
    }

    /// <summary>
    /// Initializes polar coordinates from 2-d cartesian coordinates
    /// </summary>
    public PolarPoint(PointF center, PointF point)
    {
      this = CartesianUtils.PointToPolarPoint(center, point);
    }

    /// <summary>
    /// Initializes polar coordinates from 2-d cartesian coordinates of 'x1, y1, x2, y2' format
    /// </summary>
    public PolarPoint(double x1, double y1, double x2, double y2)
    {
      this = CartesianUtils.VectorToPolarPoint(x1, y1, x2, y2);
    }
    #endregion

    #region Private Fields
    private double m_R;
    private double m_Theta;
    #endregion


    #region Properties
    /// <summary>
    /// R coordinate component which is coordinate distance from point of coordinates origin
    /// </summary>
    public double R
    {
      get { return m_R; }
      set { m_R = value; }
    }


    /// <summary>
    /// Angular azimuth coordinate component. An angle must be between 0 and 2Pi.
    /// Note: Due to screen Y coordinate going from top to bottom (in usual orientation)
    ///  Theta angle may be reversed, that is - be positive in the lower half coordinate plane.
    /// Please refer to:
    ///  http://en.wikipedia.org/wiki/Polar_coordinates
    /// </summary>
    public double Theta
    {
      get { return m_Theta; }
      set
      {
        if ((value < 0) || (value > Math.PI * 2))
          throw new AzosException("Invalid polar coordinates angle");
        m_Theta = value;
      }
    }

    /// <summary>
    /// Returns polar coordinate converted to 2-d cartesian coordinates.
    /// Coordinates are relative to 0,0 of the angle base vertex
    /// </summary>
    public Point Point
    {
      get
      {
        int x = (int)(m_R * Math.Cos(m_Theta));
        int y = (int)(m_R * Math.Sin(m_Theta));
        return new Point(x, y);
      }
    }

    /// <summary>
    /// Returns polar coordinate converted to 2-d cartesian coordinates.
    /// Coordinates are relative to 0,0 of the angle base vertex
    /// </summary>
    public PointF PointF
    {
      get
      {
        float x = (float)(m_R * Math.Cos(m_Theta));
        float y = (float)(m_R * Math.Sin(m_Theta));
        return new PointF(x, y);
      }
    }
    #endregion


    #region Operators
    public static bool operator ==(PolarPoint left, PolarPoint right)
    {
      return (left.m_R == right.m_R) && (left.m_Theta == right.m_Theta);
    }

    public static bool operator !=(PolarPoint left, PolarPoint right)
    {
      return (left.m_R != right.m_R) || (left.m_Theta != right.m_Theta);
    }
    #endregion


    #region Object overrides
    public override bool Equals(object obj) => obj is PolarPoint pp ? this.Equals(pp) : false;
    public bool Equals(PolarPoint other) => this.m_R == other.m_R && this.m_Theta == other.m_Theta;
    public override int GetHashCode() => m_R.GetHashCode() + m_Theta.GetHashCode();
    public override string ToString() => $"R: {m_R}; T: {m_Theta} rad.";
    #endregion
  }

}