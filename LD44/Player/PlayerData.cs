namespace LD44.Player {
    public sealed class PlayerData {
        public bool Prologue { get; set; } = true;
        public float Lifespan { get; set; } = 60f;
        public int Armor { get; set; } = 0;
        public bool HasCoupon { get; set; } = false;
        public int Damage { get; set; } = 4;
        public string Sword { get; set; } = "sword";
        public bool Cursed { get; set; } = false;
        public bool Dash { get; set; } = false;
        public bool DoubleJump { get; set; } = false;
        public float Speed { get; set; } = 6.5f;
        public bool Steal { get; set; } = false;
        public bool Jetpack { get; set; } = false;
    }
}
