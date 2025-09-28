// Ball.cs
using System;
using System.Drawing;

namespace BidaEngine
{
    public class Ball
    {
        public int Id;
        public double X;
        public double Y;
        public double VX;
        public double VY;
        public double Radius;

        public Ball(int id, double x, double y, double radius)
        {
            this.Id = id;
            this.X = x;
            this.Y = y;
            this.Radius = radius;
            this.VX = 0.0;
            this.VY = 0.0;
        }

        public void ApplyVelocity(double vx, double vy)
        {
            this.VX = vx;
            this.VY = vy;
        }

        public void UpdatePosition(double dt)
        {
            X += VX * dt;
            Y += VY * dt;
        }
    }
}
