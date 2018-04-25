using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Bot.Builder.Luis.Models;
using System;
using System.Net.Http;
using Microsoft.Bot.Connector;
using SimpleEchoBot.Models;
using AdaptiveCards;
using System.Web;
using System.Web.Http;

using System.ComponentModel.DataAnnotations;
using System.Threading;
using Newtonsoft.Json;
using System.IO;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [LuisModel("117d2216-c908-4bac-95c2-fa550627be38", "ecb5f3dbe980438099ca7db16c5b25d9", domain: "westus.api.cognitive.microsoft.com")]
    [Serializable]
    public class SimpleNoteDialog : LuisDialog<object>, IDialog<object>
    {

        // Default note title
        public const string DefaultNoteTitle = "default";
        // Name of note title entity
        public const string Entity_Note_Title = "Note.Title";

        List<Disease> diseases = null;

        /// <summary>
        /// Send a generic help message if an intent without an intent handler is detected.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">The result from LUIS.</param>
        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            Activity reply = ((Activity)context.Activity).CreateReply();
            // read the json in from our file
            string path = Path.Combine(HttpRuntime.AppDomainAppPath, "/AdaptiveCards/MyCard.json");
            string json1;
            using (StreamReader r = new StreamReader("MyCard.json"))
            {
                 json1 = r.ReadToEnd();
                AdaptiveCards.AdaptiveCard card = JsonConvert.DeserializeObject<AdaptiveCards.AdaptiveCard>(json1);
                reply.Attachments.Add(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                });
               // List<Item> items = JsonConvert.DeserializeObject<List<Item>>(json);
            }


            //string path1 = "~\\AdaptiveCards\\MyCard.json";
           // string json = File.ReadAllText(HttpContext.Current.Request.MapPath(path));
            // use Newtonsofts JsonConvert to deserialized the json into a C# AdaptiveCard object
           // AdaptiveCards.AdaptiveCard card = JsonConvert.DeserializeObject<AdaptiveCards.AdaptiveCard>(json);
            // put the adaptive card as an attachment to the reply message
            //reply.Attachments.Add(new Attachment
            //{
            //    ContentType = AdaptiveCard.ContentType,
            //    Content = card
            //});
           // reply.Attachments.Add(attachment);

            await context.PostAsync(reply, CancellationToken.None);

            //DBConnect dBConnect = new DBConnect();
            //string message = "Hello " + string.Join(", ", result.Entities.Select(i => i.Entity));
            //message += " How are you?";
            //await context.PostAsync(message);
            context.Wait(MessageReceived);

        }

        /// <summary>
        /// Send a generic help message if an intent without an intent handler is detected.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">The result from LUIS.</param>
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult result)
        {

            DBConnect dBConnect = new DBConnect();
            string message = "Sorry I could not get you!";
            await context.PostAsync(message);
            context.Wait(MessageReceived);

        }

        [LuisIntent("Sick")]
        public async Task Sick(IDialogContext context, LuisResult result)
        {

            string message = "Do not worry! I can help you.\n Tell me how are you feeling or tell me your symptoms?";
            var symptoms = result.Entities.Select(x => x.Entity);
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Symptoms")]
        public async Task Symptoms(IDialogContext context, LuisResult result)
        {
            Activity reply = ((Activity)context.Activity).CreateReply();

            var symptoms = result.Entities.Select(x => x.Entity).ToList();
            DBConnect dBConnect = new DBConnect();
            dBConnect.OpenConnection();
            diseases = dBConnect.getDiseases(symptoms);
            if (diseases!=null && diseases.Count>0)
            {

                HeroCard card = new HeroCard
                {
                    Subtitle = "You might be suffering from below top 5 Diseases: ",
                    Images = new List<CardImage> { new CardImage("https://robodoc.blob.core.windows.net/images/disease.jpeg") },
                };
                reply.Attachments.Add(card.ToAttachment());

                foreach (var disease in diseases)
                {
                    HeroCard entityCard = new HeroCard
                    {
                        Title = disease.name,
                        Subtitle = disease.treatment,

                    };
                    reply.Attachments.Add(entityCard.ToAttachment());
                }
            }
            else
                reply.Text = "Sorry! I could not find any disease to your symptoms";
            dBConnect.CloseConnection();
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Medicine")]
        public async Task Medicine(IDialogContext context, LuisResult result)
        {
            Activity reply = ((Activity)context.Activity).CreateReply();
            if (diseases == null)
                reply.Text = "Please tell me your symptoms first";
            else
            {

                DBConnect dBConnect = new DBConnect();
                dBConnect.OpenConnection();
                List<Medicine> medicines = dBConnect.getMedicines(diseases.Select((arg) => arg.name).Distinct().ToList());

                if (medicines != null && medicines.Count>0)
                {
                    HeroCard card = new HeroCard
                    {
                        Subtitle = "\nYou may take following top 5 medicines: ",
                        Images = new List<CardImage> { new CardImage("https://robodoc.blob.core.windows.net/images/medicine.jpg") },
                    };
                    reply.Attachments.Add(card.ToAttachment());

                    foreach (var medicine in medicines)
                    {
                        HeroCard entityCard = new HeroCard
                        {
                            Title = medicine.name,
                            Subtitle = medicine.description,

                        };
                        reply.Attachments.Add(entityCard.ToAttachment());
                    }
                    dBConnect.CloseConnection();
                }
                else
                    reply.Text = "Sorry, I could not find any medicines.";
            }
            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Doctor")]
        public async Task Doctor(IDialogContext context, LuisResult result)
        {
            Activity reply = ((Activity)context.Activity).CreateReply();
            if (diseases == null)
                reply.Text = "Please tell me your symptoms first.";
            else
            {
                DBConnect dBConnect = new DBConnect();
                dBConnect.OpenConnection();
                List<Doctor> doctors = dBConnect.getDoctors(diseases.Select((arg) => arg.specialization).Distinct().ToList());
                if (doctors != null && doctors.Count>0)
                {
                    HeroCard card = new HeroCard
                    {
                        Subtitle = "\nYou may take consultation from following top 5 Doctors: ",
                        Images = new List<CardImage> { new CardImage("https://robodoc.blob.core.windows.net/images/doctor.jpg") },
                    };
                    reply.Attachments.Add(card.ToAttachment());

                    foreach (var doctor in doctors)
                    {
                        HeroCard entityCard = new HeroCard
                        {
                            Title = doctor.fname + " " + doctor.lname,
                            Subtitle = doctor.age + "\n" + doctor.sex + "\n" + doctor.phonenumber + "\n" + doctor.visithours + "\n" + doctor.address,

                        };
                        reply.Attachments.Add(entityCard.ToAttachment());
                    }
                    dBConnect.CloseConnection();
                }
                else
                    reply.Text = "Sorry, I could not find any Doctors.";
            }

            await context.PostAsync(reply);
            context.Wait(MessageReceived);
        }
    }
}