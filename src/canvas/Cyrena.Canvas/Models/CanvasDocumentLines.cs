using Cyrena.Models;

namespace Cyrena.Canvas.Models
{
    public class CanvasDocumentLines : ISuppressibleResult
    {
        public CanvasDocumentLines(CanvasDocument doc)
        {
            DocumentId = doc.Id;
            if (string.IsNullOrEmpty(doc.Content))
                Lines = new List<CanvasDocumentLine>();
            else
            {
                var lines = doc.Content.Split("\n");
                Lines = new List<CanvasDocumentLine>();
                for(int i = 0; i < lines.Length; i++)
                {
                    Lines.Add(new CanvasDocumentLine()
                    {
                        Index = i,
                        Text = lines[i]
                    });
                }
            }
        }
        public string DocumentId { get; set; }

        public List<CanvasDocumentLine> Lines { get; set; }

        public string Suppress()
        {
            return $"[CANVAS_DOCUMENT {DocumentId} omitted, lines={Lines.Count}, call Canvas_activate/Canvas_get_active for update]";
        }
    }

    public class CanvasDocumentLine
    {
        public int Index { get; set; }
        public string? Text { get; set; }
    }
}
