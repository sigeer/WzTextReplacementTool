using MapleLib.WzLib;

namespace WinFormsApp1
{
    internal class PendingItems
    {
        public PendingItems(PendingType type, WzImageProperty nodeProperty)
        {
            Type = type;
            Node = nodeProperty;
            SubProps = [];
        }

        public PendingType Type { get; }
        public WzImageProperty Node { get; }

        public HashSet<WzImageProperty> SubProps { get; }
        public bool Processed { get; set; }
    }

    internal enum PendingType
    {
        NewNode,
        PropertyChanged,
    }
}
