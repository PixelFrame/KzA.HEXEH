﻿using System.Text;

namespace KzA.HEXEH.Base.Output
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
        public List<string> Detail { get; set; } = [];
        public List<DataNode> Children { get; set; } = [];
        public int Index { get; set; } = -1;
        public int Length { get; set; } = -1;

        public DataNode() { }
        public DataNode(string Label, string Value, int Index, int Length)
        {
            this.Label = Label;
            this.Value = Value;
            this.Index = Index;
            this.Length = Length;
        }
        public DataNode(string Label, string Value, string DisplayValue, int Index, int Length)
        {
            this.Label = Label;
            this.Value = Value;
            this.DisplayValue = DisplayValue;
            this.Index = Index;
            this.Length = Length;
        }
        public DataNode(string Label, string Value, IEnumerable<string> Detail, int Index, int Length)
        {
            this.Label = Label;
            this.Value = Value;
            this.Detail.AddRange(Detail);
            this.Index = Index;
            this.Length = Length;
        }
        public DataNode(string Label, string Value, IEnumerable<string> Detail, IEnumerable<DataNode> Children, int Index, int Length)
        {
            this.Label = Label;
            this.Value = Value;
            this.Detail.AddRange(Detail);
            this.Children.AddRange(Children);
            this.Index = Index;
            this.Length = Length;
        }
        public DataNode(string Label, string Value, IEnumerable<DataNode> Children, int Index, int Length)
        {
            this.Label = Label;
            this.Value = Value;
            this.Children.AddRange(Children);
            this.Index = Index;
            this.Length = Length;
        }

        public override string ToString()
        {
            var sbResult = new StringBuilder();
            PrintNode("", true, true, false, true, sbResult);

            return sbResult.ToString();
        }

        private void PrintNode(string indent, bool root, bool last, bool isVerbose, bool printPos, StringBuilder sbResult)
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
            if (printPos) sbResult.Append($"[{Index}:{Length}]");
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
                Children[i].PrintNode(indent, false, i == Children.Count - 1, isVerbose, printPos, sbResult);
        }

        public string ToStringVerbose()
        {
            var sbResult = new StringBuilder();
            PrintNode("", true, true, true, true, sbResult);

            return sbResult.ToString();
        }
    }
}
