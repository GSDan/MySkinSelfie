namespace SkinSelfie.ServiceData.Models
{
    public abstract class Model
    {
        public int Id { get; set; }

        public override int GetHashCode()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }
    }
}
