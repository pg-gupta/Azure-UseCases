using System;
namespace SimpleEchoBot.Models
{
    [Serializable]
    public class Doctor
    {
        public string fname;
        public string lname;
        public string age;
        public string sex;
        public string phonenumber;
        public string email;
        public string address;
        public string visithours;
     
        public Doctor()
        {
        }

        public Doctor(string fname,string lname,string age,string sex,string phonenumber,string email,string address,string visithours)
        {
            this.fname = fname;
            this.lname = lname;
            this.age = age;
            this.sex = sex;
            this.phonenumber = phonenumber;
            this.email = email;
            this.address = address;
            this.visithours = visithours;
        }
    }
}
