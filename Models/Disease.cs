using System;
namespace SimpleEchoBot.Models
{
    [Serializable]
    public class Disease
    {
        public string id;
        public string name;
        public string treatment;
        public string specialization;
        public Disease()
        {
        }
        public Disease(string id,string name, string treatment,string specialization){
            this.id = id;
            this.name = name;
            this.treatment = treatment;
            this.specialization=specialization;
        }
    }
}
