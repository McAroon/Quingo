namespace Quingo.Shared.Entities
{
    public class Node : EntityBase, IHasMeta
    {
        public int PackId { get; set; }

        public string? Name { get; set; }

        public string? ImageUrl { get; set; }

        public List<NodeLink> NodeLinksFrom { get; } = [];

        public List<NodeLink> NodeLinksTo { get; } = [];

        public List<NodeTag> NodeTags { get; } = [];

        public required Pack Pack { get; set; }

        public List<NodeLink> NodeLinks => [.. NodeLinksFrom, .. NodeLinksTo];

        public List<Node> LinkedNodes => NodeLinks.Select(x => x.NodeFrom).Concat(NodeLinks.Select(x => x.NodeTo)).Where(x => x != this).ToList();

        public IEnumerable<Tag> Tags => NodeTags.Select(x => x.Tag);

        public Meta Meta { get; set; } = new();

        public bool HasTag(string tag) => Tags.FirstOrDefault(x => x.Name == tag) != null;
        public bool HasTag(int id) => Tags.FirstOrDefault(x => x.Id == id) != null;
    }

    public class Meta
    {
        public List<MetaProperty> Properties { get; set; } = [];

        public Dictionary<string, string> PropertiesDict 
        { 
            get 
            { 
                return Properties.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            } 
            set 
            { 
                foreach (var kv in value)
                {
                    var prop = Properties.FirstOrDefault(x => x.Key == kv.Key);
                    if (prop == null)
                    {
                        prop = new MetaProperty { Key = kv.Key, Value = kv.Value };
                        Properties.Add(prop);
                    }
                    else
                    {
                        prop.Value = kv.Value;
                    }
                }
            } 
        }

        public class MetaProperty
        {
            public string Key { get; set; } = default!;
            public string Value { get; set; } = default!;
        }
    }
}
