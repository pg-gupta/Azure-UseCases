using System;
namespace SimpleEchoBot.Models
{
    [Serializable]
    public class Medicine
    {
        public string id;
        public string name;
        public string description;
        public Medicine()
        {
        }
        public Medicine(string id, string name, string description)
        {
            this.id = id;
            this.name = name;
            this.description = description;
        }

    }
}
