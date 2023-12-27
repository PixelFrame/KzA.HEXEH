using System.Text;

namespace KzA.HEXEH.Core.Output
{
    public class DataNode
    {
        public string Label { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        private string? displayValue;
        public string DisplayValue
        {
            get { return displayValue ?? Value; }
            set { displayValue = value; }
        }
        public List<string> Detail { get; set; } = new List<string>();
        public List<DataNode> Children { get; set; } = new List<DataNode>();

        public DataNode() { }
        public DataNode(string Label, string Value)
        {
            this.Label = Label;
            this.Value = Value;
        }
        public DataNode(string Label, string Value, string DisplayValue)
        {
            this.Label = Label;
            this.Value = Value;
            this.DisplayValue = DisplayValue;
        }
        public DataNode(string Label, string Value, IEnumerable<string> Detail)
        {
            this.Label = Label;
            this.Value = Value;
            this.Detail.AddRange(Detail);
        }
        public DataNode(string Label, string Value, IEnumerable<string> Detail, IEnumerable<DataNode> Children)
        {
            this.Label = Label;
            this.Value = Value;
            this.Detail.AddRange(Detail);
            this.Children.AddRange(Children);
        }
        public DataNode(string Label, string Value, IEnumerable<DataNode> Children)
        {
            this.Label = Label;
            this.Value = Value;
            this.Children.AddRange(Children);
        }

        public override string ToString()
        {
            var sbResult = new StringBuilder();
            PrintNode("", true, true, false, ref sbResult);

            return sbResult.ToString();
        }

        private void PrintNode(string indent, bool root, bool last, bool isVerbose, ref StringBuilder sbResult)
        {
            sbResult.Append(indent);
            if (root) { }
            else if (last)
            {
                sbResult.Append(@"└─");
                indent += "  ";
            }
            else
            {
                sbResult.Append(@"├─");
                indent += "│ ";
            }
            sbResult.AppendLine($"{Label}: {DisplayValue}");
            if (isVerbose && Detail.Count > 0)
            {
                var addExtraIndent = Children.Count > 0;
                foreach (var detailLine in Detail)
                {
                    sbResult.Append(indent);
                    if (addExtraIndent) sbResult.Append("│ ");
                    else sbResult.Append("  ");
                    sbResult.Append("  ");
                    sbResult.AppendLine(detailLine);
                }
            }
            for (int i = 0; i < Children.Count; i++)
                Children[i].PrintNode(indent, false, i == Children.Count - 1, isVerbose, ref sbResult);
        }
        public string ToStringVerbose()
        {
            var sbResult = new StringBuilder();
            PrintNode("", true, true, true, ref sbResult);

            return sbResult.ToString();
        }
    }
}
