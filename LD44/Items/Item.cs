using System;

namespace LD44.Items {
    public sealed class Item {
        public Item(string name, int cost) {
            Name = name;
            Cost = cost;
        }

        public Item(Item item, Random random) {
            Name = item.Name;
            Cost = Math.Max(item.Cost - 6 + random.Next(12), 1);
        }

        public string Name { get; set; }
        public int Cost { get; set; }
    }
}
