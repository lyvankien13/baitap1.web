using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BilliardsApp
{
    // Ball model
    public class Ball
    {
        public int Id;            // 0 = cue
        public int? Number;       // 1..15 or null for cue
        public float X, Y;
        public float VX, VY;
        public float Radius;
        public Color Color;
        public bool Pocketed;
        public float Angle, AngVel;
        public float Sink;

        public Ball(int id, int? number, float x, float y, float radius, Color color)
        {
            Id = id; Number = number; X = x; Y = y; Radius = radius; Color = color;
            VX = VY = 0f; Pocketed = false; Angle = AngVel = Sink = 0f;
        }
    }

    public class Form1 : Form
    {
        private Timer timer;
        private List<Ball> balls;
        private Random rnd;
        private float friction = 0.008f;
        private float powerMult = 1.0f;
        private bool showTraj = true;

        // table geometry
        private const int rail = 36;
        private const int pocketR = 36;
        private const float ballR = 14f;

        // input
        private PointF mousePos;
        private bool mouseDown = false;
        private bool dragging = false;

        // UI
        private ToolStrip tool;
        private ToolStripButton btnRack;
        private ToolStripButton btnStop;
        private ToolStripLabel lblFric;
        private ToolStripTextBox tbFric;
        private ToolStripLabel lblPower;
        private ToolStripTextBox tbPower;
        private ToolStripButton trajToggle;
        private StatusStrip status;
        private ToolStripStatusLabel statLbl;

        public Form1()
        {
            Text = "6-Pocket Pool — WinForms (.NET 2.0)";
            ClientSize = new Size(1200, 720);
            StartPosition = FormStartPosition.CenterScreen;

            // double buffering
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            UpdateStyles();

            balls = new List<Ball>();
            rnd = new Random();

            // toolbar
            tool = new ToolStrip();
            btnRack = new ToolStripButton("Rack (Space)");
            btnStop = new ToolStripButton("Stop/Reset (R)");
            lblFric = new ToolStripLabel("Friction");
            tbFric = new ToolStripTextBox();
            tbFric.Text = friction.ToString("0.000");
            tbFric.Width = 60;
            lblPower = new ToolStripLabel("Power");
            tbPower = new ToolStripTextBox();
            tbPower.Text = powerMult.ToString("0.00");
            tbPower.Width = 50;
            trajToggle = new ToolStripButton("Traj");
            trajToggle.CheckOnClick = true;
            trajToggle.Checked = showTraj;

            tool.Items.Add(btnRack);
            tool.Items.Add(btnStop);
            tool.Items.Add(new ToolStripSeparator());
            tool.Items.Add(lblFric);
            tool.Items.Add(tbFric);
            tool.Items.Add(lblPower);
            tool.Items.Add(tbPower);
            tool.Items.Add(new ToolStripSeparator());
            tool.Items.Add(trajToggle);

            Controls.Add(tool);

            // status
            status = new StatusStrip();
            statLbl = new ToolStripStatusLabel("Ready");
            status.Items.Add(statLbl);
            Controls.Add(status);

            // events
            btnRack.Click += new EventHandler(btnRack_Click);
            btnStop.Click += new EventHandler(btnStop_Click);
            trajToggle.CheckedChanged += new EventHandler(trajToggle_CheckedChanged);
            tbFric.Leave += new EventHandler(tbFric_Leave);
            tbPower.Leave += new EventHandler(tbPower_Leave);

            MouseDown += new MouseEventHandler(Form1_MouseDown);
            MouseMove += new MouseEventHandler(Form1_MouseMove);
            MouseUp += new MouseEventHandler(Form1_MouseUp);
            KeyDown += new KeyEventHandler(Form1_KeyDown);
            Resize += new EventHandler(Form1_Resize);

            // timer ~60fps
            timer = new Timer();
            timer.Interval = 16;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();

            Rack();
        }

        private void btnRack_Click(object sender, EventArgs e)
        {
            Rack();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopBalls();
        }

        private void trajToggle_CheckedChanged(object sender, EventArgs e)
        {
            showTraj = trajToggle.Checked;
        }

        private void tbFric_Leave(object sender, EventArgs e)
        {
            float v;
            if (float.TryParse(tbFric.Text, out v)) friction = v;
            tbFric.Text = friction.ToString("0.000");
        }

        private void tbPower_Leave(object sender, EventArgs e)
        {
            float v;
            if (float.TryParse(tbPower.Text, out v)) powerMult = v;
            tbPower.Text = powerMult.ToString("0.00");
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            DrawTable(g);
            DrawBalls(g);
            DrawCue(g);
        }

        private void DrawTable(Graphics g)
        {
            int W = ClientSize.Width;
            int H = ClientSize.Height;
            Rectangle tableRect = new Rectangle(rail, rail + tool.Height, W - rail * 2, H - rail * 2 - tool.Height - status.Height);

            // background
            using (SolidBrush bg = new SolidBrush(Color.FromArgb(22, 28, 36)))
            {
                g.FillRectangle(bg, ClientRectangle);
            }

            // rails gradient brush
            using (LinearGradientBrush railBrush = new LinearGradientBrush(tableRect, Color.FromArgb(60, 40, 30), Color.FromArgb(40, 30, 20), 45f))
            {
                g.FillRectangle(railBrush, tableRect.X - rail, tableRect.Y - rail, tableRect.Width + rail * 2, rail);
                g.FillRectangle(railBrush, tableRect.X - rail, tableRect.Y + tableRect.Height, tableRect.Width + rail * 2, rail);
                g.FillRectangle(railBrush, tableRect.X - rail, tableRect.Y - rail, rail, tableRect.Height + rail * 2);
                g.FillRectangle(railBrush, tableRect.X + tableRect.Width, tableRect.Y - rail, rail, tableRect.Height + rail * 2);
            }

            // felt
            using (LinearGradientBrush felt = new LinearGradientBrush(tableRect, Color.FromArgb(10, 120, 80), Color.FromArgb(6, 110, 60), 0f))
            {
                g.FillRectangle(felt, tableRect);
            }

            // pockets
            PointF[] pks = GetPockets(tableRect);
            for (int i = 0; i < pks.Length; i++)
            {
                PointF pk = pks[i];
                int rimR = pocketR + 6;
                using (SolidBrush rim = new SolidBrush(Color.FromArgb(200, 180, 150)))
                {
                    g.FillEllipse(rim, pk.X - rimR, pk.Y - rimR, rimR * 2, rimR * 2);
                }
                g.FillEllipse(Brushes.Black, pk.X - (pocketR - 6), pk.Y - (pocketR - 6), (pocketR - 6) * 2, (pocketR - 6) * 2);
            }
        }

        private void DrawBalls(Graphics g)
        {
            // copy to avoid collection modification during drawing
            Ball[] snap = balls.ToArray();
            for (int i = 0; i < snap.Length; i++)
            {
                Ball b = snap[i];
                if (b.Pocketed)
                {
                    float scale = 1f - Math.Min(1f, b.Sink);
                    int alpha = (int)(255 * Math.Max(0.1f, 1f - b.Sink));
                    int cx = (int)b.X, cy = (int)b.Y;
                    int r = (int)(b.Radius * scale);
                    if (r <= 0) continue;
                    using (SolidBrush sh = new SolidBrush(Color.FromArgb(alpha / 3, 0, 0, 0)))
                        g.FillEllipse(sh, cx - r, cy - r, r * 2, r * 2);
                    using (SolidBrush br = new SolidBrush(Color.FromArgb(alpha, b.Color)))
                        g.FillEllipse(br, cx - r, cy - r, r * 2, r * 2);
                    using (Pen pen = new Pen(Color.FromArgb(alpha, Color.Black), 1f))
                        g.DrawEllipse(pen, cx - r, cy - r, r * 2, r * 2);
                    continue;
                }

                float speed = (float)Math.Sqrt(b.VX * b.VX + b.VY * b.VY);
                float shadowScale = Math.Min(1.6f, speed / 6f);
                float shW = b.Radius * (1 + shadowScale * 0.6f);
                float shH = b.Radius * 0.55f;
                using (SolidBrush sb = new SolidBrush(Color.FromArgb(120, 0, 0, 0)))
                    g.FillEllipse(sb, b.X - shW + 3, b.Y - shH + 6, shW * 2, shH * 2);

                using (SolidBrush br = new SolidBrush(b.Color))
                    g.FillEllipse(br, b.X - b.Radius, b.Y - b.Radius, b.Radius * 2, b.Radius * 2);

                g.DrawEllipse(Pens.Black, b.X - b.Radius, b.Y - b.Radius, b.Radius * 2, b.Radius * 2);

                if (b.Number.HasValue)
                {
                    if (b.Number.Value >= 9)
                    {
                        using (SolidBrush white = new SolidBrush(Color.White))
                            g.FillRectangle(white, b.X - b.Radius, b.Y - b.Radius / 2f, b.Radius * 2, b.Radius);
                    }
                    using (SolidBrush white = new SolidBrush(Color.White))
                        g.FillEllipse(white, b.X - b.Radius * 0.72f, b.Y - b.Radius * 0.72f, b.Radius * 1.44f, b.Radius * 1.44f);

                    StringFormat sf = new StringFormat();
                    sf.Alignment = StringAlignment.Center;
                    sf.LineAlignment = StringAlignment.Center;
                    g.DrawString(b.Number.Value.ToString(), new Font("Segoe UI", 10, FontStyle.Bold), Brushes.Black, b.X, b.Y, sf);
                }
                else
                {
                    using (SolidBrush shine = new SolidBrush(Color.FromArgb(220, 255, 255, 255)))
                        g.FillEllipse(shine, b.X - 9, b.Y - 10, 8, 8);
                }
            }
        }

        private void DrawCue(Graphics g)
        {
            Ball cue = FindCue();
            if (cue == null || cue.Pocketed) return;
            if (mouseDown && dragging)
            {
                float dx = mousePos.X - cue.X;
                float dy = mousePos.Y - cue.Y;
                float ang = (float)Math.Atan2(dy, dx);
                float pull = Math.Min(240f, (float)Math.Sqrt(dx * dx + dy * dy));
                float stickLen = 200f + pull;

                g.TranslateTransform(cue.X, cue.Y);
                g.RotateTransform(ang * 180f / (float)Math.PI);
                RectangleF rect = new RectangleF(-stickLen, -6f, stickLen - (cue.Radius + 6f), 12f);
                using (LinearGradientBrush bg = new LinearGradientBrush(rect, Color.FromArgb(190, 191, 144), Color.FromArgb(75, 52, 33), 0f))
                {
                    g.FillRectangle(bg, rect);
                }
                g.ResetTransform();

                if (showTraj)
                {
                    using (Pen dashed = new Pen(Color.FromArgb(160, Color.White)))
                    {
                        dashed.DashStyle = DashStyle.Dash;
                        g.DrawLine(dashed, cue.X, cue.Y, mousePos.X, mousePos.Y);
                    }
                }
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            float dt = timer.Interval / 16.67f;
            UpdatePhysics(dt);
            UpdateStatus();
            Invalidate();
        }

        private void UpdateStatus()
        {
            int count = 0;
            for (int i = 0; i < balls.Count; i++) if (!balls[i].Pocketed) count++;
            statLbl.Text = "Balls: " + count.ToString() + "  Fric:" + friction.ToString("0.000") + " Power:" + powerMult.ToString("0.00");
        }

        private void UpdatePhysics(float dt)
        {
            Rectangle tableRect = GetTableRect();
            PointF[] pks = GetPockets(tableRect);

            // integrate
            for (int i = 0; i < balls.Count; i++)
            {
                Ball b = balls[i];
                if (b.Pocketed)
                {
                    b.Sink = Math.Min(1f, b.Sink + 0.02f * dt);
                    continue;
                }
                b.X += b.VX * dt;
                b.Y += b.VY * dt;

                b.AngVel += (b.VX * 0.02f) * 0.02f;
                b.Angle += b.AngVel * dt;
                b.AngVel *= 0.995f;

                float minX = tableRect.Left + b.Radius;
                float maxX = tableRect.Right - b.Radius;
                float minY = tableRect.Top + b.Radius;
                float maxY = tableRect.Bottom - b.Radius;

                if (b.X < minX) { b.X = minX; b.VX = -b.VX * 0.985f; b.VY *= 0.98f; b.AngVel *= 0.6f; }
                if (b.X > maxX) { b.X = maxX; b.VX = -b.VX * 0.985f; b.VY *= 0.98f; b.AngVel *= 0.6f; }
                if (b.Y < minY) { b.Y = minY; b.VY = -b.VY * 0.985f; b.VX *= 0.98f; b.AngVel *= 0.6f; }
                if (b.Y > maxY) { b.Y = maxY; b.VY = -b.VY * 0.985f; b.VX *= 0.98f; b.AngVel *= 0.6f; }

                b.VX *= (1f - friction * dt);
                b.VY *= (1f - friction * dt);

                if (Math.Sqrt(b.VX * b.VX + b.VY * b.VY) < 0.02) { b.VX = 0; b.VY = 0; b.AngVel *= 0.9f; }
            }

            // collisions pairwise
            for (int i = 0; i < balls.Count; i++)
            {
                Ball A = balls[i];
                if (A.Pocketed) continue;
                for (int j = i + 1; j < balls.Count; j++)
                {
                    Ball B = balls[j];
                    if (B.Pocketed) continue;
                    float dx = B.X - A.X, dy = B.Y - A.Y;
                    float d = (float)Math.Sqrt(dx * dx + dy * dy);
                    if (d == 0f) continue;
                    float minD = A.Radius + B.Radius;
                    if (d < minD)
                    {
                        float overlap = minD - d;
                        float nx = dx / d, ny = dy / d;
                        float shift = overlap * 0.5f;
                        A.X -= nx * shift; A.Y -= ny * shift;
                        B.X += nx * shift; B.Y += ny * shift;

                        float rvx = B.VX - A.VX, rvy = B.VY - A.VY;
                        float rel = rvx * nx + rvy * ny;
                        if (rel > 0f) rel = 0f;
                        float e = 0.985f;
                        float jimp = -(1f + e) * rel / 2f;
                        float ix = jimp * nx, iy = jimp * ny;
                        A.VX -= ix; A.VY -= iy;
                        B.VX += ix; B.VY += iy;

                        A.AngVel -= rel * 0.08f;
                        B.AngVel += rel * 0.08f;
                    }
                }
            }

            // pockets detection
            for (int i = 0; i < balls.Count; i++)
            {
                Ball b = balls[i];
                if (b.Pocketed) continue;
                for (int k = 0; k < pks.Length; k++)
                {
                    PointF pk = pks[k];
                    float dx = b.X - pk.X, dy = b.Y - pk.Y;
                    if (dx * dx + dy * dy < (pocketR - 6) * (pocketR - 6))
                    {
                        b.Pocketed = true; b.VX = 0f; b.VY = 0f; b.Sink = 0f;
                        if (b.Id == 0)
                        {
                            // cue respawn after delay
                            Ball cue = b;
                            Timer t = new Timer();
                            t.Interval = 700;
                            t.Tick += delegate (object s, EventArgs ea)
                            {
                                t.Stop();
                                t.Dispose();
                                cue.Pocketed = false;
                                cue.X = tableRect.Left + tableRect.Width * 0.18f;
                                cue.Y = tableRect.Top + tableRect.Height / 2f;
                                cue.VX = cue.VY = 0f;
                            };
                            t.Start();
                        }
                        break;
                    }
                }
            }
        }

        private Rectangle GetTableRect()
        {
            return new Rectangle(rail, rail + tool.Height, ClientSize.Width - rail * 2, ClientSize.Height - rail * 2 - tool.Height - status.Height);
        }

        private PointF[] GetPockets(Rectangle tableRect)
        {
            PointF[] p = new PointF[6];
            p[0] = new PointF(tableRect.Left, tableRect.Top);
            p[1] = new PointF(tableRect.Left + tableRect.Width / 2f, tableRect.Top);
            p[2] = new PointF(tableRect.Right, tableRect.Top);
            p[3] = new PointF(tableRect.Left, tableRect.Bottom);
            p[4] = new PointF(tableRect.Left + tableRect.Width / 2f, tableRect.Bottom);
            p[5] = new PointF(tableRect.Right, tableRect.Bottom);
            return p;
        }

        private void Rack()
        {
            balls.Clear();
            Rectangle t = GetTableRect();
            // cue
            balls.Add(new Ball(0, null, t.Left + t.Width * 0.18f, t.Top + t.Height / 2f, ballR, Color.White));

            // triangle rack
            float startX = t.Left + t.Width * 0.68f;
            float startY = t.Top + t.Height / 2f;
            int id = 1;
            float gap = ballR * 2f + 2f;
            for (int row = 0; row < 5; row++)
            {
                float offsetX = row * (gap * (float)Math.Cos(Math.PI / 6f));
                float rowY = startY - (row * gap) / 2f;
                for (int col = 0; col <= row; col++)
                {
                    if (id > 15) break;
                    float x = startX + offsetX;
                    float y = rowY + col * gap;
                    Color c = GetBallColor(id);
                    balls.Add(new Ball(id, id, x, y, ballR, c));
                    id++;
                }
                if (id > 15) break;
            }
        }

        private void StopBalls()
        {
            for (int i = 0; i < balls.Count; i++)
            {
                balls[i].VX = balls[i].VY = 0f;
            }
        }

        private Color GetBallColor(int n)
        {
            Color[] defs = new Color[]
            {
                Color.Empty,
                Color.FromArgb(247,181,0), Color.FromArgb(30,144,255), Color.FromArgb(255,59,48),
                Color.FromArgb(255,138,0), Color.FromArgb(0,179,131), Color.FromArgb(122,0,255), Color.FromArgb(255,78,192),
                Color.Black,
                Color.FromArgb(247,181,0), Color.FromArgb(30,144,255), Color.FromArgb(255,59,48),
                Color.FromArgb(255,138,0), Color.FromArgb(0,179,131), Color.FromArgb(122,0,255), Color.FromArgb(255,78,192)
            };
            if (n >= 1 && n < defs.Length) return defs[n];
            return Color.Gray;
        }

        private Ball FindCue()
        {
            for (int i = 0; i < balls.Count; i++)
            {
                if (balls[i].Id == 0) return balls[i];
            }
            return null;
        }

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            mousePos = e.Location;
            mouseDown = true;
            Ball cue = FindCue();
            if (cue != null && !cue.Pocketed)
            {
                float dx = mousePos.X - cue.X;
                float dy = mousePos.Y - cue.Y;
                if (dx * dx + dy * dy <= (cue.Radius + 8f) * (cue.Radius + 8f))
                {
                    dragging = true;
                }
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            mousePos = e.Location;
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!dragging) { mouseDown = false; dragging = false; return; }
            Ball cue = FindCue();
            if (cue == null) return;
            float dx = mousePos.X - cue.X;
            float dy = mousePos.Y - cue.Y;
            float pull = Math.Min(240f, (float)Math.Sqrt(dx * dx + dy * dy));
            float power = (pull / 120f) * powerMult;
            if (power < 0.02f) { dragging = false; mouseDown = false; return; }
            float ang = (float)Math.Atan2(dy, dx);
            float speed = power * 28f;
            cue.VX = (float)Math.Cos(ang) * speed;
            cue.VY = (float)Math.Sin(ang) * speed;
            cue.AngVel += (speed * 0.02f) * ((float)rnd.NextDouble() * 0.6f - 0.3f);
            dragging = false; mouseDown = false;
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space) Rack();
            if (e.KeyCode == Keys.R)
            {
                Ball cue = FindCue();
                if (cue != null)
                {
                    Rectangle t = GetTableRect();
                    cue.X = t.Left + t.Width * 0.18f;
                    cue.Y = t.Top + t.Height / 2f;
                    cue.VX = cue.VY = 0f;
                    cue.Pocketed = false;
                    cue.Sink = 0f;
                }
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}