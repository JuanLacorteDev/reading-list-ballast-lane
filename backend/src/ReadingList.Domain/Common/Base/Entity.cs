namespace ReadingList.Domain.Commom.Base
{
    public abstract class Entity
    {

        //protected set to allow only derived classes to set the properties
        public Guid Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }


        //protected constructor to prevent direct instantiation
        protected Entity()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
        }

        // Rehydration: identity comes from storage
        protected Entity(Guid id, DateTime createdAt, DateTime? updatedAt)
        {
            Id = id;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }



        protected void SetUpdatedAt()
        {
            UpdatedAt = DateTime.UtcNow;
        }


        //assert equality based on Id or Reference when id are empty
        public override bool Equals(object? obj)
        {
            if (obj is not Entity other) return false;
            if (ReferenceEquals(this, other)) return true;

            return Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator == (Entity left, Entity right)
        {
            if (ReferenceEquals(left, null))
                return ReferenceEquals(right, null);

            return left.Equals(right);
        }

        public static bool operator != (Entity left, Entity right)
        {
            return !(left == right);
        }
    }
}
