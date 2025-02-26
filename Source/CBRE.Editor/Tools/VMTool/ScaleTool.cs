﻿using CBRE.Common.Mediator;
using CBRE.DataStructures.Geometric;
using CBRE.Graphics;
using CBRE.Localization;
using CBRE.UI;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Vertex = CBRE.DataStructures.MapObjects.Vertex;

namespace CBRE.Editor.Tools.VMTool
{
    public class ScaleTool : VMSubTool
    {
        private enum VMState
        {
            None,
            Down,
            Moving
        }

        private VMState _state;
        private VMPoint _origin;
        private decimal _prevValue;
        private Dictionary<VMPoint, Coordinate> _originals;

        public ScaleTool(VMTool mainTool) : base(mainTool)
        {
            ScaleControl sc = new ScaleControl();
            Control = sc;
            sc.ValueChanged += ValueChanged;
            sc.ValueReset += ValueReset;
            sc.ResetOrigin += ResetOrigin;
            _origin = new VMPoint { Coordinate = Coordinate.Zero, Vertices = new List<Vertex>() };
        }

        private void ResetOrigin(object sender)
        {
            List<Coordinate> points = MainTool.GetSelectedPoints().Select(x => x.Coordinate).ToList();
            if (!points.Any()) points = MainTool.Points.Where(x => !x.IsMidPoint).Select(x => x.Coordinate).ToList();
            if (!points.Any()) _origin.Coordinate = Coordinate.Zero;
            else _origin.Coordinate = points.Aggregate(Coordinate.Zero, (a, b) => a + b) / points.Count;
        }

        private void ValueChanged(object sender, decimal value)
        {
            MovePoints(value);
            _prevValue = value;
        }

        private void ValueReset(object sender, decimal value)
        {
            _prevValue = value;
            _originals = MainTool.Points.ToDictionary(x => x, x => x.Coordinate);
        }

        public override void SelectionChanged()
        {
            ((ScaleControl)Control).ResetValue();
            if (MainTool.GetSelectedPoints().Any()) ResetOrigin(null);
        }

        public override bool ShouldDeselect(List<VMPoint> vtxs)
        {
            return !vtxs.Contains(_origin);
        }

        public override bool NoSelection()
        {
            return false;
        }

        public override bool No3DSelection()
        {
            return true;
        }

        public override bool DrawVertices()
        {
            return true;
        }

        private void MovePoints(decimal value)
        {
            Coordinate o = _origin.Coordinate;
            // Move each selected point by the computed offset from the origin
            foreach (VMPoint p in MainTool.GetSelectedPoints())
            {
                Coordinate orig = _originals[p];
                Coordinate diff = orig - o;
                Coordinate move = o + diff * value / 100;
                p.Move(move - p.Coordinate);
            }
            MainTool.SetDirty(false, true);
        }

        public override string GetName()
        {
            return Local.LocalString("data.model.scale");
        }

        public override string GetContextualHelp()
        {
            return Local.LocalString("tool.scale_distance.help");
        }

        public override void ToolSelected(bool preventHistory)
        {
            _state = VMState.None;
            _originals = MainTool.Points.ToDictionary(x => x, x => x.Coordinate);
            ResetOrigin(null);
        }

        public override void ToolDeselected(bool preventHistory)
        {
            _state = VMState.None;
            _originals = null;
            Mediator.UnsubscribeAll(this);
        }

        public override List<VMPoint> GetVerticesAtPoint(int x, int y, Viewport2D viewport)
        {
            List<VMPoint> verts = MainTool.GetVerticesAtPoint(x, y, viewport);

            Coordinate p = viewport.ScreenToWorld(x, y);
            decimal d = 8 / viewport.Zoom; // Tolerance value = 8 pixels
            Coordinate c = viewport.Flatten(_origin.Coordinate);
            if (p.X >= c.X - d && p.X <= c.X + d && p.Y >= c.Y - d && p.Y <= c.Y + d)
            {
                verts.Insert(0, _origin);
            }

            return verts;
        }

        public override List<VMPoint> GetVerticesAtPoint(int x, int y, Viewport3D viewport)
        {
            return MainTool.GetVerticesAtPoint(x, y, viewport);
        }

        public override void DragStart(List<VMPoint> clickedPoints)
        {
            if (!clickedPoints.Contains(_origin)) return;
            _state = VMState.Down;
            Editor.Instance.CaptureAltPresses = true;
        }

        public override void DragMove(Coordinate distance)
        {
            if (_state == VMState.None) return;
            _state = VMState.Moving;
            // Move the origin point by the delta value
            _origin.Move(distance);
        }

        public override void DragEnd()
        {
            _state = VMState.None;
            Editor.Instance.CaptureAltPresses = false;
        }

        public override void MouseClick(ViewportBase viewport, ViewportEvent e)
        {

        }

        public override void MouseEnter(ViewportBase viewport, ViewportEvent e)
        {

        }

        public override void MouseLeave(ViewportBase viewport, ViewportEvent e)
        {

        }

        public override void MouseDown(ViewportBase viewport, ViewportEvent e)
        {

        }

        public override void MouseDoubleClick(ViewportBase viewport, ViewportEvent e)
        {
            // Not used
        }

        public override void MouseUp(ViewportBase viewport, ViewportEvent e)
        {

        }

        public override void MouseWheel(ViewportBase viewport, ViewportEvent e)
        {

        }

        public override void MouseMove(ViewportBase viewport, ViewportEvent e)
        {

        }

        public override void KeyPress(ViewportBase viewport, ViewportEvent e)
        {

        }

        public override void KeyDown(ViewportBase viewport, ViewportEvent e)
        {
            Coordinate nudge = GetNudgeValue(e.KeyCode);
            Viewport2D vp = viewport as Viewport2D;
            if (nudge != null && vp != null && _state == VMState.None)
            {
                Coordinate translate = vp.Expand(nudge);
                _origin.Move(translate);
            }
        }

        public override void KeyUp(ViewportBase viewport, ViewportEvent e)
        {

        }

        public override void UpdateFrame(ViewportBase viewport, FrameInfo frame)
        {

        }

        public override void Render(ViewportBase viewport)
        {

        }

        public override void Render2D(Viewport2D viewport)
        {
            Coordinate pos = viewport.Flatten(_origin.Coordinate);

            GL.Color3(Color.Cyan);
            GL.Begin(PrimitiveType.Lines);
            GLX.Circle(new Vector2d(pos.DX, pos.DY), 8, (double)viewport.Zoom);
            GL.End();
            GL.Begin(PrimitiveType.Points);
            GL.Vertex2(pos.DX, pos.DY);
            GL.End();
        }

        public override void Render3D(Viewport3D viewport)
        {

        }
    }
}
