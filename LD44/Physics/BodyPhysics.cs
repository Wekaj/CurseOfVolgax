namespace LD44.Physics {
    public static class BodyPhysics {
        public static void Update(Body body, float delta) {
            body.Velocity += body.Acceleration * delta;

            float speed = body.Velocity.Length();
            if (speed > 17.5f) {
                body.Velocity = body.Velocity * 17.5f / speed;
            }

            body.Position += body.Velocity * delta;
        } 
    }
}
