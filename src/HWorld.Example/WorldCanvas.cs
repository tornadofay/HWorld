using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HWorld.Core.Geometry;
using HWorld.Core.World;

namespace HWorld.Example
{
    internal enum CanvasMode { Observe, Build, Play }

    internal sealed class WorldCanvas : Control
    {
        private static readonly SolidBrush GroundBrush = new SolidBrush(Color.FromArgb(18, 24, 30));
        private static readonly Pen WorldOutlinePen = new Pen(Color.FromArgb(90, 106, 122), 1.4f);
        private static readonly Pen GridPen = new Pen(Color.FromArgb(17, 255, 255, 255), 1f);
        private static readonly Pen MajorGridPen = new Pen(Color.FromArgb(34, 255, 255, 255), 1f);
        private static readonly Pen BuildBorderPen = new Pen(Color.FromArgb(190, 115, 230, 255), 1f) { DashStyle = DashStyle.Dash };
        private static readonly SolidBrush SolidFill = new SolidBrush(Color.FromArgb(220, 166, 79, 92));
        private static readonly Pen SolidPen = new Pen(Color.FromArgb(240, 255, 157, 170), 1f);
        private static readonly SolidBrush NatureFill = new SolidBrush(Color.FromArgb(220, 60, 150, 104));
        private static readonly Pen NaturePen = new Pen(Color.FromArgb(240, 127, 219, 155), 1f);
        private static readonly SolidBrush ResourceFill = new SolidBrush(Color.FromArgb(220, 91, 128, 204));
        private static readonly Pen ResourcePen = new Pen(Color.FromArgb(240, 163, 190, 245), 1f);
        private static readonly SolidBrush LandmarkFill = new SolidBrush(Color.FromArgb(220, 155, 104, 211));
        private static readonly Pen LandmarkPen = new Pen(Color.FromArgb(242, 218, 166, 247), 1f);
        private static readonly SolidBrush ObjectFill = new SolidBrush(Color.FromArgb(220, 71, 137, 190));
        private static readonly Pen ObjectPen = new Pen(Color.FromArgb(235, 139, 203, 246), 1f);
        private static readonly SolidBrush PlayerGlow = new SolidBrush(Color.FromArgb(35, 102, 224, 255));
        private static readonly SolidBrush PlayerFill = new SolidBrush(Color.FromArgb(245, 93, 196, 255));
        private static readonly Pen PlayerPen = new Pen(Color.FromArgb(255, 222, 248, 255), 1.6f);
        private static readonly Pen PlayerFacingPen = new Pen(Color.White, 2f);

        private World _world;
        private WorldActor _player;
        private float _zoom = 1f;
        private PointF _pan;
        private Point _lastMouse;
        private bool _panning;

        public WorldCanvas()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(10, 13, 17);
            TabStop = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public event EventHandler WorldEdited;
        public World World { get => _world; set { _world = value; FitWorld(); Invalidate(); } }
        public WorldActor Player { get => _player; set { _player = value; Invalidate(); } }
        public CanvasMode Mode { get; set; }
        public string BuildKind { get; set; } = "object";
        public WorldShapeKind BuildShape { get; set; } = WorldShapeKind.Rectangle;
        public bool BuildSolid { get; set; }
        public double BuildWidth { get; set; } = 8;
        public double BuildHeight { get; set; } = 8;
        public float Zoom => _zoom;

        public void ResetView() { FitWorld(); Invalidate(); }
        public void CenterOnPlayer()
        {
            if (_world == null || _player == null) return;
            var scale = GetScale();
            _pan = new PointF(ClientSize.Width / 2f - (float)_player.Position.X * scale - 24f,
                              ClientSize.Height / 2f - (float)_player.Position.Y * scale - 24f);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(BackColor);
            if (_world == null || ClientSize.Width <= 0 || ClientSize.Height <= 0) return;

            var scale = GetScale();
            var origin = new PointF(24f + _pan.X, 24f + _pan.Y);
            var worldRect = new RectangleF(origin.X, origin.Y, (float)_world.Width * scale, (float)_world.Height * scale);
            g.FillRectangle(GroundBrush, worldRect);
            g.DrawRectangle(WorldOutlinePen, worldRect.X, worldRect.Y, worldRect.Width, worldRect.Height);
            DrawGrid(g, scale, origin, worldRect);
            DrawItems(g, scale, origin);
            DrawPlayer(g, scale, origin);
            if (Mode == CanvasMode.Build)
                g.DrawRectangle(BuildBorderPen, 12, 12, ClientSize.Width - 24, ClientSize.Height - 24);
        }

        private void DrawItems(Graphics g, float scale, PointF origin)
        {
            for (int i = 0; i < _world.Items.Count; i++)
            {
                var item = _world.Items[i];
                var x = origin.X + (float)item.Position.X * scale;
                var y = origin.Y + (float)item.Position.Y * scale;
                var w = Math.Max(4f, (float)item.Width * scale);
                var h = Math.Max(4f, (float)item.Height * scale);
                var cx = x + w * 0.5f;
                var cy = y + h * 0.5f;
                SolidBrush fill;
                Pen border;
                GetPalette(item, out fill, out border);

                GraphicsState state = null;
                if (item.RotationDegrees != 0)
                {
                    state = g.Save();
                    g.TranslateTransform(cx, cy);
                    g.RotateTransform((float)item.RotationDegrees);
                    g.TranslateTransform(-cx, -cy);
                }

                DrawVectorShape(g, item.Shape, x, y, w, h, fill, border);
                if (state != null) g.Restore(state);
            }
        }

        private static void DrawVectorShape(Graphics g, WorldShapeKind shape, float x, float y, float w, float h, Brush fill, Pen border)
        {
            var cx = x + w * 0.5f;
            var cy = y + h * 0.5f;
            switch (shape)
            {
                case WorldShapeKind.Ellipse:
                    g.FillEllipse(fill, x, y, w, h); g.DrawEllipse(border, x, y, w, h); break;
                case WorldShapeKind.Triangle:
                    g.FillPolygon(fill, new[] { new PointF(cx, y), new PointF(x + w, y + h), new PointF(x, y + h) });
                    g.DrawPolygon(border, new[] { new PointF(cx, y), new PointF(x + w, y + h), new PointF(x, y + h) }); break;
                case WorldShapeKind.Diamond:
                    g.FillPolygon(fill, new[] { new PointF(cx, y), new PointF(x + w, cy), new PointF(cx, y + h), new PointF(x, cy) });
                    g.DrawPolygon(border, new[] { new PointF(cx, y), new PointF(x + w, cy), new PointF(cx, y + h), new PointF(x, cy) }); break;
                case WorldShapeKind.Hexagon:
                    DrawPolygon(g, 6, cx, cy, Math.Min(w, h) * 0.5f, -30f, fill, border); break;
                case WorldShapeKind.Star:
                    DrawStar(g, cx, cy, Math.Min(w, h) * 0.5f, fill, border); break;
                case WorldShapeKind.Tree:
                    g.FillRectangle(border.Brush, cx - Math.Max(2f, w * .11f), y + h * .56f, Math.Max(4f, w * .22f), h * .40f);
                    g.FillEllipse(fill, x + w * .07f, y + h * .03f, w * .86f, h * .70f);
                    g.DrawEllipse(border, x + w * .07f, y + h * .03f, w * .86f, h * .70f); break;
                case WorldShapeKind.House:
                    var body = new RectangleF(x + w * .12f, y + h * .36f, w * .76f, h * .52f);
                    g.FillRectangle(fill, body); g.DrawRectangle(border, body.X, body.Y, body.Width, body.Height);
                    var roof = new[] { new PointF(x + w * .05f, y + h * .38f), new PointF(cx, y + h * .04f), new PointF(x + w * .95f, y + h * .38f) };
                    g.FillPolygon(fill, roof); g.DrawPolygon(border, roof); break;
                case WorldShapeKind.Rock:
                    var rock = new[] { new PointF(x + w*.10f,y+h*.75f),new PointF(x+w*.22f,y+h*.34f),new PointF(x+w*.52f,y+h*.12f),new PointF(x+w*.84f,y+h*.30f),new PointF(x+w*.94f,y+h*.72f),new PointF(x+w*.67f,y+h*.92f),new PointF(x+w*.30f,y+h*.91f) };
                    g.FillPolygon(fill, rock); g.DrawPolygon(border, rock); break;
                case WorldShapeKind.Flower:
                    var r = Math.Min(w,h)*.22f;
                    for(int i=0;i<5;i++){var a=(float)(i*Math.PI*2/5);var px=cx+(float)Math.Cos(a)*r;var py=cy+(float)Math.Sin(a)*r;g.FillEllipse(fill,px-r,py-r,r*2,r*2);g.DrawEllipse(border,px-r,py-r,r*2,r*2);} g.FillEllipse(border.Brush,cx-r*.45f,cy-r*.45f,r*.9f,r*.9f); break;
                case WorldShapeKind.Pillar:
                    g.FillRectangle(fill,x+w*.22f,y+h*.14f,w*.56f,h*.72f);g.DrawRectangle(border,x+w*.22f,y+h*.14f,w*.56f,h*.72f);g.FillRectangle(border.Brush,x+w*.1f,y+h*.05f,w*.8f,Math.Max(2,h*.1f));g.FillRectangle(border.Brush,x+w*.1f,y+h*.85f,w*.8f,Math.Max(2,h*.1f)); break;
                case WorldShapeKind.Cross:
                    g.FillRectangle(fill,cx-w*.16f,y,w*.32f,h);g.FillRectangle(fill,x,cy-h*.16f,w,h*.32f);g.DrawRectangle(border,cx-w*.16f,y,w*.32f,h);g.DrawRectangle(border,x,cy-h*.16f,w,h*.32f); break;
                default:
                    g.FillRectangle(fill, x, y, w, h); g.DrawRectangle(border, x, y, w, h); break;
            }
        }

        private static void DrawPolygon(Graphics g, int sides, float cx, float cy, float radius, float rotation, Brush fill, Pen border)
        {
            var points = new PointF[sides]; var offset = rotation * (float)Math.PI / 180f;
            for (int i=0;i<sides;i++){var a=offset+i*(float)(Math.PI*2/sides);points[i]=new PointF(cx+(float)Math.Cos(a)*radius,cy+(float)Math.Sin(a)*radius);}
            g.FillPolygon(fill, points); g.DrawPolygon(border, points);
        }

        private static void DrawStar(Graphics g, float cx, float cy, float radius, Brush fill, Pen border)
        {
            var points = new PointF[10];
            for (int i=0;i<10;i++){var r=i%2==0?radius:radius*.43f;var a=(float)(-Math.PI/2+i*Math.PI/5);points[i]=new PointF(cx+(float)Math.Cos(a)*r,cy+(float)Math.Sin(a)*r);}
            g.FillPolygon(fill,points);g.DrawPolygon(border,points);
        }

        private static void GetPalette(WorldItem item, out SolidBrush fill, out Pen border)
        {
            if (item.Solid) { fill=SolidFill; border=SolidPen; }
            else if (string.Equals(item.Kind,"nature",StringComparison.OrdinalIgnoreCase)) { fill=NatureFill;border=NaturePen; }
            else if (string.Equals(item.Kind,"resource",StringComparison.OrdinalIgnoreCase)) { fill=ResourceFill;border=ResourcePen; }
            else if (string.Equals(item.Kind,"landmark",StringComparison.OrdinalIgnoreCase)) { fill=LandmarkFill;border=LandmarkPen; }
            else { fill=ObjectFill;border=ObjectPen; }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e); if (_world==null)return;
            var before=ScreenToWorld(e.Location); _zoom=Clamp(_zoom*(e.Delta>0?1.12f:1f/1.12f),.2f,12f); var after=ScreenToWorld(e.Location);
            _pan.X+=(after.X-before.X)*GetScale();_pan.Y+=(after.Y-before.Y)*GetScale();Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);Focus();
            if(Mode==CanvasMode.Build&&e.Button==MouseButtons.Left){var p=ScreenToWorld(e.Location);if(p.X>=0&&p.Y>=0&&p.X<_world.Width&&p.Y<_world.Height){var item=_world.AddItem(new WorldPoint(p.X,p.Y),BuildWidth,BuildHeight,BuildSolid);item.Kind=BuildKind;item.Name=BuildKind;item.Shape=BuildShape;WorldEdited?.Invoke(this,EventArgs.Empty);Invalidate();}return;}
            if(Mode==CanvasMode.Build&&e.Button==MouseButtons.Right){var p=ScreenToWorld(e.Location);var item=_world.FindItemAt(new WorldPoint(p.X,p.Y));if(item!=null){_world.RemoveItem(item.Id);WorldEdited?.Invoke(this,EventArgs.Empty);Invalidate();}return;}
            if((e.Button==MouseButtons.Middle||e.Button==MouseButtons.Right)&&Mode!=CanvasMode.Build){_panning=true;_lastMouse=e.Location;Cursor=Cursors.SizeAll;}
        }
        protected override void OnMouseMove(MouseEventArgs e){base.OnMouseMove(e);if(!_panning)return;_pan.X+=e.X-_lastMouse.X;_pan.Y+=e.Y-_lastMouse.Y;_lastMouse=e.Location;Invalidate();}
        protected override void OnMouseUp(MouseEventArgs e){base.OnMouseUp(e);if(e.Button==MouseButtons.Middle||e.Button==MouseButtons.Right){_panning=false;Cursor=Cursors.Default;}}

        private float GetScale()=>GetBaseScale()*_zoom;
        private float GetBaseScale(){if(_world==null||_world.Width<=0||_world.Height<=0)return 1f;return Math.Max(.05f,Math.Min((ClientSize.Width-48f)/(float)_world.Width,(ClientSize.Height-48f)/(float)_world.Height));}
        private void FitWorld(){if(_world==null||ClientSize.Width<=0||ClientSize.Height<=0)return;_zoom=1f;var scale=GetBaseScale();var width=(float)_world.Width*scale;var height=(float)_world.Height*scale;_pan=new PointF((ClientSize.Width-48f-width)/2f,(ClientSize.Height-48f-height)/2f);}
        private void DrawGrid(Graphics g,float scale,PointF origin,RectangleF worldRect){var step=Math.Max(1,(int)Math.Ceiling(20f/Math.Max(scale,.01f)));var major=step*5;for(int x=0;x<=_world.Width;x+=step){var px=origin.X+x*scale;if(px>=worldRect.Left&&px<=worldRect.Right)g.DrawLine(x%major==0?MajorGridPen:GridPen,px,worldRect.Top,px,worldRect.Bottom);}for(int y=0;y<=_world.Height;y+=step){var py=origin.Y+y*scale;if(py>=worldRect.Top&&py<=worldRect.Bottom)g.DrawLine(y%major==0?MajorGridPen:GridPen,worldRect.Left,py,worldRect.Right,py);}}
        private void DrawPlayer(Graphics g,float scale,PointF origin){if(_player==null)return;var cx=origin.X+(float)_player.Position.X*scale;var cy=origin.Y+(float)_player.Position.Y*scale;var radius=Math.Max(7f,(float)Math.Min(_player.Width,_player.Height)*scale*.6f);g.FillEllipse(PlayerGlow,cx-radius-5,cy-radius-5,(radius+5)*2,(radius+5)*2);g.FillEllipse(PlayerFill,cx-radius,cy-radius,radius*2,radius*2);g.DrawEllipse(PlayerPen,cx-radius,cy-radius,radius*2,radius*2);var a=(float)(_player.RotationDegrees*Math.PI/180);var len=radius+10;g.DrawLine(PlayerFacingPen,cx,cy,cx+(float)Math.Cos(a)*len,cy+(float)Math.Sin(a)*len);}
        private PointF ScreenToWorld(Point point){var scale=GetScale();var origin=new PointF(24f+_pan.X,24f+_pan.Y);return new PointF((point.X-origin.X)/scale,(point.Y-origin.Y)/scale);}
        private static float Clamp(float value,float min,float max)=>Math.Max(min,Math.Min(max,value));
    }
}
