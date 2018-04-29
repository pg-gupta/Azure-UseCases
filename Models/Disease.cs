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
        public string testSuggested;
        public string prefferedDiet;
        public Disease()
        {
        }
        public Disease(string id,string name, string treatment,string specialization, string testSuggested, string prefferedDiet){
            this.id = id;
            this.name = name;
            this.treatment = treatment;
            this.specialization=specialization;
            this.testSuggested = testSuggested;
            this.prefferedDiet = prefferedDiet;
        }
    }
}
