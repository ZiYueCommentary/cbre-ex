﻿using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Actions.MapObjects.Operations;
using CBRE.Editor.Properties;
using CBRE.Graphics;
using CBRE.Graphics.Helpers;
using CBRE.Localization;
using CBRE.Settings;
using CBRE.UI;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Matrix = CBRE.Graphics.Helpers.Matrix;

namespace CBRE.Editor.Tools
{
    public class ClipTool : BaseTool
    {
        public enum ClipState
        {
            None,
            Drawing,
            Drawn,
            MovingPoint1,
            MovingPoint2,
            MovingPoint3
        }

        public enum ClipSide
        {
            Both,
            Front,
            Back
        }

        private Coordinate _clipPlanePoint1;
        private Coordinate _clipPlanePoint2;
        private Coordinate _clipPlanePoint3;
        private Coordinate _drawingPoint;
        private ClipState _prevState;
        private ClipState _state;
        private ClipSide _side;

        public ClipTool()
        {
            Usage = ToolUsage.Both;
            _clipPlanePoint1 = _clipPlanePoint2 = _clipPlanePoint3 = _drawingPoint = null;
            _state = _prevState = ClipState.None;
            _side = ClipSide.Both;
        }

        public override Image GetIcon()
        {
            return Resources.Tool_Clip;
        }

        public override string GetName()
        {
            return Local.LocalString("tool.clip");
        }

        public override HotkeyTool? GetHotkeyToolType()
        {
            return HotkeyTool.Clip;
        }

        public override string GetContextualHelp()
        {
            return Local.LocalString("tool.clip.help");
        }

        private ClipState GetStateAtPoint(int x, int y, Viewport2D viewport)
        {
            if (_clipPlanePoint1 == null || _clipPlanePoint2 == null || _clipPlanePoint3 == null) return ClipState.None;

            Coordinate p = viewport.ScreenToWorld(x, y);
            Coordinate p1 = viewport.Flatten(_clipPlanePoint1);
            Coordinate p2 = viewport.Flatten(_clipPlanePoint2);
            Coordinate p3 = viewport.Flatten(_clipPlanePoint3);

            decimal d = 5 / viewport.Zoom;

            if (p.X >= p1.X - d && p.X <= p1.X + d && p.Y >= p1.Y - d && p.Y <= p1.Y + d) return ClipState.MovingPoint1;
            if (p.X >= p2.X - d && p.X <= p2.X + d && p.Y >= p2.Y - d && p.Y <= p2.Y + d) return ClipState.MovingPoint2;
            if (p.X >= p3.X - d && p.X <= p3.X + d && p.Y >= p3.Y - d && p.Y <= p3.Y + d) return ClipState.MovingPoint3;

            return ClipState.None;
        }

        public override void MouseDown(ViewportBase vp, ViewportEvent e)
        {
            if (!(vp is Viewport2D)) return;
            Viewport2D viewport = (Viewport2D)vp;
            _prevState = _state;

            Coordinate point = SnapIfNeeded(viewport.Expand(viewport.ScreenToWorld(e.X, viewport.Height - e.Y)));
            ClipState st = GetStateAtPoint(e.X, viewport.Height - e.Y, viewport);
            if (_state == ClipState.None || st == ClipState.None)
            {
                _state = ClipState.Drawing;
                _drawingPoint = point;
            }
            else if (_state == ClipState.Drawn)
            {
                _state = st;
            }
        }

        public override void MouseClick(ViewportBase viewport, ViewportEvent e)
        {
            // Not used
        }

        public override void MouseDoubleClick(ViewportBase viewport, ViewportEvent e)
        {
            // Not used
        }

        public override void MouseUp(ViewportBase vp, ViewportEvent e)
        {
            if (!(vp is Viewport2D)) return;
            Viewport2D viewport = (Viewport2D)vp;

            Coordinate point = SnapIfNeeded(viewport.Expand(viewport.ScreenToWorld(e.X, viewport.Height - e.Y)));
            if (_state == ClipState.Drawing)
            {
                // Do nothing
                _state = _prevState;
            }
            else
            {
                _state = ClipState.Drawn;
            }

            Editor.Instance.CaptureAltPresses = false;
        }

        public override void MouseMove(ViewportBase vp, ViewportEvent e)
        {
            if (!(vp is Viewport2D)) return;
            Viewport2D viewport = (Viewport2D)vp;

            Coordinate point = SnapIfNeeded(viewport.Expand(viewport.ScreenToWorld(e.X, viewport.Height - e.Y)));
            ClipState st = GetStateAtPoint(e.X, viewport.Height - e.Y, viewport);
            if (_state == ClipState.Drawing)
            {
                _state = ClipState.MovingPoint2;
                _clipPlanePoint1 = _drawingPoint;
                _clipPlanePoint2 = point;
                _clipPlanePoint3 = _clipPlanePoint1 + SnapIfNeeded(viewport.GetUnusedCoordinate(new Coordinate(128, 128, 128)));
            }
            else if (_state == ClipState.MovingPoint1)
            {
                // Move point 1
                Coordinate cp1 = viewport.GetUnusedCoordinate(_clipPlanePoint1) + point;
                if (KeyboardState.Ctrl)
                {
                    Coordinate diff = _clipPlanePoint1 - cp1;
                    _clipPlanePoint2 -= diff;
                    _clipPlanePoint3 -= diff;
                }
                _clipPlanePoint1 = cp1;
            }
            else if (_state == ClipState.MovingPoint2)
            {
                // Move point 2
                Coordinate cp2 = viewport.GetUnusedCoordinate(_clipPlanePoint2) + point;
                if (KeyboardState.Ctrl)
                {
                    Coordinate diff = _clipPlanePoint2 - cp2;
                    _clipPlanePoint1 -= diff;
                    _clipPlanePoint3 -= diff;
                }
                _clipPlanePoint2 = cp2;
            }
            else if (_state == ClipState.MovingPoint3)
            {
                // Move point 3
                Coordinate cp3 = viewport.GetUnusedCoordinate(_clipPlanePoint3) + point;
                if (KeyboardState.Ctrl)
                {
                    Coordinate diff = _clipPlanePoint3 - cp3;
                    _clipPlanePoint1 -= diff;
                    _clipPlanePoint2 -= diff;
                }
                _clipPlanePoint3 = cp3;
            }

            Editor.Instance.CaptureAltPresses = _state != ClipState.None && _state != ClipState.Drawn;

            if (st != ClipState.None || (_state != ClipState.None && _state != ClipState.Drawn))
            {
                viewport.Cursor = Cursors.Cross;
            }
            else
            {
                viewport.Cursor = Cursors.Default;
            }
        }

        public override void KeyPress(ViewportBase viewport, ViewportEvent e)
        {
            if (e.KeyChar == 13) // Enter
            {
                if (!_clipPlanePoint1.EquivalentTo(_clipPlanePoint2)
                    && !_clipPlanePoint2.EquivalentTo(_clipPlanePoint3)
                    && !_clipPlanePoint1.EquivalentTo(_clipPlanePoint3)) // Don't clip if the points are too close together
                {
                    PerformClip();
                }
            }
            if (e.KeyChar == 27 || e.KeyChar == 13) // Escape cancels, Enter commits and resets
            {
                _clipPlanePoint1 = _clipPlanePoint2 = _clipPlanePoint3 = _drawingPoint = null;
                _state = _prevState = ClipState.None;
            }
        }

        private void PerformClip()
        {
            List<Solid> objects = Document.Selection.GetSelectedObjects().OfType<Solid>().ToList();
            Plane plane = new Plane(_clipPlanePoint1, _clipPlanePoint2, _clipPlanePoint3);
            Document.PerformAction(Local.LocalString("tool.clip.perform"), new Clip(objects, plane, _side != ClipSide.Back, _side != ClipSide.Front));
        }

        public override void Render(ViewportBase viewport)
        {
            if (viewport is Viewport2D) Render2D((Viewport2D)viewport);
            if (viewport is Viewport3D) Render3D((Viewport3D)viewport);
        }

        public override HotkeyInterceptResult InterceptHotkey(HotkeysMediator hotkeyMessage, object parameters)
        {
            switch (hotkeyMessage)
            {
                case HotkeysMediator.OperationsPasteSpecial:
                case HotkeysMediator.OperationsPaste:
                    return HotkeyInterceptResult.SwitchToSelectTool;
                case HotkeysMediator.SwitchTool:
                    if (parameters is HotkeyTool && (HotkeyTool)parameters == GetHotkeyToolType())
                    {
                        CycleClipSide();
                        return HotkeyInterceptResult.Abort;
                    }
                    break;
            }
            return HotkeyInterceptResult.Continue;
        }

        private void CycleClipSide()
        {
            int side = (int)_side;
            side = (side + 1) % (Enum.GetValues(typeof(ClipSide)).Length);
            _side = (ClipSide)side;
        }

        private void Render2D(Viewport2D vp)
        {
            if (_state == ClipState.None
                || _clipPlanePoint1 == null
                || _clipPlanePoint2 == null
                || _clipPlanePoint3 == null) return; // Nothing to draw at this point

            double z = (double)vp.Zoom;
            Coordinate p1 = vp.Flatten(_clipPlanePoint1);
            Coordinate p2 = vp.Flatten(_clipPlanePoint2);
            Coordinate p3 = vp.Flatten(_clipPlanePoint3);
            // Draw points
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.White);
            GLX.Square(new Vector2d(p1.DX, p1.DY), 4, z, true);
            GLX.Square(new Vector2d(p2.DX, p2.DY), 4, z, true);
            GLX.Square(new Vector2d(p3.DX, p3.DY), 4, z, true);
            GL.End();

            GL.Enable(EnableCap.LineSmooth);
            GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);

            // Draw lines between points and point outlines
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.White);
            GL.Vertex2(p1.DX, p1.DY);
            GL.Vertex2(p2.DX, p2.DY);
            GL.Vertex2(p2.DX, p2.DY);
            GL.Vertex2(p3.DX, p3.DY);
            GL.Vertex2(p3.DX, p3.DY);
            GL.Vertex2(p1.DX, p1.DY);
            GL.Color3(Color.Black);
            GLX.Square(new Vector2d(p1.DX, p1.DY), 4, z);
            GLX.Square(new Vector2d(p2.DX, p2.DY), 4, z);
            GLX.Square(new Vector2d(p3.DX, p3.DY), 4, z);
            GL.End();

            // Draw the clipped brushes
            if (!_clipPlanePoint1.EquivalentTo(_clipPlanePoint2)
                    && !_clipPlanePoint2.EquivalentTo(_clipPlanePoint3)
                    && !_clipPlanePoint1.EquivalentTo(_clipPlanePoint3))
            {
                Plane plane = new Plane(_clipPlanePoint1, _clipPlanePoint2, _clipPlanePoint3);
                List<Face> faces = new List<Face>();
                IDGenerator idg = new IDGenerator();
                foreach (Solid solid in Document.Selection.GetSelectedObjects().OfType<Solid>().ToList())
                {
                    Solid back, front;
                    if (solid.Split(plane, out back, out front, idg))
                    {
                        if (_side != ClipSide.Front) faces.AddRange(back.Faces);
                        if (_side != ClipSide.Back) faces.AddRange(front.Faces);
                    }
                }
                GL.LineWidth(2);
                GL.Color3(Color.White);
                Matrix.Push();
                Matrix4 mat = vp.GetModelViewMatrix();
                GL.MultMatrix(ref mat);
                Rendering.Immediate.MapObjectRenderer.DrawWireframe(faces, true, false);
                Matrix.Pop();
                GL.LineWidth(1);
            }

            GL.Hint(HintTarget.LineSmoothHint, HintMode.Fastest);
            GL.Disable(EnableCap.LineSmooth);

        }

        private void Render3D(Viewport3D vp)
        {
            if (_state == ClipState.None
                || _clipPlanePoint1 == null
                || _clipPlanePoint2 == null
                || _clipPlanePoint3 == null
                || Document.Selection.IsEmpty()) return; // Nothing to draw at this point

            TextureHelper.Unbind();

            // Draw points

            if (!_clipPlanePoint1.EquivalentTo(_clipPlanePoint2)
                    && !_clipPlanePoint2.EquivalentTo(_clipPlanePoint3)
                    && !_clipPlanePoint1.EquivalentTo(_clipPlanePoint3))
            {
                Plane plane = new Plane(_clipPlanePoint1, _clipPlanePoint2, _clipPlanePoint3);

                // Draw clipped solids
                GL.Enable(EnableCap.LineSmooth);
                GL.Hint(HintTarget.LineSmoothHint, HintMode.Nicest);

                List<Face> faces = new List<Face>();
                IDGenerator idg = new IDGenerator();
                foreach (Solid solid in Document.Selection.GetSelectedObjects().OfType<Solid>().ToList())
                {
                    Solid back, front;
                    if (solid.Split(plane, out back, out front, idg))
                    {
                        if (_side != ClipSide.Front) faces.AddRange(back.Faces);
                        if (_side != ClipSide.Back) faces.AddRange(front.Faces);
                    }
                }
                GL.LineWidth(2);
                GL.Color3(Color.White);
                Rendering.Immediate.MapObjectRenderer.DrawWireframe(faces, true, false);
                GL.LineWidth(1);

                GL.Hint(HintTarget.LineSmoothHint, HintMode.Fastest);
                GL.Disable(EnableCap.LineSmooth);

                // Draw the clipping plane
                Polygon poly = new Polygon(plane);
                Box bbox = Document.Selection.GetSelectionBoundingBox();
                Coordinate point = bbox.Center;
                foreach (Plane boxPlane in bbox.GetBoxPlanes())
                {
                    Coordinate proj = boxPlane.Project(point);
                    decimal dist = (point - proj).VectorMagnitude() * 0.1m;
                    poly.Split(new Plane(boxPlane.Normal, proj + boxPlane.Normal * Math.Max(dist, 100)));
                }

                GL.Disable(EnableCap.CullFace);
                GL.Begin(PrimitiveType.Polygon);
                GL.Color4(Color.FromArgb(100, Color.Turquoise));
                foreach (Coordinate c in poly.Vertices) GL.Vertex3(c.DX, c.DY, c.DZ);
                GL.End();
                GL.Enable(EnableCap.CullFace);
            }
        }

        public override void MouseEnter(ViewportBase viewport, ViewportEvent e)
        {
            //
        }

        public override void MouseLeave(ViewportBase viewport, ViewportEvent e)
        {
            //
        }

        public override void MouseWheel(ViewportBase viewport, ViewportEvent e)
        {
            //
        }

        public override void KeyDown(ViewportBase viewport, ViewportEvent e)
        {
            //
        }

        public override void KeyUp(ViewportBase viewport, ViewportEvent e)
        {
            //
        }

        public override void UpdateFrame(ViewportBase viewport, FrameInfo frame)
        {
            //
        }
    }
}
