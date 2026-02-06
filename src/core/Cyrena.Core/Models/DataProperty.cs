namespace Cyrena.Models
{
    public class DataProperty : Entity
    {
        public DataProperty() { }
        public DataProperty(string id, string? value = null)
        {
            Id = id;
            Value = value;
        }
        public string? Value { get; set; }
    }

    public class DataPropertyCollection
    {
        public List<DataProperty> Data { get; }
        public DataPropertyCollection()
        {
            Data = new List<DataProperty>();
        }

        public DataPropertyCollection(List<DataProperty> data)
        {
            Data = data;
        }

        public string? this[string id]
        {
            get
            {
                var i = Data.FirstOrDefault(x => x.Id == id);
                if (i == null)
                    return null;
                return i.Value;
            }
            set
            {
                var i = Data.FirstOrDefault(x => x.Id == id);
                if (i == null)
                    Data.Add(new DataProperty(id, value));
                else
                    i.Value = value;
            }
        }
    }
}
