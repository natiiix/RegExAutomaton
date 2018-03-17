using System;
using Visio = Microsoft.Office.Interop.Visio;
using VisioAutomation.Models.Layouts.DirectedGraph;
using VisioAutomation.Shapes;
using VisioAutomation.Geometry;
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
                string stateInfo = $"State {i}";

                if (i == regex.StartingState)
                {
                    stateInfo += Environment.NewLine + "[START]";
                }

                if (regex.States[i].Ending)
                {
                    stateInfo += Environment.NewLine + "[END]";
                }

                shapes[i] = d.AddShape(
                    $"s{i}",
                    stateInfo,
                    basic_stencil,
                    "Rectangle"
                );
            }

            for (int i = 0, len = regex.Edges.Count; i < len; i++)
            {
                Edge edge = regex.Edges[i];

                const ConnectorType type = ConnectorType.RightAngle;
                const int beginArrow = 20;
                const int endArrow = 5;

                if (string.IsNullOrEmpty(edge.Value))
                {
                    d.AddConnection(
                        $"e{i}",
                        shapes[edge.Origin],
                        shapes[edge.Destination],
                        string.Empty,
                        type,
                        beginArrow,
                        endArrow,
                        string.Empty
                    );
                }
                else
                {
                    string edgeValue = $"\"{edge.Value}\"" + Environment.NewLine + $"[{string.Join(",", edge.CaptureGroups)}]";

                    Shape edgeShape = d.AddShape($"e{i}", edgeValue, basic_stencil, "Diamond");
                    edgeShape.Size = new Size(2.5, 1.75);

                    d.AddConnection($"e{i}_1", shapes[edge.Origin], edgeShape, string.Empty, type, beginArrow, endArrow, string.Empty);
                    d.AddConnection($"e{i}_2", edgeShape, shapes[edge.Destination], string.Empty, type, beginArrow, endArrow, string.Empty);
                }

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
                cell.FormulaU = "20 pt";
            }
        }
    }
}
