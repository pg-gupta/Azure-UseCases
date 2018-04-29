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
            DBConnect dBConnect = new DBConnect();
            string message = "Hello " + string.Join(", ", result.Entities.Select(i => i.Entity));
            message += " How are you?";
            await context.PostAsync(message);
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

            string message = "Sorry I could not get you!";
            await context.PostAsync(message);
            context.Wait(MessageReceived);

        }


        /// <summary>
        /// Send a generic help message if an intent without an intent handler is detected.
        /// </summary>
        /// <param name="context">Dialog context.</param>
        /// <param name="result">The result from LUIS.</param>
        [LuisIntent("Thanks")]
        public async Task Thanks(IDialogContext context, LuisResult result)
        {
            string message = "It was my pleasure serving you!";
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
            String message = "";
            var symptoms = result.Entities.Select(x => x.Entity).ToList();
            DBConnect dBConnect = new DBConnect();
            dBConnect.OpenConnection();
            if (symptoms != null)
            {
                diseases = dBConnect.getDiseases(symptoms);
                if (diseases != null && diseases.Count > 0)
                {
                    message += "You might be suffering below top 5 Diseases:";
                    int i = 1;
                    foreach (var disease in diseases)
                    {
                        message += "\n\n " + (i++) + " " + disease.name + "\n\n" + "\tTreatment: " + disease.treatment+
                                                                  "\n\n"+"\tSuggested Test: "+disease.testSuggested+
                                                                  "\n\n\tDiet Preferred: "+disease.prefferedDiet;
                    }
                }
                else
                    message = "Sorry! I could not find any disease to your symptoms";
                dBConnect.CloseConnection();

            }
            else
                message = "You did not tell me your symptoms";

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Medicine")]
        public async Task Medicine(IDialogContext context, LuisResult result)
        {
            string message = "";
            if (diseases == null)
                message = "Please tell me your symptoms first";
            else
            {

                DBConnect dBConnect = new DBConnect();
                dBConnect.OpenConnection();
                List<Medicine> medicines = dBConnect.getMedicines(diseases.Select((arg) => arg.name).Distinct().ToList());


                if (medicines != null && medicines.Count > 0)
                {
                    message += "You may take following top 5 medicines:";
                    int i = 1;
                    foreach (var medicine in medicines)
                    {
                        message += "\n\n " + (i++) + " " + medicine.name + "\n\n" + "\tDescription: " + medicine.description;
                    }

                    dBConnect.CloseConnection();
                }
                else
                    message = "Sorry, I could not find any medicines.";
            }
            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }

        [LuisIntent("Doctor")]
        public async Task Doctor(IDialogContext context, LuisResult result)
        {
            string message = "";
            if (diseases == null)
                message = "Please tell me your symptoms first.";
            else
            {
                DBConnect dBConnect = new DBConnect();
                dBConnect.OpenConnection();
                List<Doctor> doctors = dBConnect.getDoctors(diseases.Select((arg) => arg.specialization).Distinct().ToList());
                if (doctors != null && doctors.Count > 0)
                {
                    message += "You may consult below top 5 doctors:";
                    int i = 1;
                    foreach (var doc in doctors)
                    {
                        message += "\n\n " + (i++) + " " + doc.fname + " " + doc.lname + "\n\n\t Phone Number: " + doc.phonenumber +
                                                              "\n\n\t Address:" + doc.address;
                    }

                    dBConnect.CloseConnection();
                }
                else
                    message = "Sorry, I could not find any Doctors.";
            }

            await context.PostAsync(message);
            context.Wait(MessageReceived);
        }
    }
}