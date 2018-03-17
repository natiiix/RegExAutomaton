﻿using System;
using Visio = Microsoft.Office.Interop.Visio;
using VisioAutomation.Models.Layouts.DirectedGraph;
using VisioAutomation.Shapes;
using RegExAutomaton;

namespace RegExVisioDiagram
{
    public class Program
    {
        public static void Main(string[] args)
        {
            RenderVisioDiagram(new RegEx("^x (a*b|(cd|ef)*|hello) y$"));
        }

        public static void RenderVisioDiagram(RegEx regex)
        {
            // Start Visio
            Visio.Application app = new Visio.Application();

            // Create a new document.
            Visio.Document doc = app.Documents.Add(string.Empty);

            // The new document will have one page,
            // get the a reference to it.
            Visio.Page page1 = doc.Pages[1];

            // Name the page. This is want is shown in the page tabs.
            page1.Name = "Diagram";

            DirectedGraphLayout d = new DirectedGraphLayout();

            string basic_stencil = "basic_u.vss";

            Shape[] shapes = new Shape[regex.States.Count];

            for (int i = 0, len = regex.States.Count; i < len; i++)
            {
                shapes[i] = d.AddShape(
                    $"s{i}",
                    $"State {i}" + (i == regex.StartingState ? Environment.NewLine + "[START]" : regex.States[i].Ending ? Environment.NewLine + "[END]" : string.Empty),
                    basic_stencil,
                    "Rectangle"
                );
            }

            for (int i = 0, len = regex.Edges.Count; i < len; i++)
            {
                Edge edge = regex.Edges[i];
                string edgeValue = string.IsNullOrEmpty(edge.Value) ? string.Empty : $"\"{edge.Value}\"";

                Shape edgeShape = d.AddShape($"e{i}", edgeValue, basic_stencil, "Diamond");

                const ConnectorType type = ConnectorType.RightAngle;
                const int beginArrow = 0;
                const int endArrow = 5;

                d.AddConnection($"e{i}_1", shapes[edge.Origin], edgeShape, string.Empty, type, beginArrow, endArrow, string.Empty);
                d.AddConnection($"e{i}_2", edgeShape, shapes[edge.Destination], string.Empty, type, beginArrow, endArrow, string.Empty);

                //d.AddConnection(
                //    $"e{i}",
                //    shapes[edge.Origin],
                //    shapes[edge.Destination],
                //    edgeValue,
                //    ConnectorType.RightAngle
                //);
            }

            MsaglLayoutOptions options = new MsaglLayoutOptions();

            d.Render(page1, options);

            foreach (Visio.Shape shape in page1.Shapes)
            {
                Visio.Cell cell = shape.CellsU["Char.Size"];
                cell.FormulaU = "24 pt";
            }
        }
    }
}
