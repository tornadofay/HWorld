using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using HWorld.Core.Geometry;
using HWorld.Core.World;

namespace HWorld.WinForms.Rendering
{
    public enum CanvasMode
    {
        Observe,
        Build,
        Play
    }

    public sealed class GdiWorldCanvas : Control
    {
        private static readonly SolidBrush GroundBrush = new SolidBrush(Color.FromArgb(18, 24, 30));
        private static readonly Pen WorldOutlinePen = new Pen(Color.FromArgb(90, 106, 122), 1.4f);
        private static readonly Pen GridPen = new Pen(Color.FromArgb(17, 255, 255, 255), 1f);
        private static readonly Pen MajorGridPen = new Pen(Color.FromArgb(34, 255, 255, 255), 1f);
        private static readonly Pen BuildBorderPen = new Pen(Color.FromArgb(190, 115, 230, 255), 1f) { DashStyle = DashStyle.Dash };
        private static readonly Pen SelectionPen = new Pen(Color.FromArgb(240, 104, 213, 255), 1.5f) { DashStyle = DashStyle.Dash };
        private static readonly SolidBrush SelectionHandleBrush = new SolidBrush(Color.FromArgb(245, 104, 213, 255));
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
        private static readonly SolidBrush DetailBrush = new SolidBrush(Color.FromArgb(230, 73, 63, 62));
        private static readonly SolidBrush PlayerGlow = new SolidBrush(Color.FromArgb(35, 102, 224, 255));
        private static readonly SolidBrush PlayerFill = new SolidBrush(Color.FromArgb(245, 93, 196, 255));
        private static readonly Pen PlayerPen = new Pen(Color.FromArgb(255, 222, 248, 255), 1.6f);
        private static readonly Pen PlayerFacingPen = new Pen(Color.White, 2f);

        private readonly PointF[] _triangle = new PointF[3];
        private readonly PointF[] _diamond = new PointF[4];
        private readonly PointF[] _hexagon = new PointF[6];
        private readonly PointF[] _star = new PointF[10];
        private readonly PointF[] _rock = new PointF[7];
        private readonly PointF[] _roof = new PointF[3];

        private World _world;
        private WorldActor _player;
        private WorldItem _selectedItem;
        private float _zoom = 1f;
        private PointF _pan;
        private Point _lastMouse;
        private bool _panning;
        private bool _draggingItem;
        private PointF _dragOffset;

        public GdiWorldCanvas()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.FromArgb(10, 13, 17);
            TabStop = true;
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public event EventHandler WorldEdited;
        public event EventHandler SelectionChanged;

        public World World
        {
            get { return _world; }
            set { _world = value; _selectedItem = null; FitWorld(); Invalidate(); }
        }

        public WorldActor Player
        {
            get { return _player; }
            set { _player = value; Invalidate(); }
        }

        public CanvasMode Mode { get; set; }
        public string BuildKind { get; set; } = "object";
        public WorldShapeKind BuildShape { get; set; } = WorldShapeKind.Rectangle;
        public bool BuildSolid { get; set; }
        public double BuildWidth { get; set; } = 8;
        public double BuildHeight { get; set; } = 8;
        public float Zoom { get { return _zoom; } }
        public WorldItem SelectedItem { get { return _selectedItem; } }

        public void ResetView() { FitWorld(); Invalidate(); }

        public void CenterOnPlayer()
        {
            if (_world == null || _player == null) return;
            var scale = GetScale();
            _pan = new PointF(ClientSize.Width / 2f - (float)_player.Position.X * scale - 24f,
                              ClientSize.Height / 2f - (float)_player.Position.Y * scale - 24f);
            Invalidate();
        }

        public void DeleteSelectedItem()
        {
            if (_world == null || _selectedItem == null) return;
            var id = _selectedItem.Id;
            SelectItem(null);
            _world.RemoveItem(id);
            WorldEdited?.Invoke(this, EventArgs.Empty);
            Invalidate();
        }

        public void ClampSelectedItemToWorld()
        {
            if (_world == null || _selectedItem == null) return;
            var item = _selectedItem;
            var maxX = Math.Max(0.0, _world.Width - item.Width);
            var maxY = Math.Max(0.0, _world.Height - item.Height);
            item.Position = new WorldPoint(
                Math.Max(0.0, Math.Min(maxX, item.Position.X)),
                Math.Max(0.0, Math.Min(maxY, item.Position.Y)));
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
                if (ReferenceEquals(item, _selectedItem)) DrawSelection(g, x, y, w, h, item.RotationDegrees);
            }
        }

        private void DrawPlayer(Graphics g, float scale, PointF origin)
        {
            if (_player == null) return;
            var cx = origin.X + (float)_player.Position.X * scale;
            var cy = origin.Y + (float)_player.Position.Y * scale;
            var radius = Math.Max(7f, (float)Math.Min(_player.Width, _player.Height) * scale * 0.6f);
            g.FillEllipse(PlayerGlow, cx - radius - 5f, cy - radius - 5f, (radius + 5f) * 2f, (radius + 5f) * 2f);
            g.FillEllipse(PlayerFill, cx - radius, cy - radius, radius * 2f, radius * 2f);
            g.DrawEllipse(PlayerPen, cx - radius, cy - radius, radius * 2f, radius * 2f);
            var angle = (float)(_player.RotationDegrees * Math.PI / 180.0);
            var length = radius + 10f;
            g.DrawLine(PlayerFacingPen, cx, cy, cx + (float)Math.Cos(angle) * length, cy + (float)Math.Sin(angle) * length);
        }

        private void DrawSelection(Graphics g, float x, float y, float w, float h, double rotation)
        {
            var cx = x + w * 0.5f;
            var cy = y + h * 0.5f;
            GraphicsState state = null;
            if (rotation != 0)
            {
                state = g.Save();
                g.TranslateTransform(cx, cy);
                g.RotateTransform((float)rotation);
                g.TranslateTransform(-cx, -cy);
            }
            g.DrawRectangle(SelectionPen, x - 3f, y - 3f, w + 6f, h + 6f);
            const float handle = 5f;
            g.FillRectangle(SelectionHandleBrush, x - handle, y - handle, handle * 2f, handle * 2f);
            g.FillRectangle(SelectionHandleBrush, x + w - handle, y + h - handle, handle * 2f, handle * 2f);
            if (state != null) g.Restore(state);
        }

        private void DrawVectorShape(Graphics g, WorldShapeKind shape, float x, float y, float w, float h, Brush fill, Pen border)
        {
            var cx = x + w * 0.5f;
            var cy = y + h * 0.5f;
            switch (shape)
            {
                case WorldShapeKind.Ellipse:
                    g.FillEllipse(fill, x, y, w, h); g.DrawEllipse(border, x, y, w, h); break;
                case WorldShapeKind.Triangle:
                    _triangle[0] = new PointF(cx, y); _triangle[1] = new PointF(x + w, y + h); _triangle[2] = new PointF(x, y + h);
                    g.FillPolygon(fill, _triangle); g.DrawPolygon(border, _triangle); break;
                case WorldShapeKind.Diamond:
                    _diamond[0] = new PointF(cx, y); _diamond[1] = new PointF(x + w, cy); _diamond[2] = new PointF(cx, y + h); _diamond[3] = new PointF(x, cy);
                    g.FillPolygon(fill, _diamond); g.DrawPolygon(border, _diamond); break;
                case WorldShapeKind.Hexagon:
                    DrawPolygon(g, _hexagon, 6, cx, cy, Math.Min(w, h) * 0.5f, -30f, fill, border); break;
                case WorldShapeKind.Star:
                    DrawStar(g, cx, cy, Math.Min(w, h) * 0.5f, fill, border); break;
                case WorldShapeKind.Tree:
                    g.FillRectangle(DetailBrush, cx - Math.Max(2f, w * .11f), y + h * .56f, Math.Max(4f, w * .22f), h * .40f);
                    g.FillEllipse(fill, x + w * .07f, y + h * .03f, w * .86f, h * .70f);
                    g.DrawEllipse(border, x + w * .07f, y + h * .03f, w * .86f, h * .70f); break;
                case WorldShapeKind.House:
                    var body = new RectangleF(x + w * .12f, y + h * .36f, w * .76f, h * .52f);
                    g.FillRectangle(fill, body); g.DrawRectangle(border, body.X, body.Y, body.Width, body.Height);
                    _roof[0] = new PointF(x + w * .05f, y + h * .38f); _roof[1] = new PointF(cx, y + h * .04f); _roof[2] = new PointF(x + w * .95f, y + h * .38f);
                    g.FillPolygon(fill, _roof); g.DrawPolygon(border, _roof); break;
                case WorldShapeKind.Rock:
                    _rock[0] = new PointF(x + w*.10f,y+h*.75f); _rock[1] = new PointF(x+w*.22f,y+h*.34f); _rock[2] = new PointF(x+w*.52f,y+h*.12f); _rock[3] = new PointF(x+w*.84f,y+h*.30f); _rock[4] = new PointF(x+w*.94f,y+h*.72f); _rock[5] = new PointF(x+w*.67f,y+h*.92f); _rock[6] = new PointF(x+w*.30f,y+h*.91f);
                    g.FillPolygon(fill, _rock); g.DrawPolygon(border, _rock); break;
                case WorldShapeKind.Flower:
                    var r = Math.Min(w,h)*.22f;
                    for(int i=0;i<5;i++){var a=(float)(i*Math.PI*2/5);var px=cx+(float)Math.Cos(a)*r;var py=cy+(float)Math.Sin(a)*r;g.FillEllipse(fill,px-r,py-r,r*2,r*2);g.DrawEllipse(border,px-r,py-r,r*2,r*2);} g.FillEllipse(DetailBrush,cx-r*.45f,cy-r*.45f,r*.9f,r*.9f); break;
                case WorldShapeKind.Pillar:
                    g.FillRectangle(fill,x+w*.22f,y+h*.14f,w*.56f,h*.72f);g.DrawRectangle(border,x+w*.22f,y+h*.14f,w*.56f,h*.72f);g.FillRectangle(DetailBrush,x+w*.1f,y+h*.05f,w*.8f,Math.Max(2,h*.1f));g.FillRectangle(DetailBrush,x+w*.1f,y+h*.85f,w*.8f,Math.Max(2,h*.1f)); break;
                case WorldShapeKind.Cross:
                    g.FillRectangle(fill,cx-w*.16f,y,w*.32f,h);g.FillRectangle(fill,x,cy-h*.16f,w,h*.32f);g.DrawRectangle(border,cx-w*.16f,y,w*.32f,h);g.DrawRectangle(border,x,cy-h*.16f,w,h*.32f); break;
                default:
                    g.FillRectangle(fill, x, y, w, h); g.DrawRectangle(border, x, y, w, h); break;
            }
        }

        private static void DrawPolygon(Graphics g, PointF[] points, int count, float cx, float cy, float radius, float rotation, Brush fill, Pen border)
        {
            var offset = rotation * (float)Math.PI / 180f;
            for (int i=0;i<count;i++){var a=offset+i*(float)(Math.PI*2/count);points[i]=new PointF(cx+(float)Math.Cos(a)*radius,cy+(float)Math.Sin(a)*radius);}
            g.FillPolygon(fill,points); g.DrawPolygon(border,points);
        }

        private void DrawStar(Graphics g, float cx, float cy, float radius, Brush fill, Pen border)
        {
            for (int i=0;i<10;i++){var r=i%2==0?radius:radius*.43f;var a=(float)(-Math.PI/2+i*Math.PI/5);_star[i]=new PointF(cx+(float)Math.Cos(a)*r,cy+(float)Math.Sin(a)*r);}
            g.FillPolygon(fill,_star); g.DrawPolygon(border,_star);
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
            base.OnMouseWheel(e); if(_world==null)return;
            var before=ScreenToWorld(e.Location);_zoom=Clamp(_zoom*(e.Delta>0?1.12f:1f/1.12f),.2f,12f);var after=ScreenToWorld(e.Location);
            _pan.X+=(after.X-before.X)*GetScale();_pan.Y+=(after.Y-before.Y)*GetScale();Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e); Focus();
            if(Mode==CanvasMode.Build&&e.Button==MouseButtons.Left)
            {
                var worldPoint=ScreenToWorld(e.Location);var hit=_world.FindItemAt(new WorldPoint(worldPoint.X,worldPoint.Y));
                if(hit!=null){SelectItem(hit);_draggingItem=true;_dragOffset=new PointF((float)(hit.Position.X-worldPoint.X),(float)(hit.Position.Y-worldPoint.Y));Cursor=Cursors.SizeAll;return;}
                if(worldPoint.X>=0&&worldPoint.Y>=0&&worldPoint.X<_world.Width&&worldPoint.Y<_world.Height)
                {
                    var item=_world.AddItem(new WorldPoint(worldPoint.X,worldPoint.Y),BuildWidth,BuildHeight,BuildSolid);item.Kind=BuildKind;item.Name=BuildKind;item.Shape=BuildShape;SelectItem(item);WorldEdited?.Invoke(this,EventArgs.Empty);Invalidate();
                }
                else SelectItem(null);
                return;
            }
            if(Mode==CanvasMode.Build&&e.Button==MouseButtons.Right)
            {
                var worldPoint=ScreenToWorld(e.Location);var item=_world.FindItemAt(new WorldPoint(worldPoint.X,worldPoint.Y));
                if(item!=null){if(ReferenceEquals(item,_selectedItem))SelectItem(null);_world.RemoveItem(item.Id);WorldEdited?.Invoke(this,EventArgs.Empty);Invalidate();}
                return;
            }
            if((e.Button==MouseButtons.Middle||e.Button==MouseButtons.Right)&&Mode!=CanvasMode.Build){_panning=true;_lastMouse=e.Location;Cursor=Cursors.SizeAll;}
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if(_draggingItem&&Mode==CanvasMode.Build&&_selectedItem!=null)
            {
                var worldPoint=ScreenToWorld(e.Location);var next=new WorldPoint(worldPoint.X+_dragOffset.X,worldPoint.Y+_dragOffset.Y);
                var x=Math.Max(0.0,Math.Min(_world.Width-_selectedItem.Width,next.X));var y=Math.Max(0.0,Math.Min(_world.Height-_selectedItem.Height,next.Y));
                _selectedItem.Position=new WorldPoint(x,y);WorldEdited?.Invoke(this,EventArgs.Empty);Invalidate();return;
            }
            if(!_panning)return;_pan.X+=e.X-_lastMouse.X;_pan.Y+=e.Y-_lastMouse.Y;_lastMouse=e.Location;Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if(e.Button==MouseButtons.Left&&_draggingItem){_draggingItem=false;Cursor=Cursors.Default;return;}
            if(e.Button==MouseButtons.Middle||e.Button==MouseButtons.Right){_panning=false;Cursor=Cursors.Default;}
        }

        protected override bool IsInputKey(Keys keyData)
        {
            if(Mode==CanvasMode.Build&&(keyData==Keys.Delete||keyData==Keys.R||keyData==Keys.Q||keyData==Keys.E))return true;
            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e); if(Mode!=CanvasMode.Build||_selectedItem==null)return;
            if(e.KeyCode==Keys.Delete){DeleteSelectedItem();e.Handled=true;}
            else if(e.KeyCode==Keys.Q){_selectedItem.RotationDegrees-=15;WorldEdited?.Invoke(this,EventArgs.Empty);Invalidate();e.Handled=true;}
            else if(e.KeyCode==Keys.E||e.KeyCode==Keys.R){_selectedItem.RotationDegrees+=15;WorldEdited?.Invoke(this,EventArgs.Empty);Invalidate();e.Handled=true;}
        }

        private void SelectItem(WorldItem item)
        {
            if(ReferenceEquals(_selectedItem,item))return;_selectedItem=item;SelectionChanged?.Invoke(this,EventArgs.Empty);Invalidate();
        }

        private float GetScale(){return GetBaseScale()*_zoom;}
        private float GetBaseScale(){if(_world==null||_world.Width<=0||_world.Height<=0)return 1f;return Math.Max(.05f,Math.Min((ClientSize.Width-48f)/(float)_world.Width,(ClientSize.Height-48f)/(float)_world.Height));}
        private void FitWorld(){if(_world==null||ClientSize.Width<=0||ClientSize.Height<=0)return;_zoom=1f;var scale=GetBaseScale();var width=(float)_world.Width*scale;var height=(float)_world.Height*scale;_pan=new PointF((ClientSize.Width-48f-width)/2f,(ClientSize.Height-48f-height)/2f);}
        private void DrawGrid(Graphics g,float scale,PointF origin,RectangleF worldRect){var step=Math.Max(1,(int)Math.Ceiling(20f/Math.Max(scale,.01f)));var major=step*5;for(int x=0;x<=_world.Width;x+=step){var px=origin.X+x*scale;if(px>=worldRect.Left&&px<=worldRect.Right)g.DrawLine(x%major==0?MajorGridPen:GridPen,px,worldRect.Top,px,worldRect.Bottom);}for(int y=0;y<=_world.Height;y+=step){var py=origin.Y+y*scale;if(py>=worldRect.Top&&py<=worldRect.Bottom)g.DrawLine(y%major==0?MajorGridPen:GridPen,worldRect.Left,py,worldRect.Right,py);}}
        private PointF ScreenToWorld(Point point){var scale=GetScale();var origin=new PointF(24f+_pan.X,24f+_pan.Y);return new PointF((point.X-origin.X)/scale,(point.Y-origin.Y)/scale);}
        private static float Clamp(float value,float min,float max){return Math.Max(min,Math.Min(max,value));}
    }
}
