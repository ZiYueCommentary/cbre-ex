﻿using CBRE.Common;
using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Brushes.Controls;
using CBRE.Extensions;
using CBRE.Localization;
using System.Collections.Generic;
using System.Linq;

namespace CBRE.Editor.Brushes
{
    public class CylinderBrush : IBrush
    {
        private readonly NumericControl _numSides;

        public CylinderBrush()
        {
            _numSides = new NumericControl(this) { LabelText = Local.LocalString("brush.sides") };
        }

        public string Name
        {
            get { return Local.LocalString("brush.cylinder"); }
        }

        public bool CanRound { get { return true; } }

        public IEnumerable<BrushControl> GetControls()
        {
            yield return _numSides;
        }

        public IEnumerable<MapObject> Create(IDGenerator generator, Box box, ITexture texture, int roundDecimals)
        {
            int numSides = (int)_numSides.GetValue();
            if (numSides < 3) yield break;

            // Cylinders can be elliptical so use both major and minor rather than just the radius
            // NOTE: when a low number (< 10ish) of faces are selected this will cause the cylinder to not touch all the edges of the box.
            decimal width = box.Width;
            decimal length = box.Length;
            decimal height = box.Height;
            decimal major = width / 2;
            decimal minor = length / 2;
            decimal angle = 2 * DMath.PI / numSides;

            // Calculate the X and Y points for the ellipse
            Coordinate[] points = new Coordinate[numSides];
            for (int i = 0; i < numSides; i++)
            {
                decimal a = i * angle;
                decimal xval = box.Center.X + major * DMath.Cos(a);
                decimal yval = box.Center.Y + minor * DMath.Sin(a);
                decimal zval = box.Start.Z;
                points[i] = new Coordinate(xval, yval, zval).Round(roundDecimals);
            }

            List<Coordinate[]> faces = new List<Coordinate[]>();

            // Add the vertical faces
            Coordinate z = new Coordinate(0, 0, height).Round(roundDecimals);
            for (int i = 0; i < numSides; i++)
            {
                int next = (i + 1) % numSides;
                faces.Add(new[] { points[i], points[i] + z, points[next] + z, points[next] });
            }
            // Add the elliptical top and bottom faces
            faces.Add(points.ToArray());
            faces.Add(points.Select(x => x + z).Reverse().ToArray());

            // Nothing new here, move along
            Solid solid = new Solid(generator.GetNextObjectID()) { Colour = Colour.GetRandomBrushColour() };
            foreach (Coordinate[] arr in faces)
            {
                Face face = new Face(generator.GetNextFaceID())
                {
                    Parent = solid,
                    Plane = new Plane(arr[0], arr[1], arr[2]),
                    Colour = solid.Colour,
                    Texture = { Texture = texture }
                };
                face.Vertices.AddRange(arr.Select(x => new Vertex(x, face)));
                face.UpdateBoundingBox();
                face.AlignTextureToFace();
                solid.Faces.Add(face);
            }
            solid.UpdateBoundingBox();
            yield return solid;
        }
    }
}
