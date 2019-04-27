namespace LD44.Physics {
    public static class BodyPhysics {
        public static void Update(Body body, float delta) {
            body.Velocity += body.Acceleration * delta;

            float speed = body.Velocity.Length();
            if (speed > 20f) {
                body.Velocity = body.Velocity * 20f / speed;
            }

            body.Position += body.Velocity * delta;
        } 
    }
}
