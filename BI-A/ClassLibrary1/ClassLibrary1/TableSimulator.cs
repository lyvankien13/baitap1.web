// TableSimulator.cs
using System;
using System.Collections;
using System.Collections.Generic;

namespace BidaEngine
{
    public class TableSimulator
    {
        public double Width;
        public double Height;
        public ArrayList Balls; // .NET 2.0 compatible. Can use List<Ball> if you prefer.

        public TableSimulator(double width, double height)
        {
            this.Width = width;
            this.Height = height;
            this.Balls = new ArrayList();
        }

        public void AddBall(Ball b)
        {
            this.Balls.Add(b);
        }

        // Very simple step: update positions and handle wall & pairwise elastic collisions
        public void Step(double dt)
        {
            // update positions
            foreach (Ball b in Balls)
            {
                b.UpdatePosition(dt);
                HandleWallCollision(b);
            }

            // pairwise collisions
            int n = Balls.Count;
            for (int i = 0; i < n; i++)
            {
                Ball a = (Ball)Balls[i];
                for (int j = i + 1; j < n; j++)
                {
                    Ball b = (Ball)Balls[j];
                    HandleBallCollision(a, b);
                }
            }

            // simple friction (dampen velocities)
            foreach (Ball b in Balls)
            {
                b.VX *= 0.995; // small damping
                b.VY *= 0.995;
                if (Math.Abs(b.VX) < 0.0001) b.VX = 0.0;
                if (Math.Abs(b.VY) < 0.0001) b.VY = 0.0;
            }
        }

        private void HandleWallCollision(Ball b)
        {
            if (b.X - b.Radius < 0)
            {
                b.X = b.Radius;
                b.VX = -b.VX;
            }
            else if (b.X + b.Radius > Width)
            {
                b.X = Width - b.Radius;
                b.VX = -b.VX;
            }

            if (b.Y - b.Radius < 0)
            {
                b.Y = b.Radius;
                b.VY = -b.VY;
            }
            else if (b.Y + b.Radius > Height)
            {
                b.Y = Height - b.Radius;
                b.VY = -b.VY;
            }
        }

        private void HandleBallCollision(Ball a, Ball b)
        {
            double dx = b.X - a.X;
            double dy = b.Y - a.Y;
            double dist = Math.Sqrt(dx * dx + dy * dy);
            double minDist = a.Radius + b.Radius;
            if (dist <= 0.0) return; // avoid div by zero
            if (dist < minDist)
            {
                // push them apart (minimum translation vector)
                double overlap = 0.5 * (minDist - dist);
                double nx = dx / dist;
                double ny = dy / dist;
                a.X -= nx * overlap;
                a.Y -= ny * overlap;
                b.X += nx * overlap;
                b.Y += ny * overlap;

                // simple elastic collision along normal
                double dvx = b.VX - a.VX;
                double dvy = b.VY - a.VY;
                double rel = dvx * nx + dvy * ny;
                if (rel > 0) return; // already separating

                double restitution = 0.98; // nearly elastic
                double impulse = (-(1 + restitution) * rel) / 2.0; // equal mass
                double ix = impulse * nx;
                double iy = impulse * ny;

                a.VX -= ix;
                a.VY -= iy;
                b.VX += ix;
                b.VY += iy;
            }
        }
    }
}
