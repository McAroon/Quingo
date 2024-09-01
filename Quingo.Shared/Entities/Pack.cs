﻿namespace Quingo.Shared.Entities
{
    public class Pack : EntityBase
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public List<Node> Nodes { get; } = [];

        public List<NodeLinkType> NodeLinkTypes { get; set; } = [];

        public List<Tag> Tags { get; set; } = [];

        public List<PackPreset> Presets { get; set; } = [];
    }
}
