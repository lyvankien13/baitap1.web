using System;
using BidaEngine;
using System.Threading;

class Program
{
    static void Main()
    {
        TableSimulator sim = new TableSimulator(800, 400);
        Ball cue = new Ball(0, 100, 200, 10);
        Ball target = new Ball(1, 400, 200, 10);
        cue.ApplyVelocity(200, 0); // px/s
        sim.AddBall(cue);
        sim.AddBall(target);

        double t = 0.0;
        double dt = 0.02; // 20ms
        for (int step = 0; step < 300; step++)
        {
            sim.Step(dt);
            t += dt;
            Console.WriteLine("t={0:0.00}s Cue({1:0.0},{2:0.0}) v({3:0.0},{4:0.0}) | Target({5:0.0},{6:0.0}) v({7:0.0},{8:0.0})",
                t, cue.X, cue.Y, cue.VX, cue.VY, target.X, target.Y, target.VX, target.VY);
            Thread.Sleep(20);
        }
        Console.WriteLine("Done.");
    }
}
